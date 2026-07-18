import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { catchError, filter, from, interval, of, switchMap } from 'rxjs';
import { AuthService } from './core/services/auth.service';
import { SeoService } from './core/services/seo.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly seoService = inject(SeoService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    void this.authService.loadCurrentUser();
    this.startSessionRefreshHeartbeat();
    this.seoService.initializeRouteMetadataSync();
  }

  protected readonly title = signal('Inventory Management System');

  private startSessionRefreshHeartbeat(): void {
    interval(60_000)
      .pipe(
        filter(() => this.authService.isAuthenticated()),
        switchMap(() =>
          from(this.authService.refreshSession()).pipe(catchError(() => of(false)))
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }
}
