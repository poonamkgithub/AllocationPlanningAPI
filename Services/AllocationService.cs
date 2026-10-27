using AllocationPlanningApi.Data;
using AllocationPlanningApi.DTOs;
using AllocationPlanningApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocationPlanningApi.Services;

public class AllocationService : IAllocationService
{
    private readonly AllocationDbContext _db;
    public AllocationService(AllocationDbContext db) => _db = db;

    public async Task<AllocationRequest> CreateRequestAsync(CreateAllocationRequest request)
    {
        if (await _db.AllocationRequests.AnyAsync(x => x.RequestNumber == request.RequestNumber.Trim()))
            throw new InvalidOperationException("Request number already exists.");

        var entity = new AllocationRequest
        {
            RequestNumber = request.RequestNumber.Trim(),
            ProductCode = request.ProductCode.Trim(),
            RequestedQuantity = request.RequestedQuantity,
            RequiredDate = request.RequiredDate,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority.Trim(),
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.AllocationRequests.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<IReadOnlyList<AllocationRequest>> GetRequestsAsync(string? status)
    {
        var query = _db.AllocationRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);
        return await query.OrderByDescending(x => x.Priority == "High")
            .ThenBy(x => x.RequiredDate).ToListAsync();
    }

    public async Task<AllocationResult> AllocateAsync(int requestId)
    {
        // Serializable transaction prevents two concurrent requests from spending the same stock.
        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        var request = await _db.AllocationRequests.FirstOrDefaultAsync(x => x.Id == requestId);
        if (request is null) throw new KeyNotFoundException("Allocation request not found.");
        if (request.Status == "Allocated" || request.Status == "PartiallyAllocated")
            throw new InvalidOperationException("This request has already been processed.");

        var stocks = await _db.StockBalances
            .Where(x => x.ProductCode == request.ProductCode)
            .OrderBy(x => x.LocationCode)
            .ToListAsync();

        var priorAllocations = await _db.StockAllocations
            .Where(x => x.ProductCode == request.ProductCode && x.Status == "Allocated")
            .GroupBy(x => x.LocationCode)
            .Select(g => new { LocationCode = g.Key, Quantity = g.Sum(x => x.AllocatedQuantity) })
            .ToListAsync();

        var priorByLocation = priorAllocations.ToDictionary(x => x.LocationCode, x => x.Quantity);
        int remaining = request.RequestedQuantity;
        var locations = new List<LocationAllocation>();

        // High priority requests consume eligible stock first; within a request, locations are ordered by code.
        foreach (var stock in stocks)
        {
            if (remaining <= 0) break;
            int previouslyAllocated = priorByLocation.GetValueOrDefault(stock.LocationCode, 0);
            int available = Math.Max(0, stock.OnHandQuantity - stock.ReservedQuantity
                - stock.SafetyStockQuantity - previouslyAllocated);
            if (available <= 0) continue;

            int quantity = Math.Min(remaining, available);
            _db.StockAllocations.Add(new StockAllocation
            {
                RequestNumber = request.RequestNumber,
                ProductCode = request.ProductCode,
                LocationCode = stock.LocationCode,
                AllocatedQuantity = quantity,
                AllocationDateUtc = DateTime.UtcNow,
                Status = "Allocated"
            });
            locations.Add(new LocationAllocation
            {
                LocationCode = stock.LocationCode,
                AvailableBeforeAllocation = available,
                AllocatedQuantity = quantity
            });
            remaining -= quantity;
        }

        int allocated = request.RequestedQuantity - remaining;
        request.Status = allocated == request.RequestedQuantity
            ? "Allocated"
            : allocated > 0 ? "PartiallyAllocated" : "Unallocated";

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new AllocationResult
        {
            RequestNumber = request.RequestNumber,
            ProductCode = request.ProductCode,
            RequestedQuantity = request.RequestedQuantity,
            AllocatedQuantity = allocated,
            UnallocatedQuantity = remaining,
            Status = request.Status,
            Locations = locations,
            Message = remaining == 0
                ? "Requested quantity allocated successfully."
                : allocated > 0
                    ? $"Partially allocated. {remaining} unit(s) remain unallocated."
                    : "No eligible stock available to allocate."
        };
    }
}
