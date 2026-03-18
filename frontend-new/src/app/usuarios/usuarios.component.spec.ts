import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { BehaviorSubject } from 'rxjs';

import { UsuariosComponent } from './usuarios.component';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';

describe('UsuariosComponent', () => {
  let component: UsuariosComponent;
  let fixture: ComponentFixture<UsuariosComponent>;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    apiServiceSpy = jasmine.createSpyObj('ApiService', [
      'getUsers', 'getRoles', 'createUser', 'updateUser', 'deleteUser'
    ]);
    authServiceSpy = jasmine.createSpyObj('AuthService',
      ['getCurrentUserId', 'hasRole', 'hasPermission', 'hasAnyRole', 'logout', 'isAdmin', 'canUploadExcel'],
      {
        isAuthenticated$: new BehaviorSubject(true),
        currentUser$: new BehaviorSubject({ id: 1, email: 'admin@sinseg.cr', role: 'Admin' })
      }
    );

    apiServiceSpy.getUsers.and.returnValue(of([]));
    apiServiceSpy.getRoles.and.returnValue(of([]));
    authServiceSpy.getCurrentUserId.and.returnValue(42);
    authServiceSpy.hasPermission.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(true);
    authServiceSpy.isAdmin.and.returnValue(false);
    authServiceSpy.canUploadExcel.and.returnValue(false);

    await TestBed.configureTestingModule({
      imports: [UsuariosComponent],
      providers: [
        { provide: ApiService,  useValue: apiServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        provideNoopAnimations()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UsuariosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call getUsers on initialization', () => {
    expect(apiServiceSpy.getUsers).toHaveBeenCalledTimes(1);
  });

  it('should call getRoles on initialization', () => {
    expect(apiServiceSpy.getRoles).toHaveBeenCalledTimes(1);
  });

  it('should set currentUserId from authService.getCurrentUserId()', () => {
    expect(authServiceSpy.getCurrentUserId).toHaveBeenCalled();
    expect(component.currentUserId).toBe(42);
  });

  it('should start with an empty users array', () => {
    expect(component.users).toEqual([]);
  });

  it('should call getRoles on initialization', () => {
    expect(apiServiceSpy.getRoles).toHaveBeenCalledTimes(1);
  });
});
