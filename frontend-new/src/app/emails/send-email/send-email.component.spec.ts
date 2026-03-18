import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { SendEmailComponent } from './send-email.component';
import { EmailService, EmailResponse } from '../../services/email.service';
import { MatSnackBar } from '@angular/material/snack-bar';

describe('SendEmailComponent', () => {
  let component: SendEmailComponent;
  let fixture: ComponentFixture<SendEmailComponent>;
  let emailServiceSpy: jasmine.SpyObj<EmailService>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    emailServiceSpy = jasmine.createSpyObj('EmailService', [
      'sendEmail', 'sendBulkEmails', 'getEmailHistory', 'getEmailStats'
    ]);
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      imports: [SendEmailComponent],
      providers: [
        { provide: EmailService, useValue: emailServiceSpy },
        { provide: MatSnackBar,  useValue: snackBarSpy },
        provideRouter([]),
        provideNoopAnimations()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SendEmailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize emailForm with required controls', () => {
    expect(component.emailForm).toBeTruthy();
    expect(component.emailForm.get('toEmail')).toBeTruthy();
    expect(component.emailForm.get('subject')).toBeTruthy();
    expect(component.emailForm.get('body')).toBeTruthy();
    expect(component.emailForm.get('emailType')).toBeTruthy();
  });

  it('emailType should default to Manual', () => {
    expect(component.emailForm.get('emailType')?.value).toBe('Manual');
  });

  it('form should be invalid when required fields are empty', () => {
    component.emailForm.patchValue({ toEmail: '', subject: '', body: '' });
    expect(component.emailForm.valid).toBeFalse();
  });

  it('form should be valid with all required fields filled', () => {
    component.emailForm.patchValue({
      toEmail: 'user@test.com',
      subject: 'Hello',
      body: 'Body text',
      emailType: 'Manual'
    });
    expect(component.emailForm.valid).toBeTrue();
  });

  it('should call emailService.sendEmail on valid submit', () => {
    const mockResponse: EmailResponse = { isSuccess: true, message: 'Sent', emailLogId: 1 };
    emailServiceSpy.sendEmail.and.returnValue(of(mockResponse));

    component.emailForm.patchValue({
      toEmail: 'user@test.com',
      toName: 'Test',
      subject: 'Hello',
      body: 'Body text',
      emailType: 'Manual'
    });
    component.onSubmit();

    expect(emailServiceSpy.sendEmail).toHaveBeenCalledTimes(1);
  });

  it('should not call sendEmail when form is invalid', () => {
    component.emailForm.patchValue({ toEmail: '', subject: '', body: '' });
    component.onSubmit();
    expect(emailServiceSpy.sendEmail).not.toHaveBeenCalled();
  });
});
