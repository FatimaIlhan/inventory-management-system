import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../services/auth.models';

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const allowedRoles = (route.data['allowedRoles'] as UserRole[] | undefined) ?? [];
  if (allowedRoles.length === 0) {
    return true;
  }

  if (authService.canAccessRole(allowedRoles)) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
