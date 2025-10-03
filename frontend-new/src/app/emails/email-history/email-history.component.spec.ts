import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailHistoryComponent } from './email-history.component';

describe('EmailHistoryComponent', () => {
  let component: EmailHistoryComponent;
  let fixture: ComponentFixture<EmailHistoryComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [EmailHistoryComponent]
    });
    fixture = TestBed.createComponent(EmailHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
