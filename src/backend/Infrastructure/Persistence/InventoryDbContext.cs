using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : IdentityDbContext<User, Role, long>(options)
{
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
	public DbSet<Category> Categories => Set<Category>();
	public DbSet<Supplier> Suppliers => Set<Supplier>();
	public DbSet<Product> Products => Set<Product>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
	}
}
