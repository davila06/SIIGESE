import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { provideRouter } from '@angular/router';

import { ConfiguracionLayout } from './configuracion-layout';

describe('ConfiguracionLayout', () => {
  let component: ConfiguracionLayout;
  let fixture: ComponentFixture<ConfiguracionLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ConfiguracionLayout],
      providers: [provideRouter([])],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(ConfiguracionLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should expose a non-empty menuItems array', () => {
    expect(component.menuItems).toBeDefined();
    expect(component.menuItems.length).toBeGreaterThan(0);
  });

  it('each menu item should have label, route and icon', () => {
    component.menuItems.forEach(item => {
      expect(item['label']).toBeTruthy();
      expect(item['route']).toBeTruthy();
      expect(item['icon']).toBeTruthy();
    });
  });
});
