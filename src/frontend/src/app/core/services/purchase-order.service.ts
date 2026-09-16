import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiEnvelope } from './auth.models';
import {
  CreatePurchaseOrderRequest,
  PagedPurchaseOrders,
  PurchaseOrder,
  PurchaseOrderListQuery,
  UpdatePurchaseOrderRequest
} from './purchase-order.models';

@Injectable({ providedIn: 'root' })
export class PurchaseOrderService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiBaseUrl = '/api/purchase-orders';

  async getPagedAsync(query: PurchaseOrderListQuery): Promise<PagedPurchaseOrders> {
    const params = new HttpParams({
      fromObject: {
        page: String(query.page),
        pageSize: String(query.pageSize),
        search: query.search?.trim() ?? '',
        sortBy: query.sortBy ?? '',
        descending: String(query.descending ?? false)
      }
    });

    const response = await firstValueFrom(
      this.httpClient.get<ApiEnvelope<PagedPurchaseOrders>>(this.apiBaseUrl, { params })
    );

    return response.data;
  }

  async getByIdAsync(purchaseOrderId: number): Promise<PurchaseOrder> {
    const response = await firstValueFrom(
      this.httpClient.get<ApiEnvelope<PurchaseOrder>>(`${this.apiBaseUrl}/${purchaseOrderId}`)
    );

    return response.data;
  }

  async createAsync(request: CreatePurchaseOrderRequest): Promise<PurchaseOrder> {
    const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<PurchaseOrder>>(this.apiBaseUrl, request)
    );

    return response.data;
  }

  async updateAsync(purchaseOrderId: number, request: UpdatePurchaseOrderRequest): Promise<PurchaseOrder> {
    const response = await firstValueFrom(
      this.httpClient.put<ApiEnvelope<PurchaseOrder>>(`${this.apiBaseUrl}/${purchaseOrderId}`, request)
    );

    return response.data;
  }

  async deleteAsync(purchaseOrderId: number): Promise<void> {
    await firstValueFrom(this.httpClient.delete(`${this.apiBaseUrl}/${purchaseOrderId}`));
  }

  async submitAsync(purchaseOrderId: number): Promise<PurchaseOrder> {
    const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<PurchaseOrder>>(`${this.apiBaseUrl}/${purchaseOrderId}/submit`, {})
    );

    return response.data;
  }

  async receiveAsync(purchaseOrderId: number): Promise<PurchaseOrder> {
    const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<PurchaseOrder>>(`${this.apiBaseUrl}/${purchaseOrderId}/receive`, {})
    );

    return response.data;
  }
}