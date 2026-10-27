using System.ComponentModel.DataAnnotations;

namespace AllocationPlanningApi.Models;

public class StockBalance
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string ProductCode { get; set; } = "";
    [Required, MaxLength(50)] public string LocationCode { get; set; } = "";
    [Range(0, int.MaxValue)] public int OnHandQuantity { get; set; }
    [Range(0, int.MaxValue)] public int ReservedQuantity { get; set; }
    [Range(0, int.MaxValue)] public int SafetyStockQuantity { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
