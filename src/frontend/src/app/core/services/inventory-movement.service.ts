import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiEnvelope } from './auth.models';
import {
  CreateInventoryMovementRequest,
  InventoryMovement,
  InventoryMovementListQuery,
  PagedInventoryMovements
} from './inventory-movement.models';

@Injectable({ providedIn: 'root' })
export class InventoryMovementService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiBaseUrl = '/api/inventory-movements';

  async getPagedAsync(query: InventoryMovementListQuery): Promise<PagedInventoryMovements> {
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
      this.httpClient.get<ApiEnvelope<PagedInventoryMovements>>(this.apiBaseUrl, { params })
    );

    return response.data;
  }

  async createAsync(request: CreateInventoryMovementRequest): Promise<InventoryMovement> {
    const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<InventoryMovement>>(this.apiBaseUrl, request)
    );

    return response.data;
  }
}