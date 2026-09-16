import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';
import {
  ConfirmationDialogComponent,
  ConfirmationDialogData
} from '../components/confirmation-dialog/confirmation-dialog.component';

@Injectable({ providedIn: 'root' })
export class ConfirmationService {
  private readonly dialog = inject(MatDialog);

  async confirm(data: ConfirmationDialogData): Promise<boolean> {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data,
      width: 'min(420px, calc(100vw - 2rem))',
      autoFocus: 'dialog',
      restoreFocus: true
    });

    return (await firstValueFrom(dialogRef.afterClosed())) === true;
  }
}