import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

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
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Nueva Contraseña</mat-label>
              <input matInput type="password" formControlName="newPassword" required>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Confirmar Nueva Contraseña</mat-label>
              <input matInput type="password" formControlName="confirmPassword" required>
            </mat-form-field>

            <button mat-raised-button color="primary" type="submit" class="full-width">
              Cambiar Contraseña
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

  constructor(private fb: FormBuilder) {
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.changePasswordForm.valid) {
      // Mock password change logic
      console.log('Password change requested');
    }
  }
}
