import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { EstadoCobro, MetodoPago } from '../cobros/interfaces/cobro.interface';
import { TipoReclamo, EstadoReclamo, PrioridadReclamo } from '../reclamos/interfaces/reclamo.interface';
import { environment } from '../../environments/environment';

@Injectable()
export class MockApiInterceptor implements HttpInterceptor {
  private users = [
    {
      id: 1,
      userName: 'admin',
      email: 'admin@sinseg.com',
      password: 'password123',
      firstName: 'Admin',
      lastName: 'SINSEG',
      isActive: true,
      roles: [
        { id: 1, name: 'Admin', description: 'Administrador' },
        { id: 2, name: 'DataLoader', description: 'Cargador de Datos' }
      ],
      createdAt: new Date('2024-01-01'),
      lastLoginAt: new Date()
    },
    {
      id: 2,
      userName: 'usuario',
      email: 'user@sinseg.com',
      password: 'password123',
      firstName: 'Usuario',
      lastName: 'SINSEG',
      isActive: true,
      roles: [{ id: 3, name: 'User', description: 'Usuario' }],
      createdAt: new Date('2024-01-15'),
      lastLoginAt: new Date(Date.now() - 86400000) // 1 día atrás
    },
    {
      id: 3,
      userName: 'dataloader',
      email: 'dataloader@sinseg.com',
      password: 'password123',
      firstName: 'Cargador',
      lastName: 'Datos',
      isActive: true,
      roles: [{ id: 2, name: 'DataLoader', description: 'Cargador de Datos' }],
      createdAt: new Date('2024-02-01'),
      lastLoginAt: new Date(Date.now() - 3600000) // 1 hora atrás
    }
  ];

  private cobros = [
    {
      id: 1,
      numeroRecibo: 'REC-2024-001',
      polizaId: 1,
      numeroPoliza: 'POL-VEH-2024-001',
      clienteNombre: 'Juan Carlos',
      clienteApellido: 'Pérez García',
      fechaVencimiento: new Date('2024-12-15'),
      fechaCobro: new Date('2024-12-10'),
      montoTotal: 150000,
      montoCobrado: 150000,
      moneda: 'CRC',
      estado: EstadoCobro.Cobrado,
      metodoPago: MetodoPago.Transferencia,
      observaciones: 'Pago realizado exitosamente',
      usuarioCobroId: 1,
      usuarioCobroNombre: 'Admin SINSEG',
      fechaCreacion: new Date('2024-11-15'),
      fechaActualizacion: new Date('2024-12-10')
    },
    {
      id: 2,
      numeroRecibo: 'REC-2024-002',
      polizaId: 2,
      numeroPoliza: 'POL-VEH-2024-002',
      clienteNombre: 'María Elena',
      clienteApellido: 'Rodríguez López',
      fechaVencimiento: new Date('2024-12-20'),
      montoTotal: 225000,
      moneda: 'CRC',
      estado: EstadoCobro.Pendiente,
      observaciones: 'Pendiente de pago',
      fechaCreacion: new Date('2024-11-20'),
    },
    {
      id: 3,
      numeroRecibo: 'REC-2024-003',
      polizaId: 3,
      numeroPoliza: 'POL-VEH-2024-003',
      clienteNombre: 'Carlos Alberto',
      clienteApellido: 'González Mora',
      fechaVencimiento: new Date('2024-11-30'),
      montoTotal: 180000,
      moneda: 'CRC',
      estado: EstadoCobro.Vencido,
      observaciones: 'Cobro vencido - Requiere seguimiento',
      fechaCreacion: new Date('2024-10-30'),
    },
    {
      id: 4,
      numeroRecibo: 'REC-2024-004',
      polizaId: 4,
      numeroPoliza: 'POL-VEH-2024-004',
      clienteNombre: 'Ana Lucía',
      clienteApellido: 'Vargas Solano',
      fechaVencimiento: new Date('2024-12-25'),
      montoTotal: 320000,
      moneda: 'CRC',
      estado: EstadoCobro.Pendiente,
      observaciones: 'Cliente contactado - Confirma pago próximamente',
      fechaCreacion: new Date('2024-11-25'),
    },
    {
      id: 5,
      numeroRecibo: 'REC-2024-005',
      polizaId: 5,
      numeroPoliza: 'POL-VEH-2024-005',
      clienteNombre: 'Roberto',
      clienteApellido: 'Jiménez Castro',
      fechaVencimiento: new Date('2024-11-25'),
      montoTotal: 275000,
      moneda: 'CRC',
      estado: EstadoCobro.Vencido,
      observaciones: 'Cobro vencido - Cliente no contactado',
      fechaCreacion: new Date('2024-10-25'),
    }
  ];

  private reclamos = [
    {
      id: 1,
      numeroReclamo: 'REC-2024-001',
      polizaId: 1,
      numeroPoliza: 'POL-VEH-2024-001',
      clienteNombre: 'Juan Carlos',
      clienteApellido: 'Pérez García',
      fechaReclamo: new Date('2024-11-01').toISOString(),
      fechaResolucion: new Date('2024-11-15').toISOString(),
      tipoReclamo: TipoReclamo.Siniestro,
      estado: EstadoReclamo.Resuelto,
      descripcion: 'Daños en vehículo por colisión. Solicitud de reparación completa del parachoques delantero y faros.',
      montoReclamado: 850000,
      montoAprobado: 750000,
      moneda: 'CRC',
      prioridad: PrioridadReclamo.Alta,
      observaciones: 'Reclamo procesado y aprobado. Diferencia por depreciación aplicada.',
      usuarioAsignadoId: 1,
      usuarioAsignadoNombre: 'Admin SINSEG',
      fechaLimiteRespuesta: new Date('2024-11-30').toISOString(),
      createdAt: new Date('2024-11-01').toISOString(),
      updatedAt: new Date('2024-11-15').toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    },
    {
      id: 2,
      numeroReclamo: 'REC-2024-002',
      polizaId: 2,
      numeroPoliza: 'POL-VEH-2024-002',
      clienteNombre: 'María Elena',
      clienteApellido: 'Rodríguez López',
      fechaReclamo: new Date('2024-11-10').toISOString(),
      tipoReclamo: TipoReclamo.Servicio,
      estado: EstadoReclamo.EnProceso,
      descripcion: 'Solicitud de cambio de talleres autorizados. Cliente requiere taller más cercano a su domicilio.',
      montoReclamado: 0,
      moneda: 'CRC',
      prioridad: PrioridadReclamo.Media,
      observaciones: 'En revisión - Validando talleres disponibles en la zona.',
      usuarioAsignadoId: 1,
      usuarioAsignadoNombre: 'Admin SINSEG',
      fechaLimiteRespuesta: new Date('2024-12-10').toISOString(),
      createdAt: new Date('2024-11-10').toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    },
    {
      id: 3,
      numeroReclamo: 'REC-2024-003',
      polizaId: 3,
      numeroPoliza: 'POL-VEH-2024-003',
      clienteNombre: 'Carlos Alberto',
      clienteApellido: 'González Mora',
      fechaReclamo: new Date('2024-10-25').toISOString(),
      tipoReclamo: TipoReclamo.Facturacion,
      estado: EstadoReclamo.Abierto,
      descripcion: 'Discrepancia en el monto facturado. Cliente indica que se aplicó tarifa incorrecta en la renovación.',
      montoReclamado: 45000,
      moneda: 'CRC',
      prioridad: PrioridadReclamo.Baja,
      observaciones: 'Pendiente de revisión por área de facturación.',
      fechaLimiteRespuesta: new Date('2024-11-25').toISOString(),
      createdAt: new Date('2024-10-25').toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    },
    {
      id: 4,
      numeroReclamo: 'REC-2024-004',
      polizaId: 4,
      numeroPoliza: 'POL-VEH-2024-004',
      clienteNombre: 'Ana Lucía',
      clienteApellido: 'Vargas Solano',
      fechaReclamo: new Date('2024-11-05').toISOString(),
      tipoReclamo: TipoReclamo.Cobertura,
      estado: EstadoReclamo.Escalado,
      descripcion: 'Consulta sobre cobertura de daños por fenómenos naturales. Cliente requiere aclaración sobre alcance de la póliza.',
      montoReclamado: 0,
      moneda: 'CRC',
      prioridad: PrioridadReclamo.Critica,
      observaciones: 'Escalado a supervisión por complejidad del caso.',
      usuarioAsignadoId: 1,
      usuarioAsignadoNombre: 'Admin SINSEG',
      fechaLimiteRespuesta: new Date('2024-11-20').toISOString(),
      createdAt: new Date('2024-11-05').toISOString(),
      updatedAt: new Date('2024-11-18').toISOString(),
      createdBy: 'Sistema',
      updatedBy: 'Admin',
      isDeleted: false
    },
    {
      id: 5,
      numeroReclamo: 'REC-2024-005',
      polizaId: 5,
      numeroPoliza: 'POL-VEH-2024-005',
      clienteNombre: 'Roberto',
      clienteApellido: 'Jiménez Castro',
      fechaReclamo: new Date('2024-10-15').toISOString(),
      fechaResolucion: new Date('2024-10-20').toISOString(),
      tipoReclamo: TipoReclamo.Proceso,
      estado: EstadoReclamo.Rechazado,
      descripcion: 'Solicitud de reembolso por cancelación de póliza fuera de términos contractuales.',
      montoReclamado: 320000,
      montoAprobado: 0,
      moneda: 'CRC',
      prioridad: PrioridadReclamo.Media,
      observaciones: 'Rechazado - Cancelación solicitada fuera del período de gracia establecido en contrato.',
      usuarioAsignadoId: 1,
      usuarioAsignadoNombre: 'Admin SINSEG',
      fechaLimiteRespuesta: new Date('2024-10-30').toISOString(),
      createdAt: new Date('2024-10-15').toISOString(),
      updatedAt: new Date('2024-10-20').toISOString(),
      createdBy: 'Sistema',
      updatedBy: 'Admin',
      isDeleted: false
    }
  ];

