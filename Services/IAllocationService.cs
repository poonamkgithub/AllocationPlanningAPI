using AllocationPlanningApi.DTOs;
using AllocationPlanningApi.Models;

namespace AllocationPlanningApi.Services;

public interface IAllocationService
{
    Task<AllocationRequest> CreateRequestAsync(CreateAllocationRequest request);
    Task<AllocationResult> AllocateAsync(int requestId);
    Task<IReadOnlyList<AllocationRequest>> GetRequestsAsync(string? status);
}
