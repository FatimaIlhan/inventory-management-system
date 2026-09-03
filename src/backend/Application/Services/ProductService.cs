using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Validators;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class ProductService(
    IProductRepository productRepository,
    TimeProvider timeProvider) : IProductService
{
    public async Task<PagedResultDto<ProductDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        var validatedPage = ValidatePage(page);
        var validatedPageSize = ValidatePageSize(pageSize);

        var (items, totalCount) = await productRepository.GetPagedAsync(
            validatedPage,
            validatedPageSize,
            search,
            sortBy,
            descending,
            cancellationToken);

        return new PagedResultDto<ProductDto>(
            items.Select(ToDto).ToList(),
            validatedPage,
            validatedPageSize,
            totalCount);
    }

    public async Task<ProductDto> GetByIdAsync(long productId, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product was not found.");
        }

        return ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto, CancellationToken cancellationToken)
    {
        var normalizedSku = ProductValidationRules.ValidateAndNormalizeSku(createProductDto.Sku);
        var normalizedName = ProductValidationRules.ValidateAndNormalizeName(createProductDto.Name);
        var normalizedDescription = ProductValidationRules.ValidateAndNormalizeDescription(createProductDto.Description);
        var validatedUnitPrice = ProductValidationRules.ValidateUnitPrice(createProductDto.UnitPrice);
        var validatedCurrentStock = ProductValidationRules.ValidateCurrentStock(createProductDto.CurrentStock);
        var validatedReorderLevel = ProductValidationRules.ValidateReorderLevel(createProductDto.ReorderLevel);
        var validatedStatus = ProductValidationRules.ValidateStatus(createProductDto.Status);
        var validatedCategoryId = ProductValidationRules.ValidateCategoryId(createProductDto.CategoryId);
        var validatedSupplierId = ProductValidationRules.ValidateSupplierId(createProductDto.SupplierId);

        await EnsureSkuIsUniqueAsync(normalizedSku, null, cancellationToken);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        var product = new Product
        {
            Sku = normalizedSku,
            Name = normalizedName,
            Description = normalizedDescription,
            UnitPrice = validatedUnitPrice,
            CurrentStock = validatedCurrentStock,
            ReorderLevel = validatedReorderLevel,
            Status = validatedStatus,
            CategoryId = validatedCategoryId,
            SupplierId = validatedSupplierId,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = null
        };

        var createdProduct = await productRepository.CreateAsync(product, cancellationToken);

        return ToDto(createdProduct);
    }

    public async Task<ProductDto> UpdateAsync(long productId, UpdateProductDto updateProductDto, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product was not found.");
        }

        var normalizedSku = ProductValidationRules.ValidateAndNormalizeSku(updateProductDto.Sku);
        var normalizedName = ProductValidationRules.ValidateAndNormalizeName(updateProductDto.Name);
        var normalizedDescription = ProductValidationRules.ValidateAndNormalizeDescription(updateProductDto.Description);
        var validatedUnitPrice = ProductValidationRules.ValidateUnitPrice(updateProductDto.UnitPrice);
        var validatedReorderLevel = ProductValidationRules.ValidateReorderLevel(updateProductDto.ReorderLevel);
        var validatedStatus = ProductValidationRules.ValidateStatus(updateProductDto.Status);
        var validatedCategoryId = ProductValidationRules.ValidateCategoryId(updateProductDto.CategoryId);
        var validatedSupplierId = ProductValidationRules.ValidateSupplierId(updateProductDto.SupplierId);

        await EnsureSkuIsUniqueAsync(normalizedSku, productId, cancellationToken);

        product.Sku = normalizedSku;
        product.Name = normalizedName;
        product.Description = normalizedDescription;
        product.UnitPrice = validatedUnitPrice;
        product.ReorderLevel = validatedReorderLevel;
        product.Status = validatedStatus;
        product.CategoryId = validatedCategoryId;
        product.SupplierId = validatedSupplierId;
        product.UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        await productRepository.UpdateAsync(product, cancellationToken);

        return ToDto(product);
    }

    public async Task DeleteAsync(long productId, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product was not found.");
        }

        await productRepository.DeleteAsync(productId, cancellationToken);
    }

    private async Task EnsureSkuIsUniqueAsync(string sku, long? excludedProductId, CancellationToken cancellationToken)
    {
        var isSkuTaken = await productRepository.ExistsBySkuAsync(sku, excludedProductId, cancellationToken);

        if (isSkuTaken)
        {
            throw new ConflictException("Product SKU must be unique.");
        }
    }

    private static int ValidatePage(int page)
    {
        if (page < 1)
        {
            throw new AppValidationException("Page must be greater than or equal to 1.");
        }

        return page;
    }

    private static int ValidatePageSize(int pageSize)
    {
        if (pageSize < 1 || pageSize > 100)
        {
            throw new AppValidationException("Page size must be between 1 and 100.");
        }

        return pageSize;
    }

    private static ProductDto ToDto(Product product) =>
        new(
            product.ProductId,
            product.Sku,
            product.Name,
            product.Description,
            product.UnitPrice,
            product.CurrentStock,
            product.ReorderLevel,
            product.Status,
            product.CategoryId,
            product.SupplierId,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
}