  // Mock Email Configurations
  private emailConfigs = [
    {
      id: 1,
      configName: 'Gmail SINSEG',
      smtpServer: 'smtp.gmail.com',
      smtpPort: 587,
      username: 'notifications@sinseg.com',
      password: 'app_password_123',
      useSSL: false,
      useTLS: true,
      fromEmail: 'notifications@sinseg.com',
      fromName: 'SINSEG Notifications',
      timeoutSeconds: 30,
      maxRetries: 3,
      isActive: true,
      isDefault: true,
      createdAt: new Date('2024-01-15').toISOString(),
      updatedAt: new Date('2024-11-01').toISOString(),
      createdBy: 'Admin',
      updatedBy: 'Admin'
    },
    {
      id: 2,
      configName: 'Outlook Backup',
      smtpServer: 'smtp-mail.outlook.com',
      smtpPort: 587,
      username: 'backup@sinseg.com',
      password: 'outlook_password_456',
      useSSL: false,
      useTLS: true,
      fromEmail: 'backup@sinseg.com',
      fromName: 'SINSEG Backup',
      timeoutSeconds: 45,
      maxRetries: 2,
      isActive: false,
      isDefault: false,
      createdAt: new Date('2024-02-01').toISOString(),
      updatedAt: new Date('2024-10-15').toISOString(),
      createdBy: 'Admin',
      updatedBy: 'DataLoader'
    }
  ];

  private roles = [
    {
      id: 1,
      name: 'Admin',
      description: 'Administrador del sistema con acceso completo',
      permissions: [
        'users.read', 'users.write', 'users.delete',
        'polizas.read', 'polizas.write', 'polizas.delete',
        'cotizaciones.read', 'cotizaciones.write', 'cotizaciones.delete',
        'reclamos.read', 'reclamos.write', 'reclamos.delete',
        'cobros.read', 'cobros.write', 'cobros.delete',
        'config.read', 'config.write', 'config.delete',
        'reports.read', 'reports.write'
      ],
      isActive: true,
      createdAt: new Date('2024-01-01').toISOString(),
      updatedAt: new Date('2024-10-22').toISOString()
    },
    {
      id: 2,
      name: 'DataLoader',
      description: 'Cargador de datos con permisos para gestionar información',
      permissions: [
        'polizas.read', 'polizas.write',
        'cotizaciones.read', 'cotizaciones.write',
        'reclamos.read', 'reclamos.write',
        'cobros.read', 'cobros.write',
        'reports.read'
      ],
      isActive: true,
      createdAt: new Date('2024-01-01').toISOString(),
      updatedAt: new Date('2024-10-22').toISOString()
    },
    {
      id: 3,
      name: 'User',
      description: 'Usuario estándar con permisos de lectura',
      permissions: [
        'polizas.read',
        'cotizaciones.read',
        'reclamos.read',
        'cobros.read'
      ],
      isActive: true,
      createdAt: new Date('2024-01-01').toISOString(),
      updatedAt: new Date('2024-10-22').toISOString()
    },
    {
      id: 4,
      name: 'Support',
      description: 'Soporte técnico con permisos específicos',
      permissions: [
        'reclamos.read', 'reclamos.write',
        'polizas.read',
        'cotizaciones.read',
        'reports.read'
      ],
      isActive: true,
      createdAt: new Date('2024-01-01').toISOString(),
      updatedAt: new Date('2024-10-22').toISOString()
    },
    {
      id: 5,
      name: 'Billing',
      description: 'Facturación con permisos de cobros',
      permissions: [
        'cobros.read', 'cobros.write',
        'polizas.read',
        'reports.read'
      ],
      isActive: true,
      createdAt: new Date('2024-01-01').toISOString(),
      updatedAt: new Date('2024-10-22').toISOString()
    }
  ];

