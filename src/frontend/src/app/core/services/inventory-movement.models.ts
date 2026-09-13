import { PagedResult } from './product.models';

export enum StockMovementType {
  StockIn = 1,
  StockOut = 2,
  Adjustment = 3
}

export interface InventoryMovement {
  inventoryMovementId: number;
  productId: number;
  productName: string;
  performedByUserId: number;
  performedByUserName: string;
  movementType: StockMovementType;
  quantity: number;
  previousStock: number;
  newStock: number;
  reason: string;
  createdAtUtc: string;
}

export interface InventoryMovementListQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  descending?: boolean;
}

export interface CreateInventoryMovementRequest {
  productId: number;
  movementType: StockMovementType;
  quantity: number;
  reason: string;
}

export type PagedInventoryMovements = PagedResult<InventoryMovement>;