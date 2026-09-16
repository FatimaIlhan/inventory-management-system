using Application.DTOs;

namespace Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<PagedResultDto<PurchaseOrderDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);

    Task<PurchaseOrderDto> GetByIdAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken);

    Task<PurchaseOrderDto> CreateAsync(
        CreatePurchaseOrderDto createPurchaseOrderDto,
        CancellationToken cancellationToken);

    Task<PurchaseOrderDto> UpdateAsync(
        long purchaseOrderId,
        UpdatePurchaseOrderDto updatePurchaseOrderDto,
        CancellationToken cancellationToken);

    Task<PurchaseOrderDto> SubmitAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken);

    Task<PurchaseOrderDto> ReceiveAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken);
}