  private cotizaciones = [
    {
      id: 1,
      nombreSolicitante: 'Juan Carlos Pérez García',
      email: 'juan.perez@gmail.com',
      telefono: '2234-5678',
      tipoSeguro: 'Vehículo',
      aseguradora: 'Instituto Nacional de Seguros (INS)',
      montoAsegurado: 15000000,
      primaCotizada: 180000,
      moneda: 'CRC',
      fechaCotizacion: new Date('2024-11-20').toISOString(),
      fechaVencimiento: new Date('2024-12-20').toISOString(),
      estado: 'Pendiente',
      observaciones: 'Cotización para vehículo Toyota Corolla 2020',
      // Datos específicos para vehículo
      marca: 'Toyota',
      modelo: 'Corolla',
      año: 2020,
      placa: 'BCH-123',
      
      createdAt: new Date('2024-11-20').toISOString(),
      updatedAt: new Date('2024-11-20').toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    },
    {
      id: 2,
      nombreSolicitante: 'María Elena Rodríguez López',
      email: 'maria.rodriguez@hotmail.com',
      telefono: '8765-4321',
      tipoSeguro: 'Vida',
      aseguradora: 'ASSA Compañía de Seguros S.A.',
      montoAsegurado: 50000000,
      primaCotizada: 95000,
      moneda: 'CRC',
      fechaCotizacion: new Date('2024-11-18').toISOString(),
      fechaVencimiento: new Date('2024-12-18').toISOString(),
      estado: 'Aprobada',
      observaciones: 'Seguro de vida con cobertura completa',
      // Datos específicos para vida
      fechaNacimiento: '1985-03-15',
      genero: 'Femenino',
      ocupacion: 'Contadora',
      
      createdAt: new Date('2024-11-18').toISOString(),
      updatedAt: new Date('2024-11-19').toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    },
    {
      id: 3,
      nombreSolicitante: 'Carlos Alberto González Mora',
      email: 'carlos.gonzalez@empresa.com',
      telefono: '2789-1234',
      tipoSeguro: 'Hogar',
      aseguradora: 'Pan-American Life Insurance de Costa Rica, S.A. (PALIG)',
      montoAsegurado: 85000000,
      primaCotizada: 125000,
      moneda: 'CRC',
      fechaCotizacion: new Date('2024-11-15').toISOString(),
      fechaVencimiento: new Date('2024-12-15').toISOString(),
      estado: 'En Revisión',
      observaciones: 'Seguro para casa de habitación en San José',
      // Datos específicos para hogar
      direccionInmueble: 'San José, Escazú, Residencial Los Laureles',
      tipoInmueble: 'Casa',
      valorInmueble: 85000000,
      
      createdAt: new Date('2024-11-15').toISOString(),
      updatedAt: new Date('2024-11-16').toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    },
    {
      id: 4,
      nombreSolicitante: 'Ana Lucía Vargas Solano',
      email: 'ana.vargas@gmail.com',
      telefono: '8456-7890',
      tipoSeguro: 'Salud',
      aseguradora: 'Davivienda Seguros (Costa Rica)',
      montoAsegurado: 25000000,
      primaCotizada: 78000,
      moneda: 'CRC',
      fechaCotizacion: new Date('2024-11-10').toISOString(),
      fechaVencimiento: new Date('2024-12-10').toISOString(),
      estado: 'Vencida',
      observaciones: 'Seguro de salud familiar - 4 personas',
      // Datos específicos para salud
      fechaNacimiento: '1978-08-22',
      genero: 'Femenino',
      ocupacion: 'Médica',
      
      createdAt: new Date('2024-11-10').toISOString(),
      updatedAt: new Date('2024-11-11').toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    },
    {
      id: 5,
      nombreSolicitante: 'Roberto Jiménez Castro',
      email: 'roberto.jimenez@outlook.com',
      telefono: '2345-6789',
      tipoSeguro: 'Vehículo',
      aseguradora: 'MNK Seguros Compañía Aseguradora',
      montoAsegurado: 8500000,
      primaCotizada: 145000,
      moneda: 'CRC',
      fechaCotizacion: new Date('2024-11-22').toISOString(),
      fechaVencimiento: new Date('2024-12-22').toISOString(),
      estado: 'Pendiente',
      observaciones: 'Motocicleta Honda CBR 600',
      // Datos específicos para vehículo
      marca: 'Honda',
      modelo: 'CBR 600',
      año: 2019,
      placa: 'M-456789',
      
      createdAt: new Date('2024-11-22').toISOString(),
      updatedAt: new Date('2024-11-22').toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    }
  ];

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Si useMockApi es false, pasar directamente al backend real
    if (!environment.useMockApi) {
      return next.handle(req);
    }

    const { url, method } = req;

    console.log('🔍 Mock Interceptor - URL:', url, 'Method:', method);
    console.log('🔍 URL includes /polizas/upload:', url.includes('/polizas/upload'));
    console.log('🔍 URL includes /api/polizas/upload:', url.includes('/api/polizas/upload'));
    console.log('🔍 Method is POST:', method === 'POST');

    // Solo interceptar si la URL contiene /api/
    if (!url.includes('/api/')) {
      console.log('⚠️ Not an API call, passing through:', url);
      return next.handle(req);
    }

    console.log('🔄 Mock API Interceptor:', method, url);

    // Login endpoint
    if (url.includes('/auth/login') && method === 'POST') {
      console.log('🔑 Mock handling login');
      return this.handleLogin(req);
    }

    // UPLOAD ENDPOINT - PASAR AL BACKEND SIN INTERCEPTAR
    if ((url.includes('/polizas/upload') || url.includes('/api/polizas/upload')) && method === 'POST') {
      console.log('� UPLOAD DETECTED - BYPASSING INTERCEPTOR, passing to backend');
      console.log('� Upload URL will not be mocked:', url);
      return next.handle(req);
    }
    
    if (url.includes('/polizas')) {
      if (method === 'GET') {
        console.log('📋 Mock handling GET polizas');
        return this.handlePolizas(req);
      } else if (method === 'POST') {
        console.log('➕ Mock handling CREATE poliza');
        return this.handleCreatePoliza(req);
      } else if (method === 'PUT') {
        console.log('✏️ Mock handling UPDATE poliza');
        return this.handleUpdatePoliza(req);
      } else if (method === 'DELETE') {
        console.log('🗑️ Mock handling DELETE poliza');
        return this.handleDeletePoliza(req);
      }
    }

    // Cobros endpoints
    if (url.includes('/cobros')) {
      if (method === 'GET') {
        if (url.includes('/stats')) {
          console.log('📊 Mock handling GET cobros stats');
          return this.handleCobroStats(req);
        } else {
          // Check if it's a request for a specific cobro by ID
          const idMatch = url.match(/\/cobros\/(\d+)$/);
          if (idMatch) {
            console.log('� Mock handling GET cobro by ID:', idMatch[1]);
            return this.handleGetCobroById(req, parseInt(idMatch[1]));
          } else {
            console.log('�💰 Mock handling GET cobros');
            return this.handleGetCobros(req);
          }
        }
      } else if (method === 'POST') {
        console.log('➕ Mock handling CREATE cobro');
        return this.handleCreateCobro(req);
      } else if (method === 'PUT') {
        if (url.includes('/registrar')) {
          console.log('💳 Mock handling REGISTRAR cobro');
          return this.handleRegistrarCobro(req);
        } else if (url.includes('/cancelar')) {
          console.log('❌ Mock handling CANCELAR cobro');
          return this.handleCancelarCobro(req);
        }
      }
    }

    // Reclamos endpoints
    if (url.includes('/reclamos')) {
      if (method === 'GET') {
        if (url.includes('/stats')) {
          console.log('📊 Mock handling GET reclamos stats');
          return this.handleReclamosStats(req);
        } else if (url.match(/\/reclamos\/\d+$/)) {
          console.log('🔍 Mock handling GET reclamo by ID');
          return this.handleGetReclamoById(req);
        } else {
          console.log('📋 Mock handling GET reclamos');
          return this.handleGetReclamos(req);
        }
      } else if (method === 'POST') {
        console.log('➕ Mock handling CREATE reclamo');
        return this.handleCreateReclamo(req);
      } else if (method === 'PUT') {
        if (url.includes('/estado')) {
          console.log('🔄 Mock handling CHANGE ESTADO reclamo');
          return this.handleChangeEstadoReclamo(req);
        } else if (url.includes('/asignar')) {
          console.log('👤 Mock handling ASIGNAR reclamo');
          return this.handleAsignarReclamo(req);
        } else if (url.includes('/resolver')) {
          console.log('✅ Mock handling RESOLVER reclamo');
          return this.handleResolverReclamo(req);
        } else if (url.includes('/rechazar')) {
          console.log('❌ Mock handling RECHAZAR reclamo');
          return this.handleRechazarReclamo(req);
        } else {
          console.log('✏️ Mock handling UPDATE reclamo');
          return this.handleUpdateReclamo(req);
        }
      } else if (method === 'DELETE') {
        console.log('🗑️ Mock handling DELETE reclamo');
        return this.handleDeleteReclamo(req);
      }
    }

    // Email Config endpoints  
    if (url.includes('/emailconfig')) {
      if (method === 'GET') {
        console.log('📧 Mock handling GET emailconfig');
        return this.handleGetEmailConfig(req);
      } else if (method === 'POST') {
        if (url.includes('/test-direct')) {
          console.log('🧪 Mock handling TEST DIRECT emailconfig');
          return this.handleTestEmailConfigDirect(req);
        } else if (url.includes('/test')) {
          console.log('🧪 Mock handling TEST emailconfig');
          return this.handleTestEmailConfig(req);
        } else {
          console.log('➕ Mock handling CREATE emailconfig');
          return this.handleCreateEmailConfig(req);
        }
      } else if (method === 'PUT') {
        if (url.includes('/set-default')) {
          console.log('⭐ Mock handling SET DEFAULT emailconfig');
          return this.handleSetDefaultEmailConfig(req);
        } else if (url.includes('/toggle-status')) {
          console.log('🔄 Mock handling TOGGLE STATUS emailconfig');
          return this.handleToggleEmailConfigStatus(req);
        } else {
          console.log('✏️ Mock handling UPDATE emailconfig');
          return this.handleUpdateEmailConfig(req);
        }
      } else if (method === 'DELETE') {
        console.log('🗑️ Mock handling DELETE emailconfig');
        return this.handleDeleteEmailConfig(req);
      }
    }

