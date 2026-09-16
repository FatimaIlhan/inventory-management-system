using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Validators;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class PurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    ISupplierRepository supplierRepository,
    IProductRepository productRepository,
    IInventoryMovementService inventoryMovementService,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IPurchaseOrderService
{
    public async Task<PagedResultDto<PurchaseOrderDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        var validatedPage = PurchaseOrderValidationRules.ValidatePage(page);
        var validatedPageSize = PurchaseOrderValidationRules.ValidatePageSize(pageSize);

        var (items, totalCount) = await purchaseOrderRepository.GetPagedAsync(
            validatedPage,
            validatedPageSize,
            search,
            sortBy,
            descending,
            cancellationToken);

        return new PagedResultDto<PurchaseOrderDto>(
            items.Select(ToDto).ToList(),
            validatedPage,
            validatedPageSize,
            totalCount);
    }

    public async Task<PurchaseOrderDto> GetByIdAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(
            purchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            throw new NotFoundException("Purchase order was not found.");
        }

        return ToDto(purchaseOrder);
    }

    public async Task<PurchaseOrderDto> CreateAsync(
        CreatePurchaseOrderDto createPurchaseOrderDto,
        CancellationToken cancellationToken)
    {
        var normalizedOrderNumber = PurchaseOrderValidationRules
            .ValidateAndNormalizeOrderNumber(createPurchaseOrderDto.OrderNumber);
        var validatedSupplierId = PurchaseOrderValidationRules
            .ValidateSupplierId(createPurchaseOrderDto.SupplierId);
        var validatedItems = PurchaseOrderValidationRules
            .ValidateItems(createPurchaseOrderDto.Items);

        await EnsureOrderNumberIsUniqueAsync(normalizedOrderNumber, cancellationToken);
        await EnsureSupplierExistsAsync(validatedSupplierId, cancellationToken);
        await EnsureProductsExistAsync(validatedItems, cancellationToken);

        var purchaseOrder = new PurchaseOrder
        {
            OrderNumber = normalizedOrderNumber,
            SupplierId = validatedSupplierId,
            Status = PurchaseOrderStatus.Draft,
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
            Items = validatedItems.Select(item => new PurchaseOrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        var createdPurchaseOrder = await purchaseOrderRepository.CreateAsync(
            purchaseOrder,
            cancellationToken);

        var createdPurchaseOrderWithDetails = await purchaseOrderRepository.GetByIdAsync(
            createdPurchaseOrder.PurchaseOrderId,
            cancellationToken);

        if (createdPurchaseOrderWithDetails is null)
        {
            throw new NotFoundException("Purchase order was not found.");
        }

        await auditLogService.RecordAsync("PurchaseOrder", createdPurchaseOrder.PurchaseOrderId, "Created",
            $"Created purchase order {createdPurchaseOrder.OrderNumber}.", cancellationToken);

        return ToDto(createdPurchaseOrderWithDetails);
    }

    public async Task<PurchaseOrderDto> UpdateAsync(
        long purchaseOrderId,
        UpdatePurchaseOrderDto updatePurchaseOrderDto,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(
            purchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            throw new NotFoundException("Purchase order was not found.");
        }

        EnsureIsDraft(purchaseOrder, "updated");

        var normalizedOrderNumber = PurchaseOrderValidationRules
            .ValidateAndNormalizeOrderNumber(updatePurchaseOrderDto.OrderNumber);
        var validatedSupplierId = PurchaseOrderValidationRules
            .ValidateSupplierId(updatePurchaseOrderDto.SupplierId);
        var validatedItems = PurchaseOrderValidationRules
            .ValidateItems(updatePurchaseOrderDto.Items);

        if (!string.Equals(
                purchaseOrder.OrderNumber,
                normalizedOrderNumber,
                StringComparison.OrdinalIgnoreCase))
        {
            await EnsureOrderNumberIsUniqueAsync(normalizedOrderNumber, cancellationToken);
        }

        await EnsureSupplierExistsAsync(validatedSupplierId, cancellationToken);
        await EnsureProductsExistAsync(validatedItems, cancellationToken);

        purchaseOrder.OrderNumber = normalizedOrderNumber;
        purchaseOrder.SupplierId = validatedSupplierId;
        purchaseOrder.Items = validatedItems.Select(item => new PurchaseOrderItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList();

        await purchaseOrderRepository.UpdateAsync(purchaseOrder, cancellationToken);

        await auditLogService.RecordAsync("PurchaseOrder", purchaseOrderId, "Updated",
            $"Updated purchase order {purchaseOrder.OrderNumber}.", cancellationToken);

        return await GetByIdAsync(purchaseOrderId, cancellationToken);
    }

    public async Task DeleteAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(
            purchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            throw new NotFoundException("Purchase order was not found.");
        }

        EnsureIsDraft(purchaseOrder, "deleted");

        await purchaseOrderRepository.DeleteAsync(purchaseOrderId, cancellationToken);

        await auditLogService.RecordAsync("PurchaseOrder", purchaseOrderId, "Deleted",
            $"Deleted purchase order {purchaseOrder.OrderNumber}.", cancellationToken);
    }

    public async Task<PurchaseOrderDto> SubmitAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await GetPurchaseOrderOrThrowAsync(purchaseOrderId, cancellationToken);
        EnsureIsDraft(purchaseOrder, "submitted");

        purchaseOrder.Status = PurchaseOrderStatus.Submitted;
        purchaseOrder.SubmittedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        await purchaseOrderRepository.UpdateAsync(purchaseOrder, cancellationToken);

        await auditLogService.RecordAsync("PurchaseOrder", purchaseOrderId, "Submitted",
            $"Submitted purchase order {purchaseOrder.OrderNumber}.", cancellationToken);

        return await GetByIdAsync(purchaseOrderId, cancellationToken);
    }

    public async Task<PurchaseOrderDto> ReceiveAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await GetPurchaseOrderOrThrowAsync(purchaseOrderId, cancellationToken);
        EnsureIsSubmitted(purchaseOrder, "received");

        await unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            foreach (var item in purchaseOrder.Items)
            {
                await inventoryMovementService.CreateAsync(
                    new CreateInventoryMovementDto(
                        item.ProductId,
                        StockMovementType.StockIn,
                        item.Quantity,
                        $"Purchase order {purchaseOrder.OrderNumber} received."),
                    transactionCancellationToken);
            }

            purchaseOrder.Status = PurchaseOrderStatus.Received;
            purchaseOrder.ReceivedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
            await purchaseOrderRepository.UpdateAsync(purchaseOrder, transactionCancellationToken);
            await auditLogService.RecordAsync("PurchaseOrder", purchaseOrderId, "Received",
                $"Received purchase order {purchaseOrder.OrderNumber}.", transactionCancellationToken);
        }, cancellationToken);

        return await GetByIdAsync(purchaseOrderId, cancellationToken);
    }

    private async Task EnsureOrderNumberIsUniqueAsync(
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var isOrderNumberTaken = await purchaseOrderRepository.ExistsByOrderNumberAsync(
            orderNumber,
            cancellationToken);

        if (isOrderNumberTaken)
        {
            throw new ConflictException("Purchase order number already exists.");
        }
    }

    private async Task EnsureSupplierExistsAsync(
        long supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);

        if (supplier is null)
        {
            throw new NotFoundException("Supplier was not found.");
        }
    }

    private async Task EnsureProductsExistAsync(
        IReadOnlyList<CreatePurchaseOrderItemDto> items,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);

            if (product is null)
            {
                throw new NotFoundException("Product was not found.");
            }
        }
    }

    private static void EnsureIsDraft(PurchaseOrder purchaseOrder, string operation)
    {
        if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
        {
            throw new AppValidationException(
                $"Only draft purchase orders can be {operation}.");
        }
    }

    private static void EnsureIsSubmitted(PurchaseOrder purchaseOrder, string operation)
    {
        if (purchaseOrder.Status != PurchaseOrderStatus.Submitted)
        {
            throw new AppValidationException(
                $"Only submitted purchase orders can be {operation}.");
        }
    }

    private async Task<PurchaseOrder> GetPurchaseOrderOrThrowAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(purchaseOrderId, cancellationToken);
        if (purchaseOrder is null)
        {
            throw new NotFoundException("Purchase order was not found.");
        }

        return purchaseOrder;
    }

    private static PurchaseOrderDto ToDto(PurchaseOrder purchaseOrder) =>
        new(
            purchaseOrder.PurchaseOrderId,
            purchaseOrder.OrderNumber,
            purchaseOrder.SupplierId,
            purchaseOrder.Supplier.CompanyName,
            purchaseOrder.Status,
            purchaseOrder.Items.Select(item => new PurchaseOrderItemDto(
                item.PurchaseOrderItemId,
                item.ProductId,
                item.Product.Name,
                item.Quantity,
                item.UnitPrice)).ToList(),
            purchaseOrder.CreatedAtUtc,
            purchaseOrder.SubmittedAtUtc,
            purchaseOrder.ReceivedAtUtc);
}