import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiEnvelope } from './auth.models';
import { AuditLogListQuery, PagedAuditLogs } from './audit-log.models';

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiBaseUrl = '/api/audit-logs';

  async getPagedAsync(query: AuditLogListQuery): Promise<PagedAuditLogs> {
    const params = new HttpParams({
      fromObject: {
        page: String(query.page),
        pageSize: String(query.pageSize),
        search: query.search?.trim() ?? '',
        entityType: query.entityType ?? '',
        action: query.action ?? '',
        fromUtc: query.fromUtc ?? '',
        toUtc: query.toUtc ?? ''
      }
    });

    const response = await firstValueFrom(
      this.httpClient.get<ApiEnvelope<PagedAuditLogs>>(this.apiBaseUrl, { params })
    );

    return response.data;
  }
}