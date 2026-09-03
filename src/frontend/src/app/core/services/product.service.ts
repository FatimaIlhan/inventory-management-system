import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiEnvelope } from './auth.models';
import {
  CreateProductRequest,
  PagedResult,
  Product,
  ProductListQuery,
  UpdateProductRequest
} from './product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiBaseUrl = '/api/products';

  async getPagedAsync(query: ProductListQuery): Promise<PagedResult<Product>> {
    const params = new HttpParams({
      fromObject: {
        page: String(query.page),
        pageSize: String(query.pageSize),
        search: query.search?.trim() ?? '',
        categoryId: query.categoryId != null ? String(query.categoryId) : '',
        supplierId: query.supplierId != null ? String(query.supplierId) : '',
        status: query.status != null ? String(query.status) : '',
        _ts: String(Date.now())
      }
    });

    const headers = new HttpHeaders({
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      Pragma: 'no-cache',
      Expires: '0'
    });

    const response = await firstValueFrom(
      this.httpClient.get<ApiEnvelope<PagedResult<Product>>>(this.apiBaseUrl, { params, headers })
    );

    return response.data;
  }

  async createAsync(request: CreateProductRequest): Promise<Product> {
    const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<Product>>(this.apiBaseUrl, request)
    );

    return response.data;
  }

  async updateAsync(productId: number, request: UpdateProductRequest): Promise<Product> {
    const response = await firstValueFrom(
      this.httpClient.put<ApiEnvelope<Product>>(`${this.apiBaseUrl}/${productId}`, request)
    );

    return response.data;
  }

  async deleteAsync(productId: number): Promise<void> {
    await firstValueFrom(this.httpClient.delete(`${this.apiBaseUrl}/${productId}`));
  }
}
