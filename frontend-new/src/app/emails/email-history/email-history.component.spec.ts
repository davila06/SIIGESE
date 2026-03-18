import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { EmailHistoryComponent } from './email-history.component';
import { EmailService, EmailHistoryResponse } from '../../services/email.service';
import { MatSnackBar } from '@angular/material/snack-bar';

const MOCK_HISTORY: EmailHistoryResponse[] = [
  {
    id: 1,
    toEmail: 'user@example.com',
    toName: 'Test User',
    subject: 'Test Subject',
    emailType: 'Manual',
    isSuccess: true,
    errorMessage: '',
    sentAt: new Date(),
    senderName: 'SIIGESE'
  }
];

describe('EmailHistoryComponent', () => {
  let component: EmailHistoryComponent;
  let fixture: ComponentFixture<EmailHistoryComponent>;
  let emailServiceSpy: jasmine.SpyObj<EmailService>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    emailServiceSpy = jasmine.createSpyObj('EmailService', [
      'getEmailHistory', 'getEmailStats', 'sendEmail'
    ]);
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    emailServiceSpy.getEmailHistory.and.returnValue(of(MOCK_HISTORY));

    await TestBed.configureTestingModule({
      imports: [EmailHistoryComponent],
      providers: [
        { provide: EmailService, useValue: emailServiceSpy },
        { provide: MatSnackBar,  useValue: snackBarSpy },
        provideNoopAnimations()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EmailHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call getEmailHistory with page 1 on initialization', () => {
    expect(emailServiceSpy.getEmailHistory).toHaveBeenCalledWith(1, 10);
  });

  it('should populate dataSource with history items', () => {
    expect(component.dataSource.data.length).toBe(1);
    expect(component.dataSource.data[0].toEmail).toBe('user@example.com');
  });

  it('should set totalEmails to the number of records received', () => {
    expect(component.totalEmails).toBe(1);
  });

  it('should set loading to false after history loads', () => {
    expect(component.loading).toBeFalse();
  });

  it('should show snackbar and not crash on getEmailHistory error', () => {
    emailServiceSpy.getEmailHistory.and.returnValue(throwError(() => new Error('net error')));
    expect(() => component.loadEmailHistory()).not.toThrow();
    expect(snackBarSpy.open).toHaveBeenCalled();
  });
});
