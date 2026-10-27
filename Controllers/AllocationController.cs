using AllocationPlanningApi.Data;
using AllocationPlanningApi.DTOs;
using AllocationPlanningApi.Models;
using AllocationPlanningApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllocationPlanningApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AllocationController : ControllerBase
{
    private readonly IAllocationService _service;
    private readonly AllocationDbContext _db;

    public AllocationController(IAllocationService service, AllocationDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests([FromQuery] string? status) =>
        Ok(await _service.GetRequestsAsync(status));

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateAllocationRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var result = await _service.CreateRequestAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("requests/{id:int}/allocate")]
    public async Task<IActionResult> Allocate(int id)
    {
        try { return Ok(await _service.AllocateAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock() =>
        Ok(await _db.StockBalances.AsNoTracking().OrderBy(x => x.ProductCode)
            .ThenBy(x => x.LocationCode).ToListAsync());

    [HttpPost("stock")]
    public async Task<IActionResult> UpsertStock([FromBody] StockBalance input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (input.ReservedQuantity > input.OnHandQuantity)
            return BadRequest(new { message = "Reserved quantity cannot exceed on-hand quantity." });

        var existing = await _db.StockBalances.FirstOrDefaultAsync(x =>
            x.ProductCode == input.ProductCode && x.LocationCode == input.LocationCode);
        if (existing == null)
        {
            input.UpdatedAtUtc = DateTime.UtcNow;
            _db.StockBalances.Add(input);
        }
        else
        {
            existing.OnHandQuantity = input.OnHandQuantity;
            existing.ReservedQuantity = input.ReservedQuantity;
            existing.SafetyStockQuantity = input.SafetyStockQuantity;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(existing ?? input);
    }

    [HttpGet("allocations")]
    public async Task<IActionResult> GetAllocations() =>
        Ok(await _db.StockAllocations.AsNoTracking().OrderByDescending(x => x.AllocationDateUtc).ToListAsync());
}
