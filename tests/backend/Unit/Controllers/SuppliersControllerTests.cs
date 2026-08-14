using Api.Controllers;
using Api.DTOs;
using Api.DTOs.Suppliers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Unit.Controllers;

public sealed class SuppliersControllerTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedAtRoute_WhenSupplierIsCreated()
    {
        var createdSupplier = new SupplierDto(
            42,
            "Cape Town Office Supplies",
            "Sarah Smith",
            "+27215559867",
            "sarah@capetownsupplies.co.za",
            "29 Main Road, Cape Town",
            new DateTime(2026, 08, 09, 10, 11, 36, DateTimeKind.Utc),
            null);

        var controller = new SuppliersController(new FakeSupplierService(createdSupplier));

        var result = await controller.CreateAsync(
            new CreateSupplierRequest(
                "Cape Town Office Supplies",
                "Sarah Smith",
                "+27215559867",
                "sarah@capetownsupplies.co.za",
                "29 Main Road, Cape Town"),
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);

        Assert.Equal("GetSupplierById", createdResult.RouteName);
        Assert.Equal(42L, Convert.ToInt64(createdResult.RouteValues!["supplierId"]));

        var payload = Assert.IsType<ApiResponse<SupplierDto>>(createdResult.Value);
        Assert.True(payload.Success);
        Assert.Equal("Supplier created successfully.", payload.Message);
        Assert.Equal(42, payload.Data!.SupplierId);
    }

    private sealed class FakeSupplierService(SupplierDto createdSupplier) : ISupplierService
    {
        public Task<PagedResultDto<SupplierDto>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<SupplierDto> GetByIdAsync(long supplierId, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<SupplierDto> CreateAsync(CreateSupplierDto createSupplierDto, CancellationToken cancellationToken)
            => Task.FromResult(createdSupplier);

        public Task<SupplierDto> UpdateAsync(
            long supplierId,
            UpdateSupplierDto updateSupplierDto,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task DeleteAsync(long supplierId, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
