import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailConfigForm } from './email-config-form';

describe('EmailConfigForm', () => {
  let component: EmailConfigForm;
  let fixture: ComponentFixture<EmailConfigForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EmailConfigForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmailConfigForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
