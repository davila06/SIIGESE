import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailTest } from './email-test';

describe('EmailTest', () => {
  let component: EmailTest;
  let fixture: ComponentFixture<EmailTest>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EmailTest]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmailTest);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
