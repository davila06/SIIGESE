import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';

import { EmailConfigForm } from './email-config-form';
import { EmailConfigService } from '../../services/email-config.service';

describe('EmailConfigForm', () => {
  let component: EmailConfigForm;
  let fixture: ComponentFixture<EmailConfigForm>;
  let emailConfigServiceSpy: jasmine.SpyObj<EmailConfigService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  const activatedRouteMock = {
    snapshot: { paramMap: { get: (_: string) => null } }
  };

  beforeEach(async () => {
    emailConfigServiceSpy = jasmine.createSpyObj('EmailConfigService', [
      'getAll', 'getById', 'getDefault', 'create', 'update', 'delete',
      'testConfiguration', 'testConfigurationDirect'
    ]);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [EmailConfigForm],
      providers: [
        { provide: EmailConfigService, useValue: emailConfigServiceSpy },
        { provide: Router,            useValue: routerSpy },
        { provide: ActivatedRoute,    useValue: activatedRouteMock },
        { provide: MatSnackBar,       useValue: snackBarSpy }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(EmailConfigForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start in create mode when route has no id', () => {
    expect(component.isEditMode).toBeFalse();
    expect(component.configId).toBeFalsy();
  });

  it('should build a FormGroup with all required SMTP controls', () => {
    const form = component.emailConfigForm;
    expect(form).toBeTruthy();
    ['configName', 'smtpServer', 'smtpPort', 'fromEmail', 'fromName',
     'username', 'password', 'isActive', 'isDefault'].forEach(ctrl => {
      expect(form.get(ctrl)).withContext(`control "${ctrl}" should exist`).toBeTruthy();
    });
  });

  it('form should be invalid when required fields are reset', () => {
    component.emailConfigForm.reset();
    expect(component.emailConfigForm.valid).toBeFalse();
  });

  it('should not attempt to load config in create mode', () => {
    expect(emailConfigServiceSpy.getById).not.toHaveBeenCalled();
  });
});
