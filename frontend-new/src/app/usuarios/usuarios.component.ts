import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialogModule } from '@angular/material/dialog';
import { User, CreateUser, UpdateUser, Role } from '../interfaces/user.interface';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { MockUsersService } from '../services/mock-users.service';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatSelectModule,
    MatCheckboxModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatDialogModule,
    DatePipe
  ],
  templateUrl: './usuarios.component.html',
  styleUrls: ['./usuarios.component.scss']
})
export class UsuariosComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  users: User[] = [];
  filteredUsers: User[] = [];
  usersDataSource = new MatTableDataSource<User>([]);
  availableRoles: Role[] = [];
  
  userForm!: FormGroup;
  searchTerm = '';
  isLoading = false;
  isEditMode = false;
  selectedUser: User | null = null;
  currentUserId: number | null = null;
  
  displayedColumns: string[] = ['userName', 'fullName', 'roles', 'isActive', 'lastLoginAt', 'actions'];
  pageSizeOptions = [5, 10, 25, 50];
  pageSize = 10;

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService,
    private mockUsersService: MockUsersService
  ) {
    this.initForm();
  }

  ngOnInit() {
    this.loadUsers();
    this.loadRoles();
    this.currentUserId = this.authService.getCurrentUserId();
  }

  ngAfterViewInit() {
    // Configurar paginator después de que la vista esté inicializada
    if (this.paginator) {
      this.usersDataSource.paginator = this.paginator;
      console.log('✅ Paginator configurado correctamente');
    } else {
      console.warn('⚠️ Paginator no encontrado');
    }
  }

  private initForm() {
    this.userForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      userName: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      roleIds: [[], [Validators.required]],
      isActive: [true]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(group: FormGroup) {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  get submitButtonText(): string {
    return this.isEditMode ? 'Actualizar Usuario' : 'Crear Usuario';
  }

  get formTitle(): string {
    return this.isEditMode ? 'Editar Usuario' : 'Crear Nuevo Usuario';
  }

  isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  isFormValidForMode(): boolean {
    if (this.isEditMode) {
      return this.userForm.get('firstName')?.valid &&
             this.userForm.get('lastName')?.valid &&
             this.userForm.get('email')?.valid &&
             this.userForm.get('userName')?.valid &&
             this.userForm.get('roleIds')?.valid || false;
    }
    return this.userForm.valid;
  }

  loadUsers() {
    this.isLoading = true;
    
    // Usar temporalmente el servicio mock debido a problemas de autenticación
    console.log('📱 UsuariosComponent: Usando MockUsersService temporalmente');
    this.mockUsersService.getUsers().subscribe({
      next: (users) => {
        console.log('✅ Usuarios cargados desde mock:', users);
        console.log('📊 Total usuarios recibidos:', users.length);
        
        this.users = users;
        this.filteredUsers = users;
        this.usersDataSource.data = users;
        
        console.log('📋 DataSource actualizado:', this.usersDataSource.data);
        console.log('📋 Usuarios array:', this.users);
        console.log('📋 Usuarios filtrados:', this.filteredUsers);
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error loading users from mock:', error);
        this.isLoading = false;
      }
    });
    
    // Código original comentado para debugging
    // this.apiService.getUsers().subscribe({
    //   next: (users) => {
    //     this.users = users;
    //     this.filteredUsers = users;
    //     this.usersDataSource.data = users;
    //     this.isLoading = false;
    //   },
    //   error: (error) => {
    //     console.error('Error loading users:', error);
    //     this.isLoading = false;
    //   }
    // });
  }

  loadRoles() {
    // Usar temporalmente el servicio mock
    this.mockUsersService.getRoles().subscribe({
      next: (roles) => {
        console.log('✅ Roles cargados desde mock:', roles);
        this.availableRoles = roles;
      },
      error: (error) => {
        console.error('❌ Error loading roles from mock:', error);
      }
    });
    
    // Código original comentado
    // this.apiService.getRoles().subscribe({
    //   next: (roles) => {
    //     this.availableRoles = roles;
    //   },
    //   error: (error) => {
    //     console.error('Error loading roles:', error);
    //   }
    // });
  }

  applyFilter() {
    const filterValue = this.searchTerm.toLowerCase();
    this.filteredUsers = this.users.filter(user => 
      this.filterUsers(user, filterValue)
    );
    this.usersDataSource.data = this.filteredUsers;
  }
  
  private filterUsers(user: User, filterValue: string): boolean {
    return (
      user.firstName.toLowerCase().includes(filterValue) ||
      user.lastName.toLowerCase().includes(filterValue) ||
      user.email.toLowerCase().includes(filterValue) ||
      (user.userName && user.userName.toLowerCase().includes(filterValue)) ||
      false
    );
  }

  newUser() {
    this.isEditMode = false;
    this.selectedUser = null;
    this.initForm();
  }

  editUser(user: User) {
    this.isEditMode = true;
    this.selectedUser = user;
    this.loadUserToForm(user);
  }

  selectUser(user: User) {
    this.selectedUser = user;
  }

  private loadUserToForm(user: User) {
    this.userForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      userName: user.userName,
      roleIds: user.roles?.map(role => role.id) || [],
      isActive: user.isActive
    });
    
    // Remove password requirements for edit mode
    this.userForm.get('password')?.clearValidators();
    this.userForm.get('confirmPassword')?.clearValidators();
    this.userForm.updateValueAndValidity();
  }

  cancelEdit() {
    this.isEditMode = false;
    this.selectedUser = null;
    this.initForm();
  }

  resetForm() {
    this.userForm.reset();
    this.userForm.get('isActive')?.setValue(true);
  }

  deleteUser(user: User) {
    if (confirm(`¿Está seguro de eliminar al usuario ${user.firstName} ${user.lastName}?`)) {
      this.isLoading = true;
      this.apiService.deleteUser(user.id).subscribe({
        next: () => {
          this.loadUsers();
          console.log('Usuario eliminado exitosamente');
        },
        error: (error) => {
          console.error('Error deleting user:', error);
          this.isLoading = false;
        }
      });
    }
  }

  getRoleColor(roleName: string): string {
    switch (roleName.toLowerCase()) {
      case 'admin': return 'primary';
      case 'manager': return 'accent';
      case 'user': return 'warn';
      default: return 'primary';
    }
  }

  onSubmit() {
    if (this.isFormValidForMode()) {
      this.isLoading = true;
      
      if (this.isEditMode && this.selectedUser) {
        this.updateUser();
      } else {
        this.createUser();
      }
    }
  }

  private createUser() {
    const userData: CreateUser = this.userForm.value;
    this.apiService.createUser(userData).subscribe({
      next: (user) => {
        this.loadUsers();
        this.resetForm();
        console.log('Usuario creado exitosamente');
      },
      error: (error) => {
        console.error('Error creating user:', error);
        this.isLoading = false;
      }
    });
  }

  private updateUser() {
    if (!this.selectedUser) return;
    
    const userData: UpdateUser = {
      firstName: this.userForm.get('firstName')?.value,
      lastName: this.userForm.get('lastName')?.value,
      email: this.userForm.get('email')?.value,
      userName: this.userForm.get('userName')?.value,
      roleIds: this.userForm.get('roleIds')?.value,
      isActive: this.userForm.get('isActive')?.value
    };

    this.apiService.updateUser(this.selectedUser.id, userData).subscribe({
      next: (user) => {
        this.loadUsers();
        this.cancelEdit();
        console.log('Usuario actualizado exitosamente');
      },
      error: (error) => {
        console.error('Error updating user:', error);
        this.isLoading = false;
      }
    });
  }
}
