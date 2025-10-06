# 🏢 Guía de Implementación Multi-Empresa (Multi-Tenancy) - SIINADSEG

## 📋 Estrategias de Multi-Tenancy

### 🎯 **OPCIÓN 1: Database Per Tenant (RECOMENDADA para SIINADSEG)**

#### **Ventajas:**
- ✅ **Aislamiento total** de datos entre empresas
- ✅ **Seguridad máxima** - imposible acceso cruzado
- ✅ **Escalabilidad independiente** por empresa
- ✅ **Cumplimiento regulatorio** (GDPR, SOX)
- ✅ **Backups independientes** por empresa
- ✅ **Personalización** por empresa (esquemas, campos custom)

#### **Implementación:**
```csharp
// TenantContext.cs
public class TenantContext
{
    public string TenantId { get; set; }
    public string CompanyName { get; set; }
    public string DatabaseName { get; set; }
    public string ConnectionString { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ITenantService.cs
public interface ITenantService
{
    Task<TenantContext> GetTenantAsync(string tenantId);
    Task<TenantContext> GetTenantByDomainAsync(string domain);
    Task<string> GetConnectionStringAsync(string tenantId);
    Task<bool> TenantExistsAsync(string tenantId);
}
```

---

### 🏗️ **OPCIÓN 2: Schema Per Tenant**

#### **Para empresas medianas:**
```sql
-- Esquemas separados en misma BD
CREATE SCHEMA empresa_abc;
CREATE SCHEMA empresa_xyz;

-- Tablas por esquema
CREATE TABLE empresa_abc.Polizas (...);
CREATE TABLE empresa_xyz.Polizas (...);
```

---

### 📊 **OPCIÓN 3: Row Level Security (Shared Database)**

#### **Para empresas pequeñas con pocos datos:**
```sql
-- Columna TenantId en todas las tablas
ALTER TABLE Polizas ADD TenantId NVARCHAR(50) NOT NULL;
ALTER TABLE Cobros ADD TenantId NVARCHAR(50) NOT NULL;

-- Row Level Security
CREATE SECURITY POLICY TenantPolicy
ADD FILTER PREDICATE USER_NAME() = TenantId
ON dbo.Polizas;
```

---

## 🎯 **IMPLEMENTACIÓN RECOMENDADA PARA SIINADSEG**

### **Estrategia: Database Per Tenant + Master Database**

```
Master Database (SinsegMaster):
├── Tenants (empresas registradas)
├── Users (usuarios sistema)
├── Subscriptions (planes/facturación)
└── SystemConfig (configuración global)

Tenant Databases:
├── SinsegTenant_ABC (Empresa ABC)
├── SinsegTenant_XYZ (Empresa XYZ)
└── SinsegTenant_123 (Empresa 123)
```

### **1. Estructura Master Database:**

```sql
-- Tabla principal de empresas/tenants
CREATE TABLE Tenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId NVARCHAR(50) UNIQUE NOT NULL, -- slug empresa (ej: "empresa-abc")
    CompanyName NVARCHAR(200) NOT NULL,
    Domain NVARCHAR(100) NULL, -- custom domain (ej: abc.siinadseg.com)
    DatabaseName NVARCHAR(100) NOT NULL,
    ConnectionString NVARCHAR(500) NOT NULL,
    IsActive BIT DEFAULT 1,
    SubscriptionPlan NVARCHAR(50) NOT NULL, -- Basic, Professional, Enterprise
    MaxUsers INT DEFAULT 10,
    MaxPolizas INT DEFAULT 1000,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    
    -- Configuraciones por empresa
    LogoUrl NVARCHAR(300),
    PrimaryColor NVARCHAR(7), -- #hexcolor
    SecondaryColor NVARCHAR(7),
    CustomCss NVARCHAR(MAX),
    
    -- Facturación
    BillingEmail NVARCHAR(200),
    BillingAddress NVARCHAR(500),
    LastPaymentDate DATETIME2,
    NextBillingDate DATETIME2,
    
    INDEX IX_Tenants_TenantId (TenantId),
    INDEX IX_Tenants_Domain (Domain)
);

-- Usuarios del sistema (pueden tener acceso a múltiples tenants)
CREATE TABLE SystemUsers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(200) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    IsSuperAdmin BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Relación usuarios-tenants (un usuario puede estar en múltiples empresas)
CREATE TABLE UserTenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER REFERENCES SystemUsers(Id),
    TenantId NVARCHAR(50) REFERENCES Tenants(TenantId),
    Role NVARCHAR(50) NOT NULL, -- Admin, Manager, User, ReadOnly
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    UNIQUE(UserId, TenantId)
);
```

