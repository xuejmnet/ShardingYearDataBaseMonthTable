using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingYearAndMonth.Entities;

namespace ShardingYearAndMonth.DbContexts;

public class TestDbContext:AbstractShardingDbContext,IShardingTableDbContext
{
    public DbSet<OrderItem> OrderItems { get; set; }
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public IRouteTail RouteTail { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderItem>()
            .HasKey(o => o.Id);
        modelBuilder.Entity<OrderItem>()
            .ToTable(nameof(OrderItem));
    }
}