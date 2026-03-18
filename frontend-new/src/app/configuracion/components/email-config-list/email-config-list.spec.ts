import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { EmailConfigList } from './email-config-list';
import { EmailConfigService } from '../../services/email-config.service';

describe('EmailConfigList', () => {
  let component: EmailConfigList;
  let fixture: ComponentFixture<EmailConfigList>;
  let emailConfigServiceSpy: jasmine.SpyObj<EmailConfigService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    emailConfigServiceSpy = jasmine.createSpyObj('EmailConfigService', [
      'getAll', 'getById', 'getDefault', 'create', 'update', 'delete',
      'testConfiguration'
    ]);
    routerSpy  = jasmine.createSpyObj('Router',    ['navigate']);
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    emailConfigServiceSpy.getAll.and.returnValue(of({ data: [], success: true, message: '' } as any));

    await TestBed.configureTestingModule({
      declarations: [EmailConfigList],
      providers: [
        { provide: EmailConfigService, useValue: emailConfigServiceSpy },
        { provide: Router,            useValue: routerSpy },
        { provide: MatSnackBar,       useValue: snackBarSpy }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(EmailConfigList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call getAll on initialization', () => {
    expect(emailConfigServiceSpy.getAll).toHaveBeenCalledTimes(1);
  });

  it('should populate emailConfigs with response data', () => {
    expect(component.emailConfigs).toEqual([]);
  });

  it('should show error snackbar when getAll fails', () => {
    emailConfigServiceSpy.getAll.and.returnValue(throwError(() => new Error('net error')));
    component.loadEmailConfigs();
    expect(snackBarSpy.open).toHaveBeenCalled();
  });

  it('should navigate to new form route on onNew()', () => {
    component.onNew();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/configuracion/email/new']);
  });
});
