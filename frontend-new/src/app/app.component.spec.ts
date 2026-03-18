/**
 * @file app.component.spec.ts
 * @description Smoke tests for AppComponent: verifies creation, title, and
 * that the current-user$ observable is exposed from AuthService.
 */
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { AppComponent } from './app.component';
import { AuthService }  from './services/auth.service';
import { TitleService } from './services/title.service';

describe('AppComponent', () => {
  let authSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    authSpy = jasmine.createSpyObj<AuthService>('AuthService', ['logout', 'isAuthenticated'], {
      currentUser$: new BehaviorSubject(null).asObservable(),
    });

    await TestBed.configureTestingModule({
      declarations: [AppComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService,  useValue: authSpy },
        { provide: TitleService, useValue: { setTitle: jasmine.createSpy() } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  it('should create the app component', () => {
    const fixture   = TestBed.createComponent(AppComponent);
    const component = fixture.componentInstance;
    expect(component).toBeTruthy();
  });

  it('should have the correct application title', () => {
    const fixture   = TestBed.createComponent(AppComponent);
    const component = fixture.componentInstance;
    expect(component.title).toContain('SIIGESE');
  });

  it('should expose currentUser$ observable from AuthService', () => {
    const fixture   = TestBed.createComponent(AppComponent);
    const component = fixture.componentInstance;
    expect(component.currentUser$).toBeTruthy();
  });
});
