using Api.DTOs;
using Api.DTOs.Products;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false,
        CancellationToken cancellationToken = default)
    {
        var result = await productService.GetPagedAsync(
            page,
            pageSize,
            search,
            sortBy,
            descending,
            cancellationToken);

        return Ok(ApiResponse<PagedResultDto<ProductDto>>.Ok(result));
    }

    [HttpGet("{productId:long}", Name = "GetProductById")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetByIdAsync([FromRoute] long productId, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(productId, cancellationToken);
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateAsync(
        [FromBody] CreateProductsRequest request,
        CancellationToken cancellationToken)
    {
        var createdProduct = await productService.CreateAsync(
            new CreateProductDto(
                request.Sku,
                request.Name,
                request.Description,
                request.UnitPrice,
                request.CurrentStock,
                request.ReorderLevel,
                request.Status,
                request.CategoryId,
                request.SupplierId),
            cancellationToken);

        return CreatedAtRoute(
            "GetProductById",
            new { productId = createdProduct.ProductId },
            ApiResponse<ProductDto>.Ok(createdProduct, "Product created successfully."));
    }

    [HttpPut("{productId:long}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateAsync(
        [FromRoute] long productId,
        [FromBody] UpdateProductsRequest request,
        CancellationToken cancellationToken)
    {
        var updatedProduct = await productService.UpdateAsync(
            productId,
            new UpdateProductDto(
                request.Sku,
                request.Name,
                request.Description,
                request.UnitPrice,
                request.ReorderLevel,
                request.Status,
                request.CategoryId,
                request.SupplierId),
            cancellationToken);

        return Ok(ApiResponse<ProductDto>.Ok(updatedProduct, "Product updated successfully."));
    }

    [HttpDelete("{productId:long}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteAsync([FromRoute] long productId, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(productId, cancellationToken);
        return NoContent();
    }
}
