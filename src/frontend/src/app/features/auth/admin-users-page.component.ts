import { Component, signal, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../core/services/auth.models';
import { ApiEnvelope } from '../../core/services/auth.models';

@Component({
  selector: 'app-admin-users-page',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-users-page.component.html',
  styleUrl: './admin-users-page.component.scss'
})
export class AdminUsersPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly isSubmitting = signal(false);
  readonly resultMessage = signal<string | null>(null);
  readonly roles: UserRole[] = ['Admin', 'Manager', 'Employee'];

  readonly createUserForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['Employee' as UserRole, [Validators.required]]
  });

  async createUser(): Promise<void> {
    if (this.createUserForm.invalid) {
      this.createUserForm.markAllAsTouched();
      return;
    }

    this.resultMessage.set(null);
    this.isSubmitting.set(true);

    try {
      const formValue = this.createUserForm.getRawValue();
      // Trim email to remove any accidental whitespace
      const createUserRequest = {
        ...formValue,
        email: formValue.email.trim()
      };
      
      const createdUser = await this.authService.createUser(createUserRequest);
      this.resultMessage.set(`Created user ${createdUser.email} with ${createdUser.role} role.`);
      this.createUserForm.patchValue({ password: '' });
    } catch (error: unknown) {
      let errorMessage = 'Failed to create user.';
      
      if (error instanceof HttpErrorResponse) {
        // Try to extract error message from API envelope
        const apiError = error.error as ApiEnvelope<unknown> | null;
        if (apiError?.message) {
          errorMessage = apiError.message;
        } else if (apiError?.errors && Array.isArray(apiError.errors) && apiError.errors.length > 0) {
          errorMessage = apiError.errors[0];
        } else if (error.status === 401) {
          errorMessage = 'Unauthorized. Please log in again.';
        } else if (error.status === 403) {
          errorMessage = 'Forbidden. Admin access required.';
        } else {
          errorMessage = `Server error (${error.status}). ${error.statusText || 'Unknown error'}`;
        }
      } else if (error instanceof Error) {
        errorMessage = error.message;
      }
      
      this.resultMessage.set(`Error: ${errorMessage}`);
      console.error('[CreateUser Error]', error);
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
