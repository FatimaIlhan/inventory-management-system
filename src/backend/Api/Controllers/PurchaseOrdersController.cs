using Api.DTOs;
using Api.DTOs.PurchaseOrders;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public sealed class PurchaseOrdersController(
    IPurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<PurchaseOrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false,
        CancellationToken cancellationToken = default)
    {
        var result = await purchaseOrderService.GetPagedAsync(
            page,
            pageSize,
            search,
            sortBy,
            descending,
            cancellationToken);

        return Ok(ApiResponse<PagedResultDto<PurchaseOrderDto>>.Ok(result));
    }

    [HttpGet("{purchaseOrderId:long}", Name = "GetPurchaseOrderById")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetByIdAsync(
        [FromRoute] long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderService.GetByIdAsync(
            purchaseOrderId,
            cancellationToken);

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(purchaseOrder));
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreateAsync(
        [FromBody] CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var createdPurchaseOrder = await purchaseOrderService.CreateAsync(
            new CreatePurchaseOrderDto(
                request.OrderNumber,
                request.SupplierId,
                request.Items.Select(item => new CreatePurchaseOrderItemDto(
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice)).ToList()),
            cancellationToken);

        return CreatedAtRoute(
            "GetPurchaseOrderById",
            new { purchaseOrderId = createdPurchaseOrder.PurchaseOrderId },
            ApiResponse<PurchaseOrderDto>.Ok(
                createdPurchaseOrder,
                "Purchase order created successfully."));
    }

    [HttpPut("{purchaseOrderId:long}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateAsync(
        [FromRoute] long purchaseOrderId,
        [FromBody] UpdatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var updatedPurchaseOrder = await purchaseOrderService.UpdateAsync(
            purchaseOrderId,
            new UpdatePurchaseOrderDto(
                request.OrderNumber,
                request.SupplierId,
                request.Items.Select(item => new CreatePurchaseOrderItemDto(
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice)).ToList()),
            cancellationToken);

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(
            updatedPurchaseOrder,
            "Purchase order updated successfully."));
    }

    [HttpDelete("{purchaseOrderId:long}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteAsync(
        [FromRoute] long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        await purchaseOrderService.DeleteAsync(purchaseOrderId, cancellationToken);

        return NoContent();
    }

    [HttpPost("{purchaseOrderId:long}/submit")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> SubmitAsync(
        [FromRoute] long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderService.SubmitAsync(purchaseOrderId, cancellationToken);

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(
            purchaseOrder,
            "Purchase order submitted successfully."));
    }

    [HttpPost("{purchaseOrderId:long}/receive")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ReceiveAsync(
        [FromRoute] long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderService.ReceiveAsync(purchaseOrderId, cancellationToken);

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(
            purchaseOrder,
            "Purchase order received successfully."));
    }
}