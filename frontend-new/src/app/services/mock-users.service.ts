import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User, Role } from '../interfaces/user.interface';

@Injectable({
  providedIn: 'root'
})
export class MockUsersService {
  private mockUsers: User[] = [
    {
      id: 1,
      email: 'admin@sinseg.com',
      firstName: 'Administrador',
      lastName: 'Sistema',
      userName: 'admin',
      isActive: true,
      roles: [
        {
          id: 1,
          name: 'Admin',
          description: 'Administrador del sistema',
          permissions: ['read', 'write', 'delete', 'admin'],
          isActive: true
        }
      ],
      lastLoginAt: '2024-10-28T10:00:00',
      createdAt: '2024-01-01T00:00:00',
      updatedAt: '2024-10-28T10:00:00'
    },
    {
      id: 2,
      email: 'operador@sinseg.com',
      firstName: 'Juan',
      lastName: 'Operador',
      userName: 'joperador',
      isActive: true,
      roles: [
        {
          id: 2,
          name: 'Operador',
          description: 'Operador del sistema',
          permissions: ['read', 'write'],
          isActive: true
        }
      ],
      lastLoginAt: '2024-10-27T14:30:00',
      createdAt: '2024-02-15T00:00:00',
      updatedAt: '2024-10-27T14:30:00'
    },
    {
      id: 3,
      email: 'supervisor@sinseg.com',
      firstName: 'María',
      lastName: 'Supervisora',
      userName: 'msupervisora',
      isActive: true,
      roles: [
        {
          id: 3,
          name: 'Supervisor',
          description: 'Supervisor del sistema',
          permissions: ['read', 'write', 'approve'],
          isActive: true
        }
      ],
      lastLoginAt: '2024-10-28T08:15:00',
      createdAt: '2024-03-01T00:00:00',
      updatedAt: '2024-10-28T08:15:00'
    },
    {
      id: 4,
      email: 'dataloader@sinseg.com',
      firstName: 'Carlos',
      lastName: 'Cargador',
      userName: 'ccargador',
      isActive: true,
      roles: [
        {
          id: 4,
          name: 'DataLoader',
          description: 'Cargador de datos',
          permissions: ['read', 'upload'],
          isActive: true
        }
      ],
      lastLoginAt: '2024-10-26T16:45:00',
      createdAt: '2024-04-10T00:00:00',
      updatedAt: '2024-10-26T16:45:00'
    },
    {
      id: 5,
      email: 'usuario.inactivo@sinseg.com',
      firstName: 'Usuario',
      lastName: 'Inactivo',
      userName: 'uinactivo',
      isActive: false,
      roles: [
        {
          id: 2,
          name: 'Operador',
          description: 'Operador del sistema',
          permissions: ['read'],
          isActive: true
        }
      ],
      lastLoginAt: '2024-09-15T10:20:00',
      createdAt: '2024-05-20T00:00:00',
      updatedAt: '2024-09-15T10:20:00'
    }
  ];

  private mockRoles: Role[] = [
    {
      id: 1,
      name: 'Admin',
      description: 'Administrador del sistema',
      permissions: ['read', 'write', 'delete', 'admin'],
      isActive: true
    },
    {
      id: 2,
      name: 'Operador',
      description: 'Operador del sistema',
      permissions: ['read', 'write'],
      isActive: true
    },
    {
      id: 3,
      name: 'Supervisor',
      description: 'Supervisor del sistema',
      permissions: ['read', 'write', 'approve'],
      isActive: true
    },
    {
      id: 4,
      name: 'DataLoader',
      description: 'Cargador de datos',
      permissions: ['read', 'upload'],
      isActive: true
    }
  ];

  getUsers(): Observable<User[]> {
    console.log('🧪 MockUsersService: Devolviendo usuarios mock');
    return of([...this.mockUsers]);
  }

  getRoles(): Observable<Role[]> {
    console.log('🧪 MockUsersService: Devolviendo roles mock');
    return of([...this.mockRoles]);
  }

  createUser(user: any): Observable<User> {
    const newUser: User = {
      id: Math.max(...this.mockUsers.map(u => u.id)) + 1,
      ...user,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };
    this.mockUsers.push(newUser);
    console.log('🧪 MockUsersService: Usuario creado', newUser);
    return of(newUser);
  }

  updateUser(id: number, user: any): Observable<User> {
    const index = this.mockUsers.findIndex(u => u.id === id);
    if (index !== -1) {
      this.mockUsers[index] = {
        ...this.mockUsers[index],
        ...user,
        updatedAt: new Date().toISOString()
      };
      console.log('🧪 MockUsersService: Usuario actualizado', this.mockUsers[index]);
      return of(this.mockUsers[index]);
    }
    throw new Error('Usuario no encontrado');
  }

  deleteUser(id: number): Observable<any> {
    const index = this.mockUsers.findIndex(u => u.id === id);
    if (index !== -1) {
      const deletedUser = this.mockUsers.splice(index, 1)[0];
      console.log('🧪 MockUsersService: Usuario eliminado', deletedUser);
      return of({ message: 'Usuario eliminado exitosamente' });
    }
    throw new Error('Usuario no encontrado');
  }
}