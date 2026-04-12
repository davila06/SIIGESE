import { CommonModule } from '@angular/common';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { BehaviorSubject, of } from 'rxjs';

import { PolizasComponent } from './polizas.component';
import { Poliza } from '../interfaces/user.interface';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { LoggingService } from '../services/logging.service';

describe('PolizasComponent', () => {
  let component: PolizasComponent;
  let fixture: ComponentFixture<PolizasComponent>;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;
  let dialogSpy: jasmine.SpyObj<MatDialog>;
  let loggerSpy: jasmine.SpyObj<LoggingService>;

  const basePoliza: Poliza = {
    id: 10,
    numeroPoliza: 'POL-EDIT-001',
    modalidad: 'Individual',
    nombreAsegurado: 'Juan Perez',
    numeroCedula: '1-1234-5678',
    prima: 125000,
    moneda: 'CRC',
    fechaVigencia: '2026-12-31T00:00:00',
    frecuencia: 'MENSUAL',
    aseguradora: 'INS',
    placa: 'ABC123',
    marca: 'Toyota',
    modelo: 'Corolla',
    año: '2024',
    correo: 'juan@example.com',
    numeroTelefono: '8888-9999',
    perfilId: 1,
    esActivo: true,
    observaciones: 'Original',
    fechaCreacion: '2026-01-01T00:00:00',
    usuarioCreacion: 'admin'
  };

  beforeEach(async () => {
    apiServiceSpy = jasmine.createSpyObj('ApiService', [
      'getPolizas', 'createPoliza', 'updatePoliza', 'deletePoliza'
    ]);
    authServiceSpy = jasmine.createSpyObj('AuthService',
      ['getCurrentUserId', 'hasRole', 'hasPermission', 'hasAnyRole', 'logout', 'isAdmin', 'canUploadExcel'],
      {
        isAuthenticated$: new BehaviorSubject(true),
        currentUser$: new BehaviorSubject({ id: 1, email: 'admin@sinseg.cr', role: 'Admin' })
      }
    );
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);
    dialogSpy = jasmine.createSpyObj('MatDialog', ['open']);
    loggerSpy = jasmine.createSpyObj('LoggingService', ['error', 'warn', 'info', 'debug']);

    apiServiceSpy.getPolizas.and.returnValue(of([basePoliza]));
    apiServiceSpy.createPoliza.and.returnValue(of(basePoliza));
    apiServiceSpy.updatePoliza.and.returnValue(of({
      ...basePoliza,
      nombreAsegurado: 'Juan Perez Actualizado',
      observaciones: 'Actualizada'
    }));
    apiServiceSpy.deletePoliza.and.returnValue(of(void 0));

    authServiceSpy.getCurrentUserId.and.returnValue(1);
    authServiceSpy.hasPermission.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(true);
    authServiceSpy.hasAnyRole.and.returnValue(true);
    authServiceSpy.isAdmin.and.returnValue(true);
    authServiceSpy.canUploadExcel.and.returnValue(true);

    await TestBed.configureTestingModule({
      declarations: [PolizasComponent],
      imports: [CommonModule, FormsModule, ReactiveFormsModule],
      providers: [
        { provide: ApiService, useValue: apiServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: LoggingService, useValue: loggerSpy },
        provideNoopAnimations()
      ],
      schemas: [NO_ERRORS_SCHEMA]
    })
      .overrideTemplate(PolizasComponent, `
        <form [formGroup]="polizaForm" (ngSubmit)="onSubmit()">
          <button type="submit" [disabled]="isLoading || !isFormValidForSubmission()">
            {{ submitButtonText }}
          </button>
        </form>
      `)
      .compileComponents();

    fixture = TestBed.createComponent(PolizasComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load polizas on init', () => {
    expect(apiServiceSpy.getPolizas).toHaveBeenCalledTimes(1);
    expect(component.polizas.length).toBe(1);
    expect(component.polizas[0].numeroPoliza).toBe('POL-EDIT-001');
  });

  it('should enable submit button when editing a valid poliza', fakeAsync(() => {
    component.selectPolizaAndScroll(basePoliza);
    tick(200);
    fixture.detectChanges();

    const submitButton = fixture.nativeElement.querySelector('button[type="submit"]') as HTMLButtonElement;

    expect(component.isEditMode).toBeTrue();
    expect(component.selectedPoliza?.id).toBe(basePoliza.id);
    expect(component.polizaForm.get('nombreAsegurado')?.value).toBe('Juan Perez');
    expect(component.isFormValidForSubmission()).toBeTrue();
    expect(submitButton.disabled).toBeFalse();
  }));

  it('should normalize legacy insurer values when loading a poliza for edit', fakeAsync(() => {
    component.selectPolizaAndScroll({
      ...basePoliza,
      aseguradora: 'Sagicor Seguros'
    });
    tick(200);
    fixture.detectChanges();

    expect(component.polizaForm.get('aseguradora')?.value).toBe('SAGICOR');
    expect(component.isFormValidForSubmission()).toBeTrue();
  }));

  it('should update an existing poliza and reset the form after submit', fakeAsync(() => {
    component.selectPolizaAndScroll(basePoliza);
    tick(200);
    fixture.detectChanges();

    component.polizaForm.patchValue({
      nombreAsegurado: 'Juan Perez Actualizado',
      observaciones: 'Actualizada'
    });

    component.onSubmit();
    tick();
    fixture.detectChanges();

    expect(apiServiceSpy.updatePoliza).toHaveBeenCalledTimes(1);
    expect(apiServiceSpy.updatePoliza).toHaveBeenCalledWith(basePoliza.id, jasmine.objectContaining({
      nombreAsegurado: 'Juan Perez Actualizado',
      observaciones: 'Actualizada'
    }));
    expect(component.isEditMode).toBeFalse();
    expect(component.selectedPoliza).toBeNull();
    expect(component.polizas[0].nombreAsegurado).toBe('Juan Perez Actualizado');
    expect(snackBarSpy.open).toHaveBeenCalled();
  }));
});