    // Roles endpoints
    if (url.includes('/roles')) {
      if (method === 'GET') {
        console.log('👥 Mock handling GET roles');
        return this.handleGetRoles(req);
      } else if (method === 'POST') {
        console.log('➕ Mock handling CREATE role');
        return this.handleCreateRole(req);
      } else if (method === 'PUT') {
        console.log('✏️ Mock handling UPDATE role');
        return this.handleUpdateRole(req);
      } else if (method === 'DELETE') {
        console.log('🗑️ Mock handling DELETE role');
        return this.handleDeleteRole(req);
      }
    }

    // Users roles assignment endpoint
    if (url.match(/\/users\/\d+\/roles$/)) {
      if (method === 'POST') {
        console.log('👥 Mock handling ASSIGN ROLES to user');
        return this.handleAssignRolesToUser(req);
      }
    }

    // Users endpoints
    if (url.includes('/users') && !url.includes('/roles')) {
      if (method === 'GET') {
        console.log('👥 Mock handling GET users');
        return this.handleGetUsers(req);
      } else if (method === 'POST') {
        console.log('➕ Mock handling CREATE user');
        return this.handleCreateUser(req);
      } else if (method === 'PUT') {
        console.log('✏️ Mock handling UPDATE user');
        return this.handleUpdateUser(req);
      } else if (method === 'DELETE') {
        console.log('🗑️ Mock handling DELETE user');
        return this.handleDeleteUser(req);
      }
    }

    // Cotizaciones endpoints
    if (url.includes('/cotizaciones')) {
      if (method === 'GET') {
        console.log('📊 Mock handling GET cotizaciones');
        return this.handleGetCotizaciones(req);
      } else if (method === 'POST') {
        console.log('➕ Mock handling CREATE cotizacion');
        return this.handleCreateCotizacion(req);
      } else if (method === 'PUT') {
        console.log('✏️ Mock handling UPDATE cotizacion');
        return this.handleUpdateCotizacion(req);
      } else if (method === 'DELETE') {
        console.log('🗑️ Mock handling DELETE cotizacion');
        return this.handleDeleteCotizacion(req);
      }
    }

