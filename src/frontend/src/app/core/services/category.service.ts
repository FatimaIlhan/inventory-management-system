import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiEnvelope } from './auth.models';
import { Category, CategoryListQuery, CreateCategoryRequest, PagedResult, UpdateCategoryRequest } from './category.models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiBaseUrl = '/api/categories';

  async getPagedAsync(query: CategoryListQuery): Promise<PagedResult<Category>> {
    const params = new HttpParams({
      fromObject: {
        page: String(query.page),
        pageSize: String(query.pageSize),
        search: query.search?.trim() ?? '',
        _ts: String(Date.now())
      }
    });

    const headers = new HttpHeaders({
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      Pragma: 'no-cache',
      Expires: '0'
    });

    const response = await firstValueFrom(
      this.httpClient.get<ApiEnvelope<PagedResult<Category>>>(this.apiBaseUrl, { params, headers })
    );

    return response.data;
  }

  async createAsync(request: CreateCategoryRequest): Promise<Category> {
    const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<Category>>(this.apiBaseUrl, request)
    );

    return response.data;
  }

  async updateAsync(categoryId: number, request: UpdateCategoryRequest): Promise<Category> {
    const response = await firstValueFrom(
      this.httpClient.put<ApiEnvelope<Category>>(`${this.apiBaseUrl}/${categoryId}`, request)
    );

    return response.data;
  }

  async deleteAsync(categoryId: number): Promise<void> {
    await firstValueFrom(this.httpClient.delete(`${this.apiBaseUrl}/${categoryId}`));
  }
}
