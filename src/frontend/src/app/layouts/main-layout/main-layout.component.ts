import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { asyncScheduler, distinctUntilChanged, fromEvent, map, startWith, throttleTime } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss'
})
export class MainLayoutComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly currentUser = computed(() => this.authService.currentUser());
  readonly isAdmin = computed(() => this.authService.currentRole() === 'Admin');
  readonly hasScrolled = signal(false);

  constructor() {
    this.bindScrollState();
  }

  async logout(): Promise<void> {
    await this.authService.logout();
    await this.router.navigate(['/auth/login']);
  }

  private bindScrollState(): void {
    if (typeof window === 'undefined') {
      return;
    }

    fromEvent(window, 'scroll')
      .pipe(
        startWith(0),
        throttleTime(120, asyncScheduler, { leading: true, trailing: true }),
        map(() => window.scrollY > 8),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((isScrolled) => this.hasScrolled.set(isScrolled));
  }
}
