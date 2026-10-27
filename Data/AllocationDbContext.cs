using AllocationPlanningApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocationPlanningApi.Data;

public class AllocationDbContext : DbContext
{
    public AllocationDbContext(DbContextOptions<AllocationDbContext> options) : base(options) { }

    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<AllocationRequest> AllocationRequests => Set<AllocationRequest>();
    public DbSet<StockAllocation> StockAllocations => Set<StockAllocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockBalance>()
            .HasIndex(x => new { x.ProductCode, x.LocationCode }).IsUnique();
        modelBuilder.Entity<AllocationRequest>()
            .HasIndex(x => x.RequestNumber).IsUnique();
        modelBuilder.Entity<StockAllocation>()
            .HasIndex(x => new { x.RequestNumber, x.ProductCode, x.LocationCode }).IsUnique();
        base.OnModelCreating(modelBuilder);
    }
}