    console.log('❓ Unknown endpoint, passing to backend:', url);
    // Si no es un endpoint conocido, pasar al siguiente handler
    return next.handle(req);
  }

  private handleLogin(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const { email, password } = req.body;
    
    const user = this.users.find(u => 
      u.email.toLowerCase() === email.toLowerCase() && 
      u.password === password
    );

    if (user) {
      const token = this.generateToken(user);
      const response = new HttpResponse({
        status: 200,
        body: {
          token: `Bearer ${token}`,
          refreshToken: `refresh_${token}`,
          user: {
            id: user.id,
            email: user.email,
            firstName: user.firstName,
            lastName: user.lastName,
            roles: user.roles
          }
        }
      });
      
      console.log('✅ Mock Login successful for:', email);
      console.log('✅ User roles assigned:', user.roles);
      return of(response).pipe(delay(1000));
    } else {
      const errorResponse = new HttpResponse({
        status: 401,
        statusText: 'Unauthorized',
        body: { error: 'Credenciales inválidas' }
      });
      
      console.log('❌ Mock Login failed for:', email);
      return of(errorResponse).pipe(delay(1000));
    }
  }

  private handlePolizas(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const polizas = [
      {
        id: 1,
        numeroPoliza: "POL-001-2024",
        modalidad: "ANUAL",
        nombreAsegurado: "Juan Carlos Pérez García",
        numeroCedula: "1-1234-5678",
        prima: 1500.00,
        moneda: "CRC",
        fechaVigencia: "2025-01-15T00:00:00Z",
        frecuencia: "MENSUAL",
        aseguradora: "Instituto Nacional de Seguros (INS)",
        placa: "ABC123",
        marca: "Toyota",
        modelo: "Corolla",
        año: "2020",
        correo: "juan.perez@email.com",
        numeroTelefono: "+506 8888-1234",
        perfilId: 1,
        esActivo: true,
        fechaCreacion: "2024-01-15T00:00:00Z",
        usuarioCreacion: "Admin"
      },
      {
        id: 2,
        numeroPoliza: "POL-002-2024",
        modalidad: "ANUAL",
        nombreAsegurado: "María Elena García López",
        numeroCedula: "2-2345-6789",
        prima: 2500.00,
        moneda: "CRC",
        fechaVigencia: "2025-02-01T00:00:00Z",
        frecuencia: "TRIMESTRAL",
        aseguradora: "ASSA Compañía de Seguros S.A.",
        placa: "DEF456",
        marca: "Honda",
        modelo: "Civic",
        año: "2019",
        correo: "maria.garcia@email.com",
        numeroTelefono: "+506 7777-5678",
        perfilId: 1,
        esActivo: true,
        fechaCreacion: "2024-02-01T00:00:00Z",
        usuarioCreacion: "Admin"
      },
      {
        id: 3,
        numeroPoliza: "POL-003-2024",
        modalidad: "SEMESTRAL",
        nombreAsegurado: "Carlos Alberto González Mora",
        numeroCedula: "3-3456-7890",
        prima: 800.00,
        moneda: "USD",
        fechaVigencia: "2025-03-10T00:00:00Z",
        frecuencia: "MENSUAL",
        aseguradora: "Davivienda Seguros (Costa Rica)",
        placa: "GHI789",
        marca: "Nissan",
        modelo: "Sentra",
        año: "2021",
        correo: "carlos.gonzalez@email.com",
        numeroTelefono: "+506 6666-9012",
        perfilId: 1,
        esActivo: true,
        fechaCreacion: "2024-03-10T00:00:00Z",
        usuarioCreacion: "DataLoader"
      }
    ];

    const response = new HttpResponse({
      status: 200,
      body: polizas
    });

    console.log('✅ Mock Polizas retrieved');
    return of(response).pipe(delay(500));
  }

  private generateToken(user: any): string {
    // Crear un JWT mock válido con header, payload y signature
    const header = {
      alg: "HS256",
      typ: "JWT"
    };
    
    const payload = {
      userId: user.id,
      email: user.email,
      role: user.roles[0]?.name || 'user',
      exp: Math.floor((Date.now() + (24 * 60 * 60 * 1000)) / 1000), // JWT usa segundos, no millisegundos
      iat: Math.floor(Date.now() / 1000)
    };
    
    const signature = "mock-signature";
    
    // Crear token JWT válido: header.payload.signature
    const headerBase64 = btoa(JSON.stringify(header));
    const payloadBase64 = btoa(JSON.stringify(payload));
    const signatureBase64 = btoa(signature);
    
    return `${headerBase64}.${payloadBase64}.${signatureBase64}`;
  }

  private handleCreatePoliza(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const newPoliza = req.body;
    
    // Simular creación de póliza con ID generado
    const createdPoliza = {
      id: Date.now(), // ID simulado
      numeroPoliza: `POL-${Date.now()}`,
      ...newPoliza,
      fechaCreacion: new Date().toISOString(),
      fechaActualizacion: new Date().toISOString()
    };

    const response = new HttpResponse({
      status: 201,
      body: createdPoliza
    });

    console.log('✅ Mock Poliza created:', createdPoliza.numeroPoliza);
    return of(response).pipe(delay(800));
  }

  private handleUpdatePoliza(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const polizaId = parseInt(urlParts[urlParts.length - 1]);
    const updatedPoliza = req.body;

    // Simular actualización de póliza
    const polizaActualizada = {
      id: polizaId,
      ...updatedPoliza,
      fechaActualizacion: new Date().toISOString()
    };

    const response = new HttpResponse({
      status: 200,
      body: polizaActualizada
    });

    console.log('✅ Mock Poliza updated:', polizaId);
    return of(response).pipe(delay(800));
  }

  private handleDeletePoliza(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const polizaId = parseInt(urlParts[urlParts.length - 1]);

    // Simular eliminación exitosa
    const response = new HttpResponse({
      status: 204, // No Content para DELETE exitoso
      body: null
    });

    console.log('✅ Mock Poliza deleted:', polizaId);
    return of(response).pipe(delay(600));
  }

  private handleUploadExcel(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('📤 Mock Excel Upload iniciado');
    
    // Simular procesamiento del archivo Excel
    const mockResult = {
      success: true,
      message: '¡Archivo procesado exitosamente!',
      totalRecords: 25,
      processedRecords: 23,
      errorRecords: 2,
      errors: [
        'Fila 15: Fecha de vigencia inválida',
        'Fila 22: Prima no puede estar vacía'
      ],
      failedRecords: [
        {
          rowNumber: 15,
          error: 'Fecha de vigencia inválida',
          originalData: { 
            'POLIZA': 'POL-001',
            'NOMBRE': 'Juan Pérez',
            'NUMEROCEDULA': '118880001',
            'PRIMA': '125000',
            'MONEDA': 'CRC',
            'FECHA': '2024/13/01', // Fecha inválida
            'FRECUENCIA': 'Mensual',
            'ASEGURADORA': 'MNK Seguros Compañía Aseguradora',
            'PLACA': 'BCD123',
            'MARCA': 'Toyota',
            'MODELO': 'Corolla',
            'AÑO': '2020',
            'CORREO': 'juan@email.com',
            'NUMEROTELEFONO': '88881234'
          }
        },
        {
          rowNumber: 22,
          error: 'Prima no puede estar vacía',
          originalData: { 
            'POLIZA': 'POL-002',
            'NOMBRE': 'María González',
            'NUMEROCEDULA': '207770002',
            'PRIMA': '', // Prima vacía
            'MONEDA': 'CRC',
            'FECHA': '2024/01/15',
            'FRECUENCIA': 'Anual',
            'ASEGURADORA': 'Aseguradora del Istmo (ADISA)',
            'PLACA': 'DEF456',
            'MARCA': 'Honda',
            'MODELO': 'Civic',
            'AÑO': '2019',
            'CORREO': 'maria@email.com',
            'NUMEROTELEFONO': '88885678'
          }
        }
      ],
      status: 'Completed with errors'
    };

    const response = new HttpResponse({
      status: 200,
      body: mockResult
    });

    console.log('✅ Mock Excel Upload completado:', mockResult);
    return of(response).pipe(delay(2000)); // Simular tiempo de procesamiento
  }

  // ============== COBROS HANDLERS ==============

  private handleGetCobros(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('💰 Returning mock cobros data');
    
    const response = new HttpResponse({
      status: 200,
      body: this.cobros
    });

    return of(response).pipe(delay(800));
  }

  private handleGetCobroById(req: HttpRequest<any>, id: number): Observable<HttpEvent<any>> {
    console.log('🔍 Searching for cobro with ID:', id);
    
    const cobro = this.cobros.find(c => c.id === id);
    
    if (!cobro) {
      console.log('❌ Cobro not found with ID:', id);
      const response = new HttpResponse({
        status: 404,
        statusText: 'Not Found',
        body: { error: 'Cobro no encontrado' }
      });
      return of(response).pipe(delay(500));
    }
    
    console.log('✅ Found cobro:', cobro);
    const response = new HttpResponse({
      status: 200,
      body: cobro
    });

    return of(response).pipe(delay(800));
  }

  private handleCobroStats(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('📊 Calculating mock cobros stats');
    
    const pendientes = this.cobros.filter(c => c.estado === EstadoCobro.Pendiente);
    const cobrados = this.cobros.filter(c => c.estado === EstadoCobro.Cobrado);
    const vencidos = this.cobros.filter(c => c.estado === EstadoCobro.Vencido);
    
    const stats = {
      totalPendientes: pendientes.length,
      totalCobrados: cobrados.length,
      totalVencidos: vencidos.length,
      montoTotalPendiente: pendientes.reduce((sum, c) => sum + c.montoTotal, 0),
      montoTotalCobrado: cobrados.reduce((sum, c) => sum + (c.montoCobrado || 0), 0),
      montoPorVencer: vencidos.reduce((sum, c) => sum + c.montoTotal, 0)
    };
    
    const response = new HttpResponse({
      status: 200,
      body: stats
    });

    console.log('✅ Mock cobros stats:', stats);
    return of(response).pipe(delay(600));
  }

  private handleCreateCobro(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const newCobro = req.body;
    const nextId = Math.max(...this.cobros.map(c => c.id)) + 1;
    
    const createdCobro = {
      id: nextId,
      numeroRecibo: `REC-2024-${String(nextId).padStart(3, '0')}`,
      ...newCobro,
      estado: EstadoCobro.Pendiente,
      fechaCreacion: new Date(),
    };
    
    this.cobros.push(createdCobro);
    
    const response = new HttpResponse({
      status: 201,
      body: createdCobro
    });

    console.log('✅ Mock cobro created:', createdCobro.numeroRecibo);
    return of(response).pipe(delay(800));
  }

  private handleRegistrarCobro(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const cobroId = parseInt(urlParts[urlParts.indexOf('cobros') + 1]);
    const registroData = req.body;
    
    const cobro = this.cobros.find(c => c.id === cobroId);
    if (!cobro) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Cobro no encontrado' }
      }));
    }
    
    // Actualizar cobro con datos de pago
    cobro.fechaCobro = new Date(registroData.fechaCobro);
    cobro.montoCobrado = registroData.montoCobrado;
    cobro.metodoPago = registroData.metodoPago;
    cobro.estado = EstadoCobro.Cobrado;
    cobro.observaciones = registroData.observaciones || cobro.observaciones;
    cobro.fechaActualizacion = new Date();
    
    const response = new HttpResponse({
      status: 200,
      body: cobro
    });

    console.log('✅ Mock cobro registrado:', cobro.numeroRecibo);
    return of(response).pipe(delay(800));
  }

  private handleCancelarCobro(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const cobroId = parseInt(urlParts[urlParts.indexOf('cobros') + 1]);
    const { motivo } = req.body;
    
    const cobro = this.cobros.find(c => c.id === cobroId);
    if (!cobro) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Cobro no encontrado' }
      }));
    }
    
    // Cancelar cobro
    cobro.estado = EstadoCobro.Cancelado;
    cobro.observaciones = motivo || 'Cobro cancelado';
    cobro.fechaActualizacion = new Date();
    
    const response = new HttpResponse({
      status: 200,
      body: cobro
    });

    console.log('✅ Mock cobro cancelado:', cobro.numeroRecibo);
    return of(response).pipe(delay(600));
  }

  // ============== RECLAMOS HANDLERS ==============

  private handleGetReclamos(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('📋 Returning mock reclamos data');
    
    const response = new HttpResponse({
      status: 200,
      body: this.reclamos
    });

    return of(response).pipe(delay(800));
  }

  private handleGetReclamoById(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('🔍 Getting reclamo by ID from URL:', req.url);
    
    // Extraer ID de la URL
    const urlParts = req.url.split('/');
    const id = parseInt(urlParts[urlParts.length - 1]);
    
    console.log('🔍 Looking for reclamo with ID:', id);
    
    const reclamo = this.reclamos.find(r => r.id === id);
    
    if (!reclamo) {
      console.log('❌ Reclamo not found:', id);
      const response = new HttpResponse({
        status: 404,
        body: { message: `Reclamo con ID ${id} no encontrado` }
      });
      return of(response).pipe(delay(300));
    }
    
    console.log('✅ Found reclamo:', reclamo.numeroReclamo);
    const response = new HttpResponse({
      status: 200,
      body: reclamo
    });

    return of(response).pipe(delay(500));
  }

  private handleReclamosStats(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('📊 Calculating mock reclamos stats');
    
    const abiertos = this.reclamos.filter(r => r.estado === EstadoReclamo.Abierto);
    const enProceso = this.reclamos.filter(r => r.estado === EstadoReclamo.EnProceso);
    const resueltos = this.reclamos.filter(r => r.estado === EstadoReclamo.Resuelto);
    const cerrados = this.reclamos.filter(r => r.estado === EstadoReclamo.Cerrado);
    const rechazados = this.reclamos.filter(r => r.estado === EstadoReclamo.Rechazado);
    const prioridadAlta = this.reclamos.filter(r => r.prioridad === PrioridadReclamo.Alta);
    const prioridadCritica = this.reclamos.filter(r => r.prioridad === PrioridadReclamo.Critica);
    
    const stats = {
      totalReclamos: this.reclamos.length,
      reclamosAbiertos: abiertos.length,
      reclamosEnProceso: enProceso.length,
      reclamosResueltos: resueltos.length,
      reclamosCerrados: cerrados.length,
      reclamosRechazados: rechazados.length,
      totalMontoReclamado: this.reclamos.reduce((sum, r) => sum + (r.montoReclamado || 0), 0),
      totalMontoAprobado: this.reclamos.reduce((sum, r) => sum + (r.montoAprobado || 0), 0),
      monedaPrincipal: 'CRC',
      reclamosPrioridadAlta: prioridadAlta.length,
      reclamosPrioridadCritica: prioridadCritica.length,
      reclamosVencidos: this.reclamos.filter(r => 
        r.fechaLimiteRespuesta && new Date(r.fechaLimiteRespuesta) < new Date() && 
        r.estado !== EstadoReclamo.Resuelto && r.estado !== EstadoReclamo.Cerrado
      ).length
    };
    
    const response = new HttpResponse({
      status: 200,
      body: stats
    });

    console.log('✅ Mock reclamos stats:', stats);
    return of(response).pipe(delay(600));
  }

  private handleCreateReclamo(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const newReclamo = req.body;
    const nextId = Math.max(...this.reclamos.map(r => r.id)) + 1;
    
    const createdReclamo = {
      id: nextId,
      numeroReclamo: `REC-2024-${String(nextId).padStart(3, '0')}`,
      ...newReclamo,
      estado: EstadoReclamo.Abierto,
      fechaReclamo: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      createdBy: 'Sistema',
      isDeleted: false,
      fechaLimiteRespuesta: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString() // 30 días
    };
    
    this.reclamos.push(createdReclamo);
    
    const response = new HttpResponse({
      status: 201,
      body: createdReclamo
    });

    console.log('✅ Mock reclamo created:', createdReclamo.numeroReclamo);
    return of(response).pipe(delay(800));
  }

  private handleUpdateReclamo(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const reclamoId = parseInt(urlParts[urlParts.length - 1]);
    const updateData = req.body;
    
    const reclamo = this.reclamos.find(r => r.id === reclamoId);
    if (!reclamo) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Reclamo no encontrado' }
      }));
    }
    
    // Actualizar reclamo
    Object.assign(reclamo, updateData);
    reclamo.updatedAt = new Date().toISOString();
    reclamo.updatedBy = 'Admin';
    
    const response = new HttpResponse({
      status: 200,
      body: reclamo
    });

    console.log('✅ Mock reclamo updated:', reclamo.numeroReclamo);
    return of(response).pipe(delay(800));
  }

  private handleDeleteReclamo(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const reclamoId = parseInt(urlParts[urlParts.length - 1]);
    
    const reclamo = this.reclamos.find(r => r.id === reclamoId);
    if (!reclamo) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Reclamo no encontrado' }
      }));
    }
    
    // Soft delete
    reclamo.isDeleted = true;
    reclamo.updatedAt = new Date().toISOString();
    reclamo.updatedBy = 'Admin';
    
    const response = new HttpResponse({
      status: 204,
      body: null
    });

    console.log('✅ Mock reclamo deleted:', reclamo.numeroReclamo);
    return of(response).pipe(delay(600));
  }

  private handleChangeEstadoReclamo(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const reclamoId = parseInt(urlParts[urlParts.length - 2]); // ID está antes de 'estado'
    const { estado, observaciones } = req.body;
    
    const reclamoIndex = this.reclamos.findIndex(r => r.id === reclamoId);
    if (reclamoIndex === -1) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Reclamo no encontrado' }
      }));
    }
    
    // Actualizar estado del reclamo
    this.reclamos[reclamoIndex].estado = estado;
    this.reclamos[reclamoIndex].updatedAt = new Date().toISOString();
    this.reclamos[reclamoIndex].updatedBy = 'Admin';
    
    // Actualizar observaciones como string si se proporcionan
    if (observaciones) {
      this.reclamos[reclamoIndex].observaciones = observaciones;
    }
    
    const response = new HttpResponse({
      status: 200,
      body: {
        success: true,
        message: `Estado del reclamo cambiado a ${this.getEstadoNombre(estado)} exitosamente`,
        data: this.reclamos[reclamoIndex]
      }
    });

    console.log('🔄 Mock reclamo estado changed:', this.reclamos[reclamoIndex].numeroReclamo, 'new estado:', estado);
    return of(response).pipe(delay(600));
  }

  private handleAsignarReclamo(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const reclamoId = parseInt(urlParts[urlParts.length - 2]);
    const { usuarioId } = req.body;
    
    const reclamoIndex = this.reclamos.findIndex(r => r.id === reclamoId);
    if (reclamoIndex === -1) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Reclamo no encontrado' }
      }));
    }
    
    // Usar Object.assign para asignar propiedades dinámicamente
    Object.assign(this.reclamos[reclamoIndex], {
      asignadoA: usuarioId,
      updatedAt: new Date().toISOString(),
      updatedBy: 'Admin'
    });
    
    const response = new HttpResponse({
      status: 200,
      body: {
        success: true,
        message: 'Reclamo asignado exitosamente',
        data: this.reclamos[reclamoIndex]
      }
    });

    console.log('👤 Mock reclamo assigned:', this.reclamos[reclamoIndex].numeroReclamo);
    return of(response).pipe(delay(600));
  }

  private handleResolverReclamo(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const reclamoId = parseInt(urlParts[urlParts.length - 2]);
    const { montoAprobado, observaciones } = req.body;
    
    const reclamoIndex = this.reclamos.findIndex(r => r.id === reclamoId);
    if (reclamoIndex === -1) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Reclamo no encontrado' }
      }));
    }
    
    this.reclamos[reclamoIndex].estado = EstadoReclamo.Resuelto;
    this.reclamos[reclamoIndex].montoAprobado = montoAprobado;
    this.reclamos[reclamoIndex].fechaResolucion = new Date().toISOString();
    this.reclamos[reclamoIndex].updatedAt = new Date().toISOString();
    this.reclamos[reclamoIndex].updatedBy = 'Admin';
    
    const response = new HttpResponse({
      status: 200,
      body: {
        success: true,
        message: 'Reclamo resuelto exitosamente',
        data: this.reclamos[reclamoIndex]
      }
    });

    console.log('✅ Mock reclamo resolved:', this.reclamos[reclamoIndex].numeroReclamo);
    return of(response).pipe(delay(600));
  }

  private handleRechazarReclamo(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const reclamoId = parseInt(urlParts[urlParts.length - 2]);
    const { observaciones } = req.body;
    
    const reclamoIndex = this.reclamos.findIndex(r => r.id === reclamoId);
    if (reclamoIndex === -1) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Reclamo no encontrado' }
      }));
    }
    
    // Usar Object.assign para propiedades dinámicas
    Object.assign(this.reclamos[reclamoIndex], {
      estado: EstadoReclamo.Rechazado,
      fechaRechazo: new Date().toISOString(),
      motivoRechazo: observaciones,
      updatedAt: new Date().toISOString(),
      updatedBy: 'Admin'
    });
    
    const response = new HttpResponse({
      status: 200,
      body: {
        success: true,
        message: 'Reclamo rechazado exitosamente',
        data: this.reclamos[reclamoIndex]
      }
    });

    console.log('❌ Mock reclamo rejected:', this.reclamos[reclamoIndex].numeroReclamo);
    return of(response).pipe(delay(600));
  }

  private getEstadoNombre(estado: number): string {
    switch (estado) {
      case EstadoReclamo.Abierto: return 'Abierto';
      case EstadoReclamo.EnProceso: return 'En Proceso';
      case EstadoReclamo.Resuelto: return 'Resuelto';
      case EstadoReclamo.Cerrado: return 'Cerrado';
      case EstadoReclamo.Rechazado: return 'Rechazado';
      case EstadoReclamo.Escalado: return 'Escalado';
      default: return 'Desconocido';
    }
  }

  // Email Config handlers
  private handleGetEmailConfig(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const emailConfigId = urlParts[urlParts.length - 1];
    
    if (emailConfigId && emailConfigId !== 'emailconfig') {
      // Get specific email config by ID
      const configId = parseInt(emailConfigId);
      const emailConfig = this.emailConfigs.find(ec => ec.id === configId);
      
      if (!emailConfig) {
        return of(new HttpResponse({
          status: 404,
          body: { message: 'Configuración de email no encontrada' }
        }));
      }

      const response = new HttpResponse({
        status: 200,
        body: {
          data: emailConfig,
          message: 'Configuración obtenida exitosamente'
        }
      });

      console.log('✅ Mock email config found:', emailConfig.configName);
      return of(response).pipe(delay(500));
    } else {
      // Get all email configs
      const response = new HttpResponse({
        status: 200,
        body: {
          data: this.emailConfigs,
          total: this.emailConfigs.length,
          message: 'Configuraciones obtenidas exitosamente'
        }
      });

      console.log('✅ Mock returning', this.emailConfigs.length, 'email configs');
      return of(response).pipe(delay(500));
    }
  }

  private handleCreateEmailConfig(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const createData = req.body;
    
    const newEmailConfig = {
      id: Math.max(...this.emailConfigs.map(ec => ec.id)) + 1,
      ...createData,
      isActive: true,
      isDefault: this.emailConfigs.length === 0, // First config is default
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      createdBy: 'Admin',
      updatedBy: 'Admin'
    };

    this.emailConfigs.push(newEmailConfig);

    const response = new HttpResponse({
      status: 201,
      body: {
        data: newEmailConfig,
        message: 'Configuración de email creada exitosamente'
      }
    });

    console.log('✅ Mock email config created:', newEmailConfig.configName);
    return of(response).pipe(delay(800));
  }

  private handleUpdateEmailConfig(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const configId = parseInt(urlParts[urlParts.length - 1]);
    const updateData = req.body;
    
    const emailConfig = this.emailConfigs.find(ec => ec.id === configId);
    if (!emailConfig) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Configuración de email no encontrada' }
      }));
    }
    
    // Update email config
    Object.assign(emailConfig, updateData);
    emailConfig.updatedAt = new Date().toISOString();
    emailConfig.updatedBy = 'Admin';
    
    const response = new HttpResponse({
      status: 200,
      body: {
        data: emailConfig,
        message: 'Configuración actualizada exitosamente'
      }
    });

    console.log('✅ Mock email config updated:', emailConfig.configName);
    return of(response).pipe(delay(800));
  }

  private handleDeleteEmailConfig(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const configId = parseInt(urlParts[urlParts.length - 1]);
    
    const configIndex = this.emailConfigs.findIndex(ec => ec.id === configId);
    if (configIndex === -1) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Configuración de email no encontrada' }
      }));
    }
    
    const deletedConfig = this.emailConfigs[configIndex];
    this.emailConfigs.splice(configIndex, 1);
    
    const response = new HttpResponse({
      status: 204,
      body: null
    });

    console.log('✅ Mock email config deleted:', deletedConfig.configName);
    return of(response).pipe(delay(600));
  }

  private handleSetDefaultEmailConfig(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const configId = parseInt(urlParts[urlParts.length - 2]); // ID está antes de 'set-default'
    
    const configIndex = this.emailConfigs.findIndex(ec => ec.id === configId);
    if (configIndex === -1) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Configuración de email no encontrada' }
      }));
    }
    
    // Desactivar isDefault en todas las configuraciones
    this.emailConfigs.forEach(config => config.isDefault = false);
    
    // Activar isDefault en la configuración seleccionada
    this.emailConfigs[configIndex].isDefault = true;
    
    const response = new HttpResponse({
      status: 200,
      body: {
        success: true,
        message: 'Configuración establecida como predeterminada exitosamente',
        data: true
      }
    });

    console.log('⭐ Mock email config set as default:', this.emailConfigs[configIndex].configName);
    return of(response).pipe(delay(600));
  }

  private handleToggleEmailConfigStatus(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const configId = parseInt(urlParts[urlParts.length - 2]); // ID está antes de 'toggle-status'
    
    const configIndex = this.emailConfigs.findIndex(ec => ec.id === configId);
    if (configIndex === -1) {
      return of(new HttpResponse({
        status: 404,
        body: { message: 'Configuración de email no encontrada' }
      }));
    }
    
    // Cambiar el estado isActive
    this.emailConfigs[configIndex].isActive = !this.emailConfigs[configIndex].isActive;
    
    const response = new HttpResponse({
      status: 200,
      body: {
        success: true,
        message: `Configuración ${this.emailConfigs[configIndex].isActive ? 'activada' : 'desactivada'} exitosamente`,
        data: true
      }
    });

    console.log('🔄 Mock email config status toggled:', this.emailConfigs[configIndex].configName, 'isActive:', this.emailConfigs[configIndex].isActive);
    return of(response).pipe(delay(600));
  }

  private handleTestEmailConfig(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const testData = req.body;
    
    // Simulate email test
    const success = Math.random() > 0.2; // 80% success rate
    
    const response = new HttpResponse({
      status: 200,
      body: {
        data: {
          success: success,
          testEmail: testData.testEmail || testData.fromEmail,
          timestamp: new Date().toISOString(),
          errorMessage: success ? null : 'Error de conexión SMTP: Credenciales inválidas'
        },
        message: success ? 'Test de email exitoso' : 'Test de email falló'
      }
    });

    console.log('✅ Mock email test result:', success ? 'SUCCESS' : 'FAILED');
    return of(response).pipe(delay(2000)); // Simulate longer test time
  }

  private handleTestEmailConfigDirect(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const testData = req.body;
    
    // Simulate direct email test
    const success = Math.random() > 0.2; // 80% success rate
    
    const response = new HttpResponse({
      status: 200,
      body: {
        data: {
          success: success,
          testEmail: testData.testEmail || testData.fromEmail,
          timestamp: new Date().toISOString(),
          errorMessage: success ? null : 'Error de conexión SMTP: Verificar servidor y credenciales'
        },
        message: success ? 'Test directo de configuración exitoso' : 'Test directo de configuración falló'
      }
    });

    console.log('✅ Mock email direct test result:', success ? 'SUCCESS' : 'FAILED');
    return of(response).pipe(delay(2000)); // Simulate longer test time
  }

  // ============== ROLES HANDLERS ==============

  private handleGetRoles(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('👥 Returning mock roles data');
    
    const response = new HttpResponse({
      status: 200,
      body: this.roles
    });

    return of(response).pipe(delay(500));
  }

  private handleCreateRole(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('➕ Creating new role:', req.body);
    
    const newRole = {
      id: Math.max(...this.roles.map(r => r.id)) + 1,
      ...req.body,
      isActive: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };
    
    this.roles.push(newRole);
    
    const response = new HttpResponse({
      status: 201,
      body: newRole
    });

    console.log('✅ Mock role created:', newRole.id);
    return of(response).pipe(delay(600));
  }

  private handleUpdateRole(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const id = parseInt(urlParts[urlParts.length - 1]);
    
    console.log('✏️ Updating role:', id, req.body);
    
    const index = this.roles.findIndex(r => r.id === id);
    if (index === -1) {
      const response = new HttpResponse({
        status: 404,
        body: { message: `Rol con ID ${id} no encontrado` }
      });
      return of(response).pipe(delay(300));
    }
    
    const updatedRole = {
      ...this.roles[index],
      ...req.body,
      updatedAt: new Date().toISOString()
    };
    
    this.roles[index] = updatedRole;
    
    const response = new HttpResponse({
      status: 200,
      body: updatedRole
    });

    console.log('✅ Mock role updated:', id);
    return of(response).pipe(delay(600));
  }

  private handleDeleteRole(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const id = parseInt(urlParts[urlParts.length - 1]);
    
    console.log('🗑️ Deleting role:', id);
    
    const index = this.roles.findIndex(r => r.id === id);
    if (index === -1) {
      const response = new HttpResponse({
        status: 404,
        body: { message: `Rol con ID ${id} no encontrado` }
      });
      return of(response).pipe(delay(300));
    }
    
    this.roles.splice(index, 1);
    
    const response = new HttpResponse({
      status: 200,
      body: { message: 'Rol eliminado exitosamente' }
    });

    console.log('✅ Mock role deleted:', id);
    return of(response).pipe(delay(400));
  }

  private handleAssignRolesToUser(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const userId = parseInt(urlParts[urlParts.length - 2]); // users/{userId}/roles
    const { roleIds } = req.body;
    
    console.log('👥 Assigning roles to user:', userId, 'roleIds:', roleIds);
    
    const userIndex = this.users.findIndex(u => u.id === userId);
    if (userIndex === -1) {
      const response = new HttpResponse({
        status: 404,
        body: { message: `Usuario con ID ${userId} no encontrado` }
      });
      return of(response).pipe(delay(300));
    }
    
    // Find roles by IDs
    const assignedRoles = this.roles.filter(role => roleIds.includes(role.id));
    
    // Update user roles
    this.users[userIndex].roles = assignedRoles;
    
    const response = new HttpResponse({
      status: 200,
      body: { 
        message: 'Roles asignados exitosamente',
        user: this.users[userIndex],
        assignedRoles 
      }
    });

    console.log('✅ Mock roles assigned to user:', userId, assignedRoles.map(r => r.name));
    return of(response).pipe(delay(600));
  }

  // ============== USERS HANDLERS ==============

  private handleGetUsers(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('👥 Returning mock users data');
    
    // Return users without passwords for security
    const safeUsers = this.users.map(user => ({
      ...user,
      password: undefined
    }));
    
    const response = new HttpResponse({
      status: 200,
      body: safeUsers
    });

    return of(response).pipe(delay(500));
  }

  private handleCreateUser(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('➕ Creating new user:', req.body);
    
    const newUser = {
      id: Math.max(...this.users.map(u => u.id)) + 1,
      ...req.body,
      roles: [], // Start with no roles
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      isActive: true,
      lastLoginAt: null
    };
    
    this.users.push(newUser);
    
    const response = new HttpResponse({
      status: 201,
      body: {
        ...newUser,
        password: undefined // Don't return password
      }
    });

    console.log('✅ Mock user created:', newUser.id);
    return of(response).pipe(delay(600));
  }

  private handleUpdateUser(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const id = parseInt(urlParts[urlParts.length - 1]);
    
    console.log('✏️ Updating user:', id, req.body);
    
    const index = this.users.findIndex(u => u.id === id);
    if (index === -1) {
      const response = new HttpResponse({
        status: 404,
        body: { message: `Usuario con ID ${id} no encontrado` }
      });
      return of(response).pipe(delay(300));
    }
    
    const updatedUser = {
      ...this.users[index],
      ...req.body,
      updatedAt: new Date().toISOString()
    };
    
    this.users[index] = updatedUser;
    
    const response = new HttpResponse({
      status: 200,
      body: {
        ...updatedUser,
        password: undefined // Don't return password
      }
    });

    console.log('✅ Mock user updated:', id);
    return of(response).pipe(delay(600));
  }

  private handleDeleteUser(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const id = parseInt(urlParts[urlParts.length - 1]);
    
    console.log('🗑️ Deleting user:', id);
    
    const index = this.users.findIndex(u => u.id === id);
    if (index === -1) {
      const response = new HttpResponse({
        status: 404,
        body: { message: `Usuario con ID ${id} no encontrado` }
      });
      return of(response).pipe(delay(300));
    }
    
    this.users.splice(index, 1);
    
    const response = new HttpResponse({
      status: 200,
      body: { message: 'Usuario eliminado exitosamente' }
    });

    console.log('✅ Mock user deleted:', id);
    return of(response).pipe(delay(400));
  }

  // ============== COTIZACIONES HANDLERS ==============

  private handleGetCotizaciones(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('📊 Returning mock cotizaciones data');
    
    const response = new HttpResponse({
      status: 200,
      body: this.cotizaciones
    });

    return of(response).pipe(delay(800));
  }

  private handleCreateCotizacion(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    console.log('➕ Creating new cotizacion:', req.body);
    
    // Verificar si ya existe una cotización muy similar creada recientemente (últimos 2 segundos)
    const now = Date.now();
    const recentCotizacion = this.cotizaciones.find(c => {
      const createdAt = new Date(c.createdAt).getTime();
      const timeDiff = now - createdAt;
      return timeDiff < 2000 && // Últimos 2 segundos
             c.nombreSolicitante === req.body.nombreSolicitante &&
             c.email === req.body.email &&
             c.tipoSeguro === req.body.tipoSeguro;
    });

    if (recentCotizacion) {
      console.log('⚠️ Duplicate cotizacion detected, returning existing one:', recentCotizacion.id);
      const response = new HttpResponse({
        status: 200,
        body: recentCotizacion
      });
      return of(response).pipe(delay(100));
    }
    
    const newCotizacion = {
      id: Math.max(...this.cotizaciones.map(c => c.id)) + 1,
      numeroCotizacion: `COT-${Date.now().toString().slice(-6)}`,
      ...req.body,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      createdBy: 'Sistema',
      isDeleted: false
    };
    
    this.cotizaciones.unshift(newCotizacion);
    
    const response = new HttpResponse({
      status: 201,
      body: newCotizacion
    });

    console.log('✅ Mock cotizacion created:', newCotizacion.id);
    return of(response).pipe(delay(600));
  }

  private handleUpdateCotizacion(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const id = parseInt(urlParts[urlParts.length - 1]);
    
    console.log('✏️ Updating cotizacion:', id, req.body);
    
    const index = this.cotizaciones.findIndex(c => c.id === id);
    if (index === -1) {
      const response = new HttpResponse({
        status: 404,
        body: { message: `Cotización con ID ${id} no encontrada` }
      });
      return of(response).pipe(delay(300));
    }
    
    const updatedCotizacion = {
      ...this.cotizaciones[index],
      ...req.body,
      updatedAt: new Date().toISOString()
    };
    
    this.cotizaciones[index] = updatedCotizacion;
    
    const response = new HttpResponse({
      status: 200,
      body: updatedCotizacion
    });

    console.log('✅ Mock cotizacion updated:', id);
    return of(response).pipe(delay(600));
  }

  private handleDeleteCotizacion(req: HttpRequest<any>): Observable<HttpEvent<any>> {
    const urlParts = req.url.split('/');
    const id = parseInt(urlParts[urlParts.length - 1]);
    
    console.log('🗑️ Deleting cotizacion:', id);
    
    const index = this.cotizaciones.findIndex(c => c.id === id);
    if (index === -1) {
      const response = new HttpResponse({
        status: 404,
        body: { message: `Cotización con ID ${id} no encontrada` }
      });
      return of(response).pipe(delay(300));
    }
    
    this.cotizaciones.splice(index, 1);
    
    const response = new HttpResponse({
      status: 200,
      body: { message: 'Cotización eliminada exitosamente' }
    });

    console.log('✅ Mock cotizacion deleted:', id);
    return of(response).pipe(delay(400));
  }
}