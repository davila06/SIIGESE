import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailConfigList } from './email-config-list';

describe('EmailConfigList', () => {
  let component: EmailConfigList;
  let fixture: ComponentFixture<EmailConfigList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EmailConfigList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmailConfigList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
