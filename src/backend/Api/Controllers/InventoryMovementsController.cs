using Api.DTOs;
using Api.DTOs.InventoryMovements;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/inventory-movements")]
[Authorize]
public sealed class InventoryMovementsController(
    IInventoryMovementService inventoryMovementService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<InventoryMovementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false,
        CancellationToken cancellationToken = default)
    {
        var result = await inventoryMovementService.GetPagedAsync(
            page,
            pageSize,
            search,
            sortBy,
            descending,
            cancellationToken);

        return Ok(ApiResponse<PagedResultDto<InventoryMovementDto>>.Ok(result));
    }

    [HttpGet("{inventoryMovementId:long}", Name = "GetInventoryMovementById")]
    [ProducesResponseType(typeof(ApiResponse<InventoryMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetByIdAsync(
        [FromRoute] long inventoryMovementId,
        CancellationToken cancellationToken)
    {
        var inventoryMovement = await inventoryMovementService.GetByIdAsync(
            inventoryMovementId,
            cancellationToken);

        if (inventoryMovement is null)
        {
            return NotFound(ApiResponse<object>.Fail("Inventory movement was not found."));
        }

        return Ok(ApiResponse<InventoryMovementDto>.Ok(inventoryMovement));
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<InventoryMovementDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<InventoryMovementDto>>> CreateAsync(
        [FromBody] CreateInventoryMovementRequest request,
        CancellationToken cancellationToken)
    {
        var createdMovement = await inventoryMovementService.CreateAsync(
            new CreateInventoryMovementDto(
                request.ProductId,
                request.MovementType,
                request.Quantity,
                request.Reason),
            cancellationToken);

        return CreatedAtRoute(
            "GetInventoryMovementById",
            new { inventoryMovementId = createdMovement.InventoryMovementId },
            ApiResponse<InventoryMovementDto>.Ok(
                createdMovement,
                "Inventory movement created successfully."));
    }
}