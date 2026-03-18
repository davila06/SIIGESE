/**
 * @file change-password.spec.ts
 * @description Unit tests for ChangePasswordComponent.
 *
 * Tests cover:
 *  - Component creation and initial form state
 *  - Cross-field passwords-match validator
 *  - Successful password change flow (calls AuthService, resets the form)
 *  - Error propagation from AuthService
 */
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { ChangePasswordComponent } from './change-password.component';
import { AuthService } from '../../services/auth.service';

describe('ChangePasswordComponent', () => {
  let component: ChangePasswordComponent;
  let fixture:   ComponentFixture<ChangePasswordComponent>;
  let authSpy:   jasmine.SpyObj<AuthService>;
  let snackSpy:  jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    authSpy  = jasmine.createSpyObj<AuthService>('AuthService', ['changePassword']);
    snackSpy = jasmine.createSpyObj<MatSnackBar>('MatSnackBar',  ['open']);

    await TestBed.configureTestingModule({
      declarations: [ChangePasswordComponent],
      imports:      [ReactiveFormsModule],
      providers: [
        { provide: AuthService,  useValue: authSpy  },
        { provide: MatSnackBar,  useValue: snackSpy },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture   = TestBed.createComponent(ChangePasswordComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with an invalid form (all fields empty)', () => {
    expect(component.changePasswordForm.valid).toBeFalse();
    expect(component.isLoading).toBeFalse();
  });

  it('should be invalid when newPassword and confirmPassword do not match', () => {
    component.changePasswordForm.setValue({
      currentPassword: 'OldPass1!',
      newPassword:     'NewPass1!',
      confirmPassword: 'DIFFERENT',
    });
    expect(component.changePasswordForm.hasError('mismatch')).toBeTrue();
  });

  it('should be valid when all fields are filled and passwords match', () => {
    component.changePasswordForm.setValue({
      currentPassword: 'OldPass1!',
      newPassword:     'NewPass1!',
      confirmPassword: 'NewPass1!',
    });
    expect(component.changePasswordForm.valid).toBeTrue();
  });

  describe('onSubmit', () => {
    it('should do nothing when the form is invalid', () => {
      component.onSubmit();
      expect(authSpy.changePassword).not.toHaveBeenCalled();
    });

    it('should call AuthService.changePassword and reset the form on success', () => {
      authSpy.changePassword.and.returnValue(of(void 0));
      component.changePasswordForm.setValue({
        currentPassword: 'OldPass1!',
        newPassword:     'NewPass1!',
        confirmPassword: 'NewPass1!',
      });

      component.onSubmit();

      expect(authSpy.changePassword).toHaveBeenCalledWith('OldPass1!', 'NewPass1!', 'NewPass1!');
      expect(snackSpy.open).toHaveBeenCalled();
      expect(component.isLoading).toBeFalse();
    });

    it('should show an error snack-bar and clear isLoading when AuthService fails', () => {
      authSpy.changePassword.and.returnValue(throwError(() => ({ message: 'Contraseña incorrecta' })));
      component.changePasswordForm.setValue({
        currentPassword: 'WrongPass!',
        newPassword:     'NewPass1!',
        confirmPassword: 'NewPass1!',
      });

      component.onSubmit();

      expect(snackSpy.open).toHaveBeenCalledWith('Contraseña incorrecta', 'Cerrar', { duration: 5000 });
      expect(component.isLoading).toBeFalse();
    });
  });
});
