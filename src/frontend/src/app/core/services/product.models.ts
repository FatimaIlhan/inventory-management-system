export enum ProductStatus {
    Active,
    Inactive
}

export interface Product {
    productId: number;
    sku: string;
    name: string;
    description?: string;
    unitPrice: number;
    currentStock: number;
    reorderLevel: number;
    status: ProductStatus;
    categoryId: number;
    supplierId: number;
    createdAtUtc: Date;
    updatedAtUtc?: Date;
}
export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
}
export interface ProductListQuery {
    page: number;
    pageSize: number;
    search?: string;
    categoryId?: number;
    supplierId?: number;
    status?: ProductStatus;
}
export interface CreateProductRequest {
    sku: string;
    name: string;
    description?: string;
    unitPrice: number;
    currentStock: number;
    reorderLevel: number;
    status: ProductStatus;
    categoryId: number;
    supplierId: number;
}
export interface UpdateProductRequest {
    sku: string;
    name: string;
    description?: string;
    unitPrice: number;
    currentStock: number;
    reorderLevel: number;
    status: ProductStatus;
    categoryId: number;
    supplierId: number;
}