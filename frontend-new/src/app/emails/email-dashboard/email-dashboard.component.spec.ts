import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailDashboardComponent } from './email-dashboard.component';

describe('EmailDashboardComponent', () => {
  let component: EmailDashboardComponent;
  let fixture: ComponentFixture<EmailDashboardComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [EmailDashboardComponent]
    });
    fixture = TestBed.createComponent(EmailDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
