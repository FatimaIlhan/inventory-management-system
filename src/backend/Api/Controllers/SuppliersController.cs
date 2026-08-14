using Api.DTOs;
using Api.DTOs.Suppliers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public sealed class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResultDto<SupplierDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await supplierService.GetPagedAsync(
            page,
            pageSize,
            search,
            cancellationToken);

        return Ok(ApiResponse<PagedResultDto<SupplierDto>>.Ok(result));
    }

    [HttpGet("{supplierId:long}", Name ="GetSupplierById")]
    [ProducesResponseType(
        typeof(ApiResponse<SupplierDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult> GetByIdAsync(
        [FromRoute] long supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await supplierService.GetByIdAsync(
            supplierId,
            cancellationToken);

        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPost("create-supplier")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(
        typeof(ApiResponse<SupplierDto>),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateAsync(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var createdSupplier = await supplierService.CreateAsync(
            new CreateSupplierDto(
                request.CompanyName,
                request.ContactPerson,
                request.Phone,
                request.Email,
                request.Address),
            cancellationToken);

        return CreatedAtRoute(
            "GetSupplierById",
            new { supplierId = createdSupplier.SupplierId },
            ApiResponse<SupplierDto>.Ok(
                createdSupplier,
                "Supplier created successfully."));
    }

    [HttpPut("{supplierId:long}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(
        typeof(ApiResponse<SupplierDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateAsync(
        [FromRoute] long supplierId,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var updatedSupplier = await supplierService.UpdateAsync(
            supplierId,
            new UpdateSupplierDto(
                request.CompanyName,
                request.ContactPerson,
                request.Phone,
                request.Email,
                request.Address),
            cancellationToken);

        return Ok(
            ApiResponse<SupplierDto>.Ok(
                updatedSupplier,
                "Supplier updated successfully."));
    }

    [HttpDelete("{supplierId:long}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteAsync(
        [FromRoute] long supplierId,
        CancellationToken cancellationToken)
    {
        await supplierService.DeleteAsync(
            supplierId,
            cancellationToken);

        return NoContent();
    }
}