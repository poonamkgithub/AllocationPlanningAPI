namespace AllocationPlanningApi.DTOs;

public class AllocationResult
{
    public string RequestNumber { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public int RequestedQuantity { get; set; }
    public int AllocatedQuantity { get; set; }
    public int UnallocatedQuantity { get; set; }
    public string Status { get; set; } = "";
    public List<LocationAllocation> Locations { get; set; } = new();
    public string Message { get; set; } = "";
}

public class LocationAllocation
{
    public string LocationCode { get; set; } = "";
    public int AvailableBeforeAllocation { get; set; }
    public int AllocatedQuantity { get; set; }
}
