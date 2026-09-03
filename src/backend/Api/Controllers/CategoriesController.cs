using Api.DTOs;
using Api.DTOs.Categories;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await categoryService.GetPagedAsync(page, pageSize, search, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<CategoryDto>>.Ok(result));
    }

    [HttpGet("{categoryId:long}",Name = "GetCategoryById")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetByIdAsync([FromRoute] long categoryId, CancellationToken cancellationToken)
    {
        var category = await categoryService.GetByIdAsync(categoryId, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(category));
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateAsync(
    [FromBody] CreateCategoryRequest request,
    CancellationToken cancellationToken)
    {
        var createdCategory = await categoryService.CreateAsync(
            new CreateCategoryDto(
                request.Name,
                request.Description),
            cancellationToken);

        return CreatedAtRoute(
            "GetCategoryById",
            new { categoryId = createdCategory.Id },
            ApiResponse<CategoryDto>.Ok(
                createdCategory,
                "Category created successfully."));
    }

    [HttpPut("{categoryId:long}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateAsync(
        [FromRoute] long categoryId,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var updatedCategory = await categoryService.UpdateAsync(
            categoryId,
            new UpdateCategoryDto(request.Name, request.Description),
            cancellationToken);

        return Ok(ApiResponse<CategoryDto>.Ok(updatedCategory, "Category updated successfully."));
    }

    [HttpDelete("{categoryId:long}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteAsync([FromRoute] long categoryId, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(categoryId, cancellationToken);
        return NoContent();
    }
}
