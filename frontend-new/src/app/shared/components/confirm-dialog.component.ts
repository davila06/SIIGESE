import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

export interface ConfirmDialogData {
  title: string;
  message: string;
  detail?: string;
  confirmText?: string;
  confirmColor?: 'primary' | 'accent' | 'warn';
  icon?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="confirm-dialog-inner">
      <div class="confirm-header" [class.warn]="(data.confirmColor || 'warn') === 'warn'">
        <mat-icon class="confirm-icon">{{ data.icon || 'warning' }}</mat-icon>
        <h2 mat-dialog-title>{{ data.title }}</h2>
      </div>
      <mat-dialog-content>
        <p class="confirm-message">{{ data.message }}</p>
        <p class="confirm-detail" *ngIf="data.detail">{{ data.detail }}</p>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-stroked-button (click)="cancel()">
          <mat-icon>close</mat-icon> Cancelar
        </button>
        <button mat-raised-button [color]="data.confirmColor || 'warn'" (click)="confirm()">
          <mat-icon>{{ data.icon || 'delete' }}</mat-icon>
          {{ data.confirmText || 'Confirmar' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .confirm-dialog-inner { padding: 0; }
    .confirm-header {
      display: flex; align-items: center; gap: 12px;
      padding: 20px 24px 12px;
      border-bottom: 1px solid rgba(0,0,0,0.08);
    }
    .confirm-header.warn .confirm-icon { color: #f44336; }
    .confirm-icon { font-size: 28px; width: 28px; height: 28px; }
    h2[mat-dialog-title] { margin: 0; font-size: 18px; font-weight: 600; }
    mat-dialog-content { padding: 16px 24px; }
    .confirm-message { margin: 0 0 6px; font-size: 15px; }
    .confirm-detail { margin: 0; font-size: 13px; color: #888; }
    mat-dialog-actions { padding: 8px 16px 16px; gap: 8px; }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}

  confirm(): void { this.dialogRef.close(true); }
  cancel(): void  { this.dialogRef.close(false); }
}
