import { DestroyRef, Injectable, inject } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';

interface RouteSeoData {
  title?: string;
  description?: string;
}

@Injectable({ providedIn: 'root' })
export class SeoService {
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly titleService = inject(Title);
  private readonly metaService = inject(Meta);
  private readonly destroyRef = inject(DestroyRef);

  private initialized = false;

  initializeRouteMetadataSync(): void {
    if (this.initialized) {
      return;
    }

    this.initialized = true;

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        startWith(null),
        map(() => this.getDeepestActiveRoute(this.activatedRoute)),
        map((route) => route.snapshot.data as RouteSeoData),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((seoData) => {
        const title = seoData.title ?? 'Inventory Management System';
        const description =
          seoData.description ??
          'Inventory management platform for authentication, dashboards, and role-based administration.';

        this.titleService.setTitle(title);
        this.metaService.updateTag({ name: 'description', content: description });
        this.metaService.updateTag({ property: 'og:title', content: title });
        this.metaService.updateTag({ property: 'og:description', content: description });
      });
  }

  private getDeepestActiveRoute(route: ActivatedRoute): ActivatedRoute {
    let activeRoute = route;
    while (activeRoute.firstChild) {
      activeRoute = activeRoute.firstChild;
    }

    return activeRoute;
  }
}
