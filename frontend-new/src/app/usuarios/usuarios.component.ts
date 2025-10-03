import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { User, CreateUser, UpdateUser, Role } from '../interfaces/user.interface';

@Component({
    selector: 'app-usuarios',
    templateUrl: './usuarios.component.html',
    styleUrls: ['./usuarios.component.scss'],
    standalone: false
})
export class UsuariosComponent implements OnInit, AfterViewInit {
  @ViewChild('formSection') formSection!: ElementRef;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  users: User[] = [];
  filteredUsers: User[] = [];
  availableRoles: Role[] = [];
  usersDataSource = new MatTableDataSource<User>([]);
  userForm: FormGroup;
  selectedUser: User | null = null;
  isEditMode = false;
  isLoading = false;
  searchTerm: string = '';
  currentUserId: number = 0;

  // Configuración de tabla
  displayedColumns: string[] = ['userName', 'fullName', 'roles', 'isActive', 'lastLoginAt', 'actions'];
  pageSize = 10;
  pageSizeOptions = [5, 10, 25, 50];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.userForm = this.createForm();
    this.currentUserId = this.authService.getCurrentUser()?.id || 0;
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      if (this.paginator) {
        this.usersDataSource.paginator = this.paginator;
      }
      if (this.sort) {
        this.usersDataSource.sort = this.sort;
      }
    }, 0);
  }

  createForm(): FormGroup {
    return this.fb.group({
      userName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      roleIds: [[], Validators.required],
      isActive: [true]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(control: AbstractControl): {[key: string]: any} | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      return { 'passwordMismatch': true };
    }
    return null;
  }

  loadUsers(): void {
    this.isLoading = true;
    this.apiService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.filteredUsers = [...users];
        this.usersDataSource.data = this.filteredUsers;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error al cargar usuarios:', error);
        this.showSnackBar('Error al cargar usuarios: ' + error.message, 'error');
        this.isLoading = false;
      }
    });
  }

  loadRoles(): void {

    this.apiService.getRoles().subscribe({
      next: (roles) => {

        this.availableRoles = roles;
      },
      error: (error) => {
        console.error('Error al cargar roles:', error);
        this.showSnackBar('Error al cargar roles: ' + error.message, 'error');
      }
    });
  }

  onSubmit(): void {
    if (!this.isAdmin()) {
      this.showSnackBar('No tienes permisos para realizar esta acción', 'error');
      return;
    }

    if (this.userForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading = true;
    const formValue = this.userForm.value;


    if (this.isEditMode && this.selectedUser) {
      const updateData: UpdateUser = {
        userName: formValue.userName,
        email: formValue.email,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        isActive: formValue.isActive,
        roleIds: formValue.roleIds
      };

      this.apiService.updateUser(this.selectedUser.id, updateData).subscribe({
        next: (updatedUser) => {
          this.showSnackBar('Usuario actualizado exitosamente', 'success');
          this.loadUsers();
          this.resetForm();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error al actualizar usuario:', error);
          this.showSnackBar('Error al actualizar usuario: ' + error.message, 'error');
          this.isLoading = false;
        }
      });
    } else {
      const createData: CreateUser = {
        userName: formValue.userName,
        email: formValue.email,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        password: formValue.password,
        isActive: formValue.isActive,
        roleIds: formValue.roleIds
      };



      this.apiService.createUser(createData).subscribe({
        next: (newUser) => {
          this.showSnackBar('Usuario creado exitosamente', 'success');
          this.loadUsers();
          this.resetForm();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error al crear usuario:', error);
          this.showSnackBar('Error al crear usuario: ' + error.message, 'error');
          this.isLoading = false;
        }
      });
    }
  }

  editUser(user: User): void {
    if (!this.isAdmin()) {
      this.showSnackBar('No tienes permisos para editar usuarios', 'error');
      return;
    }

    this.selectedUser = user;
    this.isEditMode = true;
    
    // Remover validadores de contraseña para edición
    this.userForm.get('password')?.clearValidators();
    this.userForm.get('confirmPassword')?.clearValidators();
    this.userForm.updateValueAndValidity();
    
    this.userForm.patchValue({
      userName: user.userName,
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      isActive: user.isActive,
      roleIds: user.roles.map(r => r.id)
    });
    this.scrollToForm();
  }

  deleteUser(user: User): void {
    if (!this.isAdmin()) {
      this.showSnackBar('No tienes permisos para eliminar usuarios', 'error');
      return;
    }

    if (user.id === this.currentUserId) {
      this.showSnackBar('No puedes eliminar tu propio usuario', 'error');
      return;
    }

    if (confirm(`¿Estás seguro de que deseas eliminar al usuario "${user.userName}"?`)) {
      this.isLoading = true;
      this.apiService.deleteUser(user.id).subscribe({
        next: () => {
          this.showSnackBar('Usuario eliminado exitosamente', 'success');
          this.loadUsers();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error al eliminar usuario:', error);
          this.showSnackBar('Error al eliminar usuario: ' + error.message, 'error');
          this.isLoading = false;
        }
      });
    }
  }

  selectUser(user: User): void {
    this.selectedUser = user;
  }

  cancelEdit(): void {
    this.resetForm();
  }

  resetForm(): void {
    this.isEditMode = false;
    this.selectedUser = null;
    this.userForm.reset();
    this.userForm = this.createForm();
  }

  scrollToForm(): void {
    if (this.formSection) {
      this.formSection.nativeElement.scrollIntoView({ 
        behavior: 'smooth', 
        block: 'start' 
      });
    }
  }

  isAdmin(): boolean {
    return this.authService.hasAnyRole(['Admin']);
  }

  applyFilter(): void {
    const filterValue = this.searchTerm.toLowerCase();
    this.filteredUsers = this.users.filter(user =>
      user.userName.toLowerCase().includes(filterValue) ||
      user.email.toLowerCase().includes(filterValue) ||
      user.firstName.toLowerCase().includes(filterValue) ||
      user.lastName.toLowerCase().includes(filterValue)
    );
    this.usersDataSource.data = this.filteredUsers;
    
    if (this.paginator) {
      this.paginator.firstPage();
    }
  }

  getRoleColor(roleName: string): 'primary' | 'accent' | 'warn' {
    switch (roleName.toLowerCase()) {
      case 'admin':
        return 'warn';
      case 'dataloader':
        return 'accent';
      default:
        return 'primary';
    }
  }

  get formTitle(): string {
    return this.isEditMode ? 'Editar Usuario' : 'Nuevo Usuario';
  }

  get submitButtonText(): string {
    return this.isEditMode ? 'Actualizar Usuario' : 'Crear Usuario';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.userForm.controls).forEach(key => {
      const control = this.userForm.get(key);
      control?.markAsTouched();
    });
  }

  private showSnackBar(message: string, type: 'success' | 'error' | 'info'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 5000,
      panelClass: [`snackbar-${type}`],
      horizontalPosition: 'end',
      verticalPosition: 'top'
    });
  }
}
