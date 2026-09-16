import { PagedResult } from './product.models';

export enum PurchaseOrderStatus {
  Draft = 1,
  Submitted = 2,
  Received = 3,
  Cancelled = 4
}

export interface PurchaseOrderItem {
  purchaseOrderItemId: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface PurchaseOrderItemRequest {
  productId: number;
  quantity: number;
  unitPrice: number;
}

export interface CreatePurchaseOrderRequest {
  orderNumber: string;
  supplierId: number;
  items: PurchaseOrderItemRequest[];
}

export type UpdatePurchaseOrderRequest = CreatePurchaseOrderRequest;

export interface PurchaseOrder {
  purchaseOrderId: number;
  orderNumber: string;
  supplierId: number;
  supplierName: string;
  status: PurchaseOrderStatus;
  items: PurchaseOrderItem[];
  createdAtUtc: Date;
  submittedAtUtc?: Date;
  receivedAtUtc?: Date;
}

export interface PurchaseOrderListQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: PurchaseOrderSortField;
  descending?: boolean;
}

export type PurchaseOrderSortField =
  | 'orderNumber'
  | 'supplierName'
  | 'status'
  | 'createdAtUtc'
  | 'submittedAtUtc'
  | 'receivedAtUtc';

export type PagedPurchaseOrders = PagedResult<PurchaseOrder>;