### **2. Configuración Backend Multi-Tenant:**

```csharp
// Models/MultiTenant/TenantInfo.cs
public class TenantInfo
{
    public string TenantId { get; set; }
    public string CompanyName { get; set; }
    public string DatabaseName { get; set; }
    public string ConnectionString { get; set; }
    public string LogoUrl { get; set; }
    public string PrimaryColor { get; set; }
    public string SecondaryColor { get; set; }
    public bool IsActive { get; set; }
    public string SubscriptionPlan { get; set; }
    public int MaxUsers { get; set; }
    public int MaxPolizas { get; set; }
}

// Services/MultiTenant/TenantService.cs
public class TenantService : ITenantService
{
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    
    public async Task<TenantInfo> GetTenantByDomainAsync(string domain)
    {
        // 1. Intentar desde cache
        if (_cache.TryGetValue($"tenant_{domain}", out TenantInfo cachedTenant))
            return cachedTenant;
            
        // 2. Consultar master database
        using var connection = new SqlConnection(_config.GetConnectionString("MasterDatabase"));
        
        var tenant = await connection.QueryFirstOrDefaultAsync<TenantInfo>(@"
            SELECT TenantId, CompanyName, DatabaseName, ConnectionString, 
                   LogoUrl, PrimaryColor, SecondaryColor, IsActive, 
                   SubscriptionPlan, MaxUsers, MaxPolizas
            FROM Tenants 
            WHERE Domain = @domain AND IsActive = 1", new { domain });
            
        // 3. Cache por 1 hora
        if (tenant != null)
        {
            _cache.Set($"tenant_{domain}", tenant, TimeSpan.FromHours(1));
        }
        
        return tenant;
    }
    
    public async Task<TenantInfo> GetTenantByIdAsync(string tenantId)
    {
        if (_cache.TryGetValue($"tenant_id_{tenantId}", out TenantInfo cachedTenant))
            return cachedTenant;
            
        using var connection = new SqlConnection(_config.GetConnectionString("MasterDatabase"));
        
        var tenant = await connection.QueryFirstOrDefaultAsync<TenantInfo>(@"
            SELECT * FROM Tenants WHERE TenantId = @tenantId AND IsActive = 1", 
            new { tenantId });
            
        if (tenant != null)
        {
            _cache.Set($"tenant_id_{tenantId}", tenant, TimeSpan.FromHours(1));
        }
        
        return tenant;
    }
}

// Middleware/TenantMiddleware.cs
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    
    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        string tenantId = null;
        
        // 1. Desde subdomain (empresa-abc.siinadseg.com)
        var host = context.Request.Host.Value;
        if (host.Contains(".siinadseg.com"))
        {
            tenantId = host.Split('.')[0];
        }
        
        // 2. Desde header (para APIs)
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        }
        
        // 3. Desde claim del JWT
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = context.User?.FindFirst("tenant_id")?.Value;
        }
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenant = await tenantService.GetTenantByIdAsync(tenantId);
            if (tenant != null && tenant.IsActive)
            {
                context.Items["TenantInfo"] = tenant;
                context.Items["TenantId"] = tenantId;
            }
        }
        
        await _next(context);
    }
}

// Data/TenantDbContext.cs
public class TenantDbContext : DbContext
{
    private readonly TenantInfo _tenantInfo;
    
    public TenantDbContext(DbContextOptions<TenantDbContext> options, TenantInfo tenantInfo) 
        : base(options)
    {
        _tenantInfo = tenantInfo;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_tenantInfo != null)
        {
            optionsBuilder.UseSqlServer(_tenantInfo.ConnectionString);
        }
        base.OnConfiguring(optionsBuilder);
    }
    
    // Mismo esquema que tu DbContext actual
    public DbSet<Poliza> Polizas { get; set; }
    public DbSet<Cobro> Cobros { get; set; }
    public DbSet<Reclamo> Reclamos { get; set; }
    // ... resto de entidades
}
```

### **3. Configuración Frontend Multi-Tenant:**

