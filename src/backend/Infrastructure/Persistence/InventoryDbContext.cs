using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
