import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-change-password',
  standalone: false,
  template: `
    <div class="container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Cambiar Contraseña</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="changePasswordForm" (ngSubmit)="onSubmit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Contraseña Actual</mat-label>
              <input matInput type="password" formControlName="currentPassword" required>
              <mat-error *ngIf="changePasswordForm.get('currentPassword')?.hasError('required')">
                La contraseña actual es requerida
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Nueva Contraseña</mat-label>
              <input matInput type="password" formControlName="newPassword" required>
              <mat-error *ngIf="changePasswordForm.get('newPassword')?.hasError('required')">
                La nueva contraseña es requerida
              </mat-error>
              <mat-error *ngIf="changePasswordForm.get('newPassword')?.hasError('minlength')">
                Debe tener al menos 6 caracteres
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Confirmar Nueva Contraseña</mat-label>
              <input matInput type="password" formControlName="confirmPassword" required>
              <mat-error *ngIf="changePasswordForm.get('confirmPassword')?.hasError('required')">
                La confirmación es requerida
              </mat-error>
              <mat-error *ngIf="changePasswordForm.hasError('mismatch') && changePasswordForm.get('confirmPassword')?.touched">
                Las contraseñas no coinciden
              </mat-error>
            </mat-form-field>

            <button mat-raised-button color="primary" type="submit" class="full-width"
                    [disabled]="isLoading || changePasswordForm.invalid">
              <mat-spinner diameter="20" *ngIf="isLoading" style="display:inline-block;margin-right:8px;"></mat-spinner>
              {{ isLoading ? 'Cambiando...' : 'Cambiar Contraseña' }}
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 500px;
      margin: 0 auto;
    }
    .full-width {
      width: 100%;
      margin-bottom: 15px;
    }
  `]
})
export class ChangePasswordComponent {
  changePasswordForm: FormGroup;
  isLoading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly snackBar: MatSnackBar
  ) {
    this.changePasswordForm = this.fb.group(
      {
        currentPassword: ['', Validators.required],
        newPassword: ['', [Validators.required, Validators.minLength(6)]],
        confirmPassword: ['', Validators.required]
      },
      { validators: this.passwordsMatchValidator }
    );
  }

  private passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const newPwd = group.get('newPassword')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return newPwd === confirm ? null : { mismatch: true };
  }

  onSubmit(): void {
    if (this.changePasswordForm.invalid) return;

    this.isLoading = true;
    const { currentPassword, newPassword, confirmPassword } = this.changePasswordForm.value;

    this.authService.changePassword(currentPassword, newPassword, confirmPassword).subscribe({
      next: () => {
        this.snackBar.open('Contraseña cambiada exitosamente', 'Cerrar', { duration: 4000 });
        this.changePasswordForm.reset();
        this.isLoading = false;
      },
      error: (err) => {
        const msg = err?.error?.message || err?.message || 'Error al cambiar la contraseña';
        this.snackBar.open(msg, 'Cerrar', { duration: 5000 });
        this.isLoading = false;
      }
    });
  }
}