```typescript
// services/tenant.service.ts
@Injectable({ providedIn: 'root' })
export class TenantService {
  private tenantInfo$ = new BehaviorSubject<TenantInfo | null>(null);
  
  constructor(private http: HttpClient) {
    this.loadTenantInfo();
  }
  
  private loadTenantInfo(): void {
    // 1. Desde localStorage (login previo)
    const stored = localStorage.getItem('tenant_info');
    if (stored) {
      this.tenantInfo$.next(JSON.parse(stored));
    }
    
    // 2. Detectar desde URL/subdomain
    const hostname = window.location.hostname;
    if (hostname.includes('.siinadseg.com')) {
      const tenantId = hostname.split('.')[0];
      this.loadTenantById(tenantId);
    }
  }
  
  private loadTenantById(tenantId: string): void {
    this.http.get<TenantInfo>(`/api/tenants/${tenantId}/info`).subscribe({
      next: (tenant) => {
        this.tenantInfo$.next(tenant);
        localStorage.setItem('tenant_info', JSON.stringify(tenant));
        this.applyTenantStyling(tenant);
      }
    });
  }
  
  private applyTenantStyling(tenant: TenantInfo): void {
    // Aplicar colores de la empresa
    document.documentElement.style.setProperty('--primary-color', tenant.primaryColor);
    document.documentElement.style.setProperty('--secondary-color', tenant.secondaryColor);
    
    // Aplicar logo
    const logoElements = document.querySelectorAll('.tenant-logo');
    logoElements.forEach(el => {
      (el as HTMLImageElement).src = tenant.logoUrl;
    });
    
    // Título de la página
    document.title = `${tenant.companyName} - SIINADSEG`;
  }
  
  getTenantInfo(): Observable<TenantInfo | null> {
    return this.tenantInfo$.asObservable();
  }
  
  getCurrentTenant(): TenantInfo | null {
    return this.tenantInfo$.value;
  }
}

// interfaces/tenant.interface.ts
export interface TenantInfo {
  tenantId: string;
  companyName: string;
  logoUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  subscriptionPlan: string;
  maxUsers: number;
  maxPolizas: number;
  isActive: boolean;
}

// guards/tenant.guard.ts
@Injectable({ providedIn: 'root' })
export class TenantGuard implements CanActivate {
  constructor(
    private tenantService: TenantService,
    private router: Router
  ) {}
  
  canActivate(): Observable<boolean> {
    return this.tenantService.getTenantInfo().pipe(
      map(tenant => {
        if (!tenant || !tenant.isActive) {
          this.router.navigate(['/tenant-not-found']);
          return false;
        }
        return true;
      })
    );
  }
}
```

### **4. Interceptor para Multi-Tenancy:**

```typescript
// interceptors/tenant.interceptor.ts
@Injectable()
export class TenantInterceptor implements HttpInterceptor {
  constructor(private tenantService: TenantService) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const tenant = this.tenantService.getCurrentTenant();
    
    if (tenant) {
      // Agregar header con tenant ID
      const tenantReq = req.clone({
        headers: req.headers.set('X-Tenant-ID', tenant.tenantId)
      });
      return next.handle(tenantReq);
    }
    
    return next.handle(req);
  }
}
```

### **5. Routing Multi-Tenant:**

```typescript
// app-routing.module.ts
const routes: Routes = [
  {
    path: '',
    canActivate: [TenantGuard],
    children: [
      { path: 'dashboard', loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule) },
      { path: 'polizas', loadChildren: () => import('./polizas/polizas.module').then(m => m.PolizasModule) },
      { path: 'cobros', loadChildren: () => import('./cobros/cobros.module').then(m => m.CobrosModule) },
      { path: 'reclamos', loadChildren: () => import('./reclamos/reclamos.module').then(m => m.ReclamosModule) }
    ]
  },
  { path: 'tenant-not-found', component: TenantNotFoundComponent },
  { path: 'select-tenant', component: TenantSelectorComponent },
  { path: '**', redirectTo: '/dashboard' }
];
```

## 🚀 **Plan de Migración Multi-Tenant**

