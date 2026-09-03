import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiEnvelope } from './auth.models';
import { Category, CategoryListQuery, CreateCategoryRequest, PagedResult, UpdateCategoryRequest } from './category.models';
import { HttpErrorResponse } from '@angular/common/http';
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
     console.log('Creating category with request:', request);
  console.log('POST URL:', this.apiBaseUrl);
  try{
        const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<Category>>(this.apiBaseUrl, request)
    );
   console.log('Create category response:', response);
    return response.data;
  
  }
catch (error) {
  console.error('Create category failed:', error);

  if (error instanceof HttpErrorResponse) {
    console.error('Status:', error.status);
    console.error('Status text:', error.statusText);
    console.error('URL:', error.url);
    console.error('Response body:', error.error);
  }

  throw error;
}

  }
  async updateAsync(categoryId: number, request: UpdateCategoryRequest): Promise<Category> {
    const response = await firstValueFrom(
      this.httpClient.put<ApiEnvelope<Category>>(`${this.apiBaseUrl}/${categoryId}`, request)
    );
    console.log('Update category response:', response);
    return response.data;
  }

  async deleteAsync(categoryId: number): Promise<void> {
    await firstValueFrom(this.httpClient.delete(`${this.apiBaseUrl}/${categoryId}`));
  }
}
