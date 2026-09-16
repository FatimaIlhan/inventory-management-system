import { StockMovementType } from './inventory-movement.models';

export interface DashboardSummary {
  totalProducts: number;
  totalCategories: number;
  totalSuppliers: number;
  lowStockItemCount: number;
  totalPurchaseOrders: number;
  recentActivityCount: number;
  movementTrends: DashboardMovementTrend[];
  inventoryByCategory: DashboardCategoryInventory[];
  topMovingProducts: DashboardTopMovingProduct[];
  recentActivities: DashboardRecentActivity[];
  lowStockProducts: DashboardLowStockProduct[];
}

export interface DashboardMovementTrend {
  dateUtc: string;
  stockInQuantity: number;
  stockOutQuantity: number;
  adjustmentQuantity: number;
}

export interface DashboardCategoryInventory {
  categoryId: number;
  categoryName: string;
  currentStock: number;
}

export interface DashboardTopMovingProduct {
  productId: number;
  productName: string;
  sku: string;
  totalQuantityMoved: number;
}

export interface DashboardRecentActivity {
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

export interface DashboardLowStockProduct {
  productId: number;
  sku: string;
  productName: string;
  currentStock: number;
  reorderLevel: number;
}

export interface DashboardQuery {
  fromUtc: string;
  toUtc: string;
}