### **Fase 1: Preparación (2-3 días)**
```bash
# 1. Crear Master Database
sqlcmd -S "server" -Q "CREATE DATABASE SinsegMaster"

# 2. Ejecutar scripts de esquema master
sqlcmd -S "server" -d "SinsegMaster" -i "create_master_schema.sql"

# 3. Migrar datos existentes como primer tenant
INSERT INTO Tenants (TenantId, CompanyName, DatabaseName, ConnectionString, SubscriptionPlan)
VALUES ('empresa-default', 'Mi Empresa', 'SinsegTenant_Default', 'connection_string', 'Professional')
```

### **Fase 2: Código Backend (3-4 días)**
- [ ] Implementar TenantService y middleware
- [ ] Crear TenantDbContext dinámico
- [ ] Actualizar controllers con tenant context
- [ ] Implementar tenant provisioning API

### **Fase 3: Frontend (2-3 días)**
- [ ] Implementar TenantService
- [ ] Crear guards y interceptors
- [ ] Aplicar styling dinámico
- [ ] Crear componente selector de empresa

### **Fase 4: Testing y Deploy (2-3 días)**
- [ ] Testing multi-tenant completo
- [ ] Configurar subdominios en DNS
- [ ] Deploy a producción
- [ ] Documentación y training

## 💡 **Funcionalidades Adicionales**

### **1. Tenant Provisioning Automático:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class TenantProvisioningController : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        // 1. Validar disponibilidad del tenantId
        if (await _tenantService.TenantExistsAsync(request.TenantId))
            return BadRequest("Tenant ID already exists");
            
        // 2. Crear database para el tenant
        var connectionString = await _databaseService.CreateTenantDatabaseAsync(request.TenantId);
        
        // 3. Ejecutar migrations en nueva database
        await _migrationService.RunMigrationsAsync(connectionString);
        
        // 4. Registrar tenant en master database
        var tenant = new Tenant
        {
            TenantId = request.TenantId,
            CompanyName = request.CompanyName,
            DatabaseName = $"SinsegTenant_{request.TenantId}",
            ConnectionString = connectionString,
            SubscriptionPlan = request.Plan
        };
        
        await _tenantRepository.CreateAsync(tenant);
        
        // 5. Crear usuario admin inicial
        await _userService.CreateTenantAdminAsync(tenant.TenantId, request.AdminEmail);
        
        return Ok(new { TenantId = tenant.TenantId, Status = "Created" });
    }
}
```

### **2. Facturación por Tenant:**
```csharp
public class TenantBillingService
{
    public async Task<decimal> CalculateMonthlyBillAsync(string tenantId)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId);
        var usage = await _usageService.GetMonthlyUsageAsync(tenantId);
        
        decimal baseFee = tenant.SubscriptionPlan switch
        {
            "Basic" => 29.99m,
            "Professional" => 99.99m,
            "Enterprise" => 299.99m,
            _ => 0m
        };
        
        // Cargos adicionales por uso
        decimal extraUsers = Math.Max(0, usage.ActiveUsers - tenant.MaxUsers) * 5.00m;
        decimal extraPolizas = Math.Max(0, usage.TotalPolizas - tenant.MaxPolizas) * 0.10m;
        
        return baseFee + extraUsers + extraPolizas;
    }
}
```

### **3. Configuración por Tenant:**
```typescript
// Cada empresa puede tener configuraciones únicas
interface TenantConfig {
  tenantId: string;
  branding: {
    logoUrl: string;
    primaryColor: string;
    secondaryColor: string;
    customCss?: string;
  };
  features: {
    enableReclamos: boolean;
    enableReportes: boolean;
    enableIntegraciones: boolean;
  };
  limits: {
    maxUsers: number;
    maxPolizas: number;
    storageGB: number;
  };
  integrations: {
    emailProvider: 'sendgrid' | 'mailgun' | 'smtp';
    paymentProvider: 'stripe' | 'paypal' | 'local';
    smsProvider: 'twilio' | 'nexmo' | null;
  };
}
```

## 🎯 **Beneficios de esta Implementación**

1. **🔒 Seguridad Total:** Cada empresa tiene su propia base de datos
2. **🎨 Personalización:** Logo, colores, features por empresa
3. **📈 Escalabilidad:** Agregar empresas sin afectar performance
4. **💰 Monetización:** Facturación flexible por uso y features
5. **🛠️ Mantenimiento:** Updates y backups independientes
6. **📊 Analytics:** Métricas separadas por empresa
7. **🌐 Subdominios:** empresa.siinadseg.com para cada cliente

¿Quieres que implemente alguna parte específica de esta arquitectura multi-tenant?