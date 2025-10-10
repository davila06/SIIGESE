import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfiguracionLayout } from './configuracion-layout';

describe('ConfiguracionLayout', () => {
  let component: ConfiguracionLayout;
  let fixture: ComponentFixture<ConfiguracionLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ConfiguracionLayout]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfiguracionLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
