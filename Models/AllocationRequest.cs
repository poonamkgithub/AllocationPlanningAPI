using System.ComponentModel.DataAnnotations;

namespace AllocationPlanningApi.Models;

public class AllocationRequest
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string RequestNumber { get; set; } = "";
    [Required, MaxLength(50)] public string ProductCode { get; set; } = "";
    [Range(1, int.MaxValue)] public int RequestedQuantity { get; set; }
    public DateTime RequiredDate { get; set; }
    [MaxLength(20)] public string Priority { get; set; } = "Normal";
    [MaxLength(30)] public string Status { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
