import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { EmailDashboardComponent } from './email-dashboard.component';
import { EmailService, EmailStats } from '../../services/email.service';
import { MatSnackBar } from '@angular/material/snack-bar';

const MOCK_STATS: EmailStats = {
  totalSent: 10,
  totalFailed: 2,
  pendingCobros: 5,
  polizasPorVencer: 3
};

describe('EmailDashboardComponent', () => {
  let component: EmailDashboardComponent;
  let fixture: ComponentFixture<EmailDashboardComponent>;
  let emailServiceSpy: jasmine.SpyObj<EmailService>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    emailServiceSpy = jasmine.createSpyObj('EmailService', [
      'sendEmail', 'sendBulkEmails', 'getEmailHistory', 'getEmailStats',
      'sendAutomaticCobroVencidoNotifications',
      'sendAutomaticPolizasPorVencerNotifications'
    ]);
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    emailServiceSpy.getEmailStats.and.returnValue(of(MOCK_STATS));

    await TestBed.configureTestingModule({
      imports: [EmailDashboardComponent],
      providers: [
        { provide: EmailService, useValue: emailServiceSpy },
        { provide: MatSnackBar,  useValue: snackBarSpy },
        provideRouter([]),
        provideNoopAnimations()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EmailDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call getEmailStats on initialization', () => {
    expect(emailServiceSpy.getEmailStats).toHaveBeenCalledTimes(1);
  });

  it('should populate stats from service response', () => {
    expect(component.stats.totalSent).toBe(10);
    expect(component.stats.totalFailed).toBe(2);
    expect(component.stats.pendingCobros).toBe(5);
    expect(component.stats.polizasPorVencer).toBe(3);
  });

  it('should set loading to false after stats load', () => {
    expect(component.loading).toBeFalse();
  });

  it('should handle getEmailStats error without crashing', () => {
    emailServiceSpy.getEmailStats.and.returnValue(throwError(() => new Error('HTTP 500')));
    expect(() => component.loadStats()).not.toThrow();
    expect(component.loading).toBeFalse();
  });
});
