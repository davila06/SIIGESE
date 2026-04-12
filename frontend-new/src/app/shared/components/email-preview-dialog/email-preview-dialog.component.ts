import { Component, Inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';

export type EmailDialogMode = 'preview' | 'details';

export interface EmailPreviewDialogData {
  mode: EmailDialogMode;
  subject: string;
  /** Rendered HTML body — only required in 'preview' mode. */
  body?: string;
  toEmail?: string;
  toName?: string;
  sentAt?: Date | string;
  emailType?: string;
  isSuccess?: boolean;
  errorMessage?: string;
}

@Component({
  selector: 'app-email-preview-dialog',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatTooltipModule,
  ],
  templateUrl: './email-preview-dialog.component.html',
  styleUrls: ['./email-preview-dialog.component.scss'],
})
export class EmailPreviewDialogComponent {
  safeHtml: SafeHtml | null = null;

  private readonly emailTypeLabels: Record<string, string> = {
    Manual: 'Manual',
    CobroVencido: 'Cobro Vencido',
    ReclamoRecibido: 'Reclamo Recibido',
    Bienvenida: 'Bienvenida',
    PolizaPorVencer: 'Póliza por Vencer',
  };

  constructor(
    public dialogRef: MatDialogRef<EmailPreviewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EmailPreviewDialogData,
    private sanitizer: DomSanitizer,
  ) {
    if (data.mode === 'preview' && data.body) {
      this.safeHtml = sanitizer.bypassSecurityTrustHtml(data.body);
    }
  }

  get emailTypeLabel(): string {
    return this.emailTypeLabels[this.data.emailType ?? ''] ?? (this.data.emailType ?? '—');
  }

  get recipientDisplay(): string {
    if (this.data.toName) {
      return `${this.data.toName} <${this.data.toEmail}>`;
    }
    return this.data.toEmail ?? '—';
  }

  close(): void {
    this.dialogRef.close();
  }
}
