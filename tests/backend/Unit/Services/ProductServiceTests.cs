using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Unit.Services;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task CreateAndRead_ShouldReturnReferenceNamesAndPreserveIds()
    {
        await using var dbContext = await CreateDbContextAsync();
        var service = CreateService(dbContext);

        var created = await service.CreateAsync(
            new CreateProductDto("LAPTOP-15", "Business Laptop 15-inch", null,
                800m, 10, 2, ProductStatus.Active, 201, 301),
            CancellationToken.None);

        AssertProductDetails(created, 201, "Computers", 301, "Business Electronics");

        dbContext.ChangeTracker.Clear();
        var detail = await service.GetByIdAsync(created.ProductId, CancellationToken.None);
        AssertProductDetails(detail, 201, "Computers", 301, "Business Electronics");

        var page = await service.GetPagedAsync(1, 10, "Laptop", null, false, CancellationToken.None);
        Assert.Equal(1, page.TotalCount);
        AssertProductDetails(Assert.Single(page.Items), 201, "Computers", 301, "Business Electronics");
    }

    [Fact]
    public async Task Update_ShouldReturnNewReferenceNamesWithoutChangingStockOrRelatedRecords()
    {
        await using var dbContext = await CreateDbContextAsync();
        var service = CreateService(dbContext);
        var created = await service.CreateAsync(
            new CreateProductDto("LAPTOP-15", "Business Laptop 15-inch", null,
                800m, 10, 2, ProductStatus.Active, 201, 301),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var updated = await service.UpdateAsync(created.ProductId,
            new UpdateProductDto("LAPTOP-15", "Business Laptop 15-inch", null,
                900m, 3, ProductStatus.Active, 202, 302),
            CancellationToken.None);

        AssertProductDetails(updated, 202, "Office Equipment", 302, "Office Supplies");
        Assert.Equal(10, updated.CurrentStock);
        Assert.Equal(900m, updated.UnitPrice);
        Assert.NotNull(updated.UpdatedAtUtc);
        dbContext.ChangeTracker.Clear();
        Assert.Equal("Computers", (await dbContext.Categories.FindAsync(201L))!.Name);
        Assert.Equal("Business Electronics", (await dbContext.Suppliers.FindAsync(301L))!.CompanyName);
        Assert.Equal(2, await dbContext.Categories.CountAsync());
        Assert.Equal(2, await dbContext.Suppliers.CountAsync());
    }

    [Fact]
    public async Task RepositoryUpdate_ShouldNotPersistLoadedReferenceChanges()
    {
        await using var dbContext = await CreateDbContextAsync();
        var service = CreateService(dbContext);
        var created = await service.CreateAsync(
            new CreateProductDto("LAPTOP-15", "Business Laptop 15-inch", null,
                800m, 10, 2, ProductStatus.Active, 201, 301),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();
        var repository = new ProductRepository(dbContext);
        var product = (await repository.GetByIdAsync(created.ProductId, CancellationToken.None))!;
        product.Category.Name = "Should not be saved";
        product.Supplier.CompanyName = "Should not be saved";
        product.UnitPrice = 900m;

        await repository.UpdateAsync(product, CancellationToken.None);

        dbContext.ChangeTracker.Clear();
        var saved = (await repository.GetByIdAsync(created.ProductId, CancellationToken.None))!;
        Assert.Equal(900m, saved.UnitPrice);
        Assert.Equal("Computers", saved.Category.Name);
        Assert.Equal("Business Electronics", saved.Supplier.CompanyName);
    }

    [Theory]
    [InlineData("Business Laptop 15-inch")]
    [InlineData("  Business Laptop 15-inch  ")]
    public void ValidateName_ShouldAcceptAndTrimNamesWithNumbers(string name)
    {
        Assert.Equal("Business Laptop 15-inch", ProductValidationRules.ValidateAndNormalizeName(name));
    }

    [Fact]
    public void ValidateName_ShouldKeepRequiredAndLengthLimits()
    {
        Assert.Throws<AppValidationException>(() => ProductValidationRules.ValidateAndNormalizeName(" "));
        Assert.Throws<AppValidationException>(() => ProductValidationRules.ValidateAndNormalizeName(new string('A', 101)));
    }

    private static void AssertProductDetails(
        ProductDto product, long categoryId, string categoryName, long supplierId, string supplierName)
    {
        Assert.Equal("Business Laptop 15-inch", product.Name);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.Equal(categoryName, product.CategoryName);
        Assert.Equal(supplierId, product.SupplierId);
        Assert.Equal(supplierName, product.SupplierName);
    }

    private static ProductService CreateService(InventoryDbContext dbContext) =>
        new(new ProductRepository(dbContext), new StubAuditLogService(), TimeProvider.System);

    private static async Task<InventoryDbContext> CreateDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new InventoryDbContext(options);
        dbContext.Categories.AddRange(
            new Category { CategoryId = 201, Name = "Computers" },
            new Category { CategoryId = 202, Name = "Office Equipment" });
        dbContext.Suppliers.AddRange(
            new Supplier { SupplierId = 301, CompanyName = "Business Electronics" },
            new Supplier { SupplierId = 302, CompanyName = "Office Supplies" });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        return dbContext;
    }

    private sealed class StubAuditLogService : IAuditLogService
    {
        public Task RecordAsync(string entityType, long? entityId, string action, string description,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<PagedResultDto<AuditLogDto>> GetPagedAsync(
            int page, int pageSize, string? search, string? entityType, string? action,
            long? userId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
