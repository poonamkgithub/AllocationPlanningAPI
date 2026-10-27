using System.ComponentModel.DataAnnotations;

namespace AllocationPlanningApi.Models;

public class StockAllocation
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string RequestNumber { get; set; } = "";
    [Required, MaxLength(50)] public string ProductCode { get; set; } = "";
    [Required, MaxLength(50)] public string LocationCode { get; set; } = "";
    [Range(1, int.MaxValue)] public int AllocatedQuantity { get; set; }
    public DateTime AllocationDateUtc { get; set; } = DateTime.UtcNow;
    [MaxLength(30)] public string Status { get; set; } = "Allocated";
}
