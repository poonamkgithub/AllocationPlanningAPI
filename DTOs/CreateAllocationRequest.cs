using System.ComponentModel.DataAnnotations;

namespace AllocationPlanningApi.DTOs;

public class CreateAllocationRequest
{
    [Required] public string RequestNumber { get; set; } = "";
    [Required] public string ProductCode { get; set; } = "";
    [Range(1, int.MaxValue)] public int RequestedQuantity { get; set; }
    public DateTime RequiredDate { get; set; }
    public string Priority { get; set; } = "Normal";
}
