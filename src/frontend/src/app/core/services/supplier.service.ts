import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import type { Supplier, SupplierListQuery, PagedResult, CreateSupplierRequest, UpdateSupplierRequest } from "./supplier.models";
import { ApiEnvelope } from "./auth.models";
import { firstValueFrom } from "rxjs/internal/firstValueFrom";

@Injectable({ providedIn: 'root' })
export class SupplierService {
 private readonly httpClient = inject(HttpClient);
  private readonly apiBaseUrl = '/api/suppliers';

  async getPagedAsync(query: SupplierListQuery): Promise<PagedResult<Supplier>> {
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
      this.httpClient.get<ApiEnvelope<PagedResult<Supplier>>>(this.apiBaseUrl, { params, headers })
    );

    return response.data;
  }

  
    async createAsync(request: CreateSupplierRequest): Promise<Supplier> {
      const response = await firstValueFrom(
        this.httpClient.post<ApiEnvelope<Supplier>>(`${this.apiBaseUrl}/create-supplier`, request)
      );

      return response.data;
    }

      async updateAsync(supplierId: number, request: UpdateSupplierRequest): Promise<Supplier> {
        const response = await firstValueFrom(
          this.httpClient.put<ApiEnvelope<Supplier>>(`${this.apiBaseUrl}/${supplierId}`, request)
        );

        return response.data;
      }

      async deleteAsync(supplierId: number): Promise<void> {
        await firstValueFrom(this.httpClient.delete(`${this.apiBaseUrl}/${supplierId}`));
      }
}