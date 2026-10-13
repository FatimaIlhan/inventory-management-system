import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiEnvelope } from './auth.models';
import { DashboardQuery, DashboardSummary } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiBaseUrl = `${environment.apiBaseUrl}/dashboard`;

  async getSummaryAsync(query: DashboardQuery): Promise<DashboardSummary> {
    const params = new HttpParams({
      fromObject: {
        fromUtc: query.fromUtc,
        toUtc: query.toUtc
      }
    });

    const response = await firstValueFrom(
      this.httpClient.get<ApiEnvelope<DashboardSummary>>(this.apiBaseUrl, { params })
    );

    return response.data;
  }
}