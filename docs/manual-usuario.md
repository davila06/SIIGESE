# Manual de Usuario — SIINSEG
## Sistema Integral de Gestión de Seguros
**Versión:** 1.0 | **Fecha:** Marzo 2026

---

## Tabla de Contenidos

1. [Bienvenida a SIINSEG](#1-bienvenida-a-siinseg)
2. [Requisitos para usar el sistema](#2-requisitos-para-usar-el-sistema)
3. [Acceso al sistema — Login](#3-acceso-al-sistema--login)
4. [Cambio de contraseña](#4-cambio-de-contraseña)
5. [Navegación general](#5-navegación-general)
6. [Módulo de Pólizas](#6-módulo-de-pólizas)
7. [Módulo de Cotizaciones](#7-módulo-de-cotizaciones)
8. [Módulo de Cobros](#8-módulo-de-cobros)
9. [Módulo de Reclamos](#9-módulo-de-reclamos)
10. [Módulo de Usuarios (Admin)](#10-módulo-de-usuarios-admin)
11. [Módulo de Emails](#11-módulo-de-emails)
12. [Módulo de Configuración (Admin)](#12-módulo-de-configuración-admin)
13. [Módulo de Chat](#13-módulo-de-chat)
14. [Importación masiva de pólizas (Excel)](#14-importación-masiva-de-pólizas-excel)
15. [Glosario de términos](#15-glosario-de-términos)
16. [Preguntas frecuentes](#16-preguntas-frecuentes)
17. [Solución de problemas comunes](#17-solución-de-problemas-comunes)

---

## 1. Bienvenida a SIINSEG

SIINSEG es el sistema de gestión de seguros de su agencia. Con él puede:

- 📋 **Gestionar pólizas** — ver, crear, editar y buscar todas las pólizas activas de sus clientes
- 💬 **Crear cotizaciones** — generar propuestas comerciales para nuevos clientes y convertirlas en pólizas
- 💰 **Controlar cobros** — ver qué está pendiente de pago, registrar pagos y generar cobros automáticamente
- 📣 **Manejar reclamos** — dar seguimiento completo a siniestros, quejas y solicitudes
- 📧 **Enviar emails** — notificar a clientes sobre cobros, vencimientos y comunicaciones generales
- 💬 **Chat interno** — comunicarse con el equipo de la agencia en tiempo real

El sistema funciona **desde cualquier navegador web**, sin necesidad de instalar nada.

---

## 2. Requisitos para usar el sistema

### Dispositivos compatibles
- Computadora de escritorio o laptop (recomendado)
- Tablet (compatible)
- Celular (compatible, pero pantalla pequeña para tablas grandes)

### Navegadores compatibles
| Navegador | Versión mínima | Recomendado |
|---|---|---|
| Google Chrome | 90+ | ✅ Recomendado |
| Mozilla Firefox | 88+ | ✅ |
| Microsoft Edge | 90+ | ✅ |
| Safari (Mac/iOS) | 14+ | ✅ |
| Internet Explorer | Cualquiera | ❌ No soportado |

### Conexión a internet
- Se requiere conexión a internet activa en todo momento
- El sistema **no funciona offline**

### Credenciales
- Necesita su correo electrónico y contraseña, proporcionados por el administrador del sistema

---

## 3. Acceso al sistema — Login

### Paso a paso

1. Abra su navegador y vaya a la dirección del sistema (ej: `https://su-agencia.siinseg.com`)
2. Verá la pantalla de inicio de sesión

```
┌─────────────────────────────────┐
│         SIINSEG                 │
│   Sistema de Gestión            │
│                                 │
│  ┌───────────────────────────┐  │
│  │  correo@ejemplo.com       │  │
│  └───────────────────────────┘  │
│  ┌───────────────────────────┐  │
│  │  ••••••••                 │  │
│  └───────────────────────────┘  │
│                                 │
│  [ INGRESAR AL SISTEMA ]        │
│                                 │
│  ¿Olvidó su contraseña?         │
└─────────────────────────────────┘
```

3. Ingrese su **correo electrónico** y **contraseña**
4. Haga clic en **"INGRESAR AL SISTEMA"**

### ¿Olvidó su contraseña?

1. Haga clic en "¿Olvidó su contraseña?"
2. Ingrese su correo electrónico registrado
3. Recibirá un email con un enlace para restablecer su contraseña
4. El enlace es válido por **24 horas**
5. Haga clic en el enlace del email y defina su nueva contraseña

> **Importante:** Si no recibe el email en 5 minutos, revise la carpeta de spam/correo no deseado.

### Errores comunes al iniciar sesión

| Mensaje | Causa | Solución |
|---|---|---|
| "Email o contraseña incorrectos" | Credenciales equivocadas | Verifique el correo y contraseña |
| "Usuario inactivo" | Cuenta desactivada | Contacte al administrador |
| Pantalla no carga | Sin internet | Verifique su conexión |

### Cierre de sesión automático

La sesión expira automáticamente después de **8 horas de inactividad**. Al cerrar el navegador, la sesión se cierra. Para cerrar sesión manualmente, busque el botón de cierre de sesión en el menú.

---

## 4. Cambio de contraseña

### Primera vez que ingresa

Si el administrador creó su cuenta recientemente, el sistema le pedirá cambiar su contraseña antes de poder acceder. Esta es una medida de seguridad.

1. Ingrese su contraseña temporal (la que le dio el administrador)
2. Ingrese su nueva contraseña (mínimo 8 caracteres)
3. Confirme la nueva contraseña
4. Haga clic en "Guardar"

### Cambiar contraseña voluntariamente

Puede cambiar su contraseña en cualquier momento desde el menú de perfil o desde el módulo de configuración.

### Requisitos de contraseña
- Mínimo **8 caracteres**
- Se recomienda combinar: letras, números y un símbolo especial
- Ejemplos de contraseña segura: `Agente2026!`, `Seguros#CR24`

---

## 5. Navegación general

### Menú lateral

Después de ingresar, verá el menú lateral con los módulos disponibles:

```
┌──────────────────┐
│  SIINSEG         │
│  [nombre usuario]│
├──────────────────┤
│ 📋 Pólizas       │
│ 💬 Cotizaciones  │
│ 💰 Cobros        │
│ 📣 Reclamos      │
│ 📧 Emails        │
│ 💬 Chat          │
│ ─────────────    │ ← Solo Admin:
│ 👥 Usuarios      │
│ ⚙️  Configuración│
└──────────────────┘
```

> **Nota:** Los módulos visibles dependen de su rol. Los usuarios con rol **Admin** ven todos los módulos. Los **Agentes** ven todos excepto Usuarios y Configuración.

### Barra superior

- **Nombre del usuario** — su nombre y rol
- **Botón de cerrar sesión** — para salir de forma segura

### Tablas de datos

Todas las tablas del sistema tienen funciones comunes:

| Icono/Botón | Función |
|---|---|
| Columna de encabezado (clic) | Ordenar por esa columna (asc/desc) |
| 🔍 Barra de búsqueda | Filtrar registros en tiempo real |
| ✏️ Lápiz o "Editar" | Abrir formulario de edición |
| 🗑️ Papelera o "Eliminar" | Eliminar/desactivar registro |
| ➕ "Nuevo" o "Crear" | Abrir formulario de creación |

---

## 6. Módulo de Pólizas

La sección de Pólizas es el corazón del sistema. Aquí administra todas las pólizas de seguros de sus clientes.

### 6.1 Ver pólizas

Al ingresar a Pólizas, verá una tabla con todas las pólizas activas:

| Columna | Descripción |
|---|---|
| Número Póliza | Identificador único de la póliza |
| Nombre Asegurado | Nombre del cliente titular |
| Aseguradora | INS, SAGICOR, ASSA, BCR, MAPFRE u Otras |
| Modalidad | BASICO, PLUS, PREMIUM o TOTAL |
| Prima | Monto de la prima de seguro |
| Moneda | CRC (₡), USD ($) o EUR (€) |
| Frecuencia | Cada cuánto se cobra |
| Vigencia | Fecha de inicio de la póliza |

### 6.2 Buscar una póliza

Use la barra de búsqueda para encontrar pólizas por:
- Número de póliza
- Nombre del asegurado
- Número de cédula

**Ejemplo:** Escriba "Mora" y el sistema filtrará todas las pólizas con "Mora" en el nombre.

### 6.3 Crear una póliza nueva

1. Haga clic en **"+ Nueva Póliza"**
2. Complete el formulario:

**Sección: Datos de la Póliza**
- **Número de Póliza** — asignado por la aseguradora (ej: `INS-2026-001234`)
- **Aseguradora** — seleccione de la lista
- **Modalidad** — plan del seguro (BASICO, PLUS, PREMIUM, TOTAL)
- **Frecuencia de cobro** — cada cuánto tiempo se generará un cobro

**Sección: Datos del Asegurado**
- **Nombre completo** del asegurado
- **Número de cédula** o ID
- **Correo electrónico** — para envío de notificaciones
- **Teléfono**

**Sección: Datos Económicos**
- **Prima** — monto del seguro a cobrar
- **Moneda** — CRC, USD o EUR
- **Fecha de vigencia** — fecha de inicio de la cobertura

**Sección: Datos del Vehículo** *(si aplica)*
- Placa, Marca, Modelo, Año

**Sección: Observaciones**
- Notas adicionales relevantes para la póliza

3. Haga clic en **"Guardar"**

> **Consejo:** Una vez guardada la póliza, puede ir al módulo de Cobros y hacer clic en "Generar Cobros" para crear automáticamente todos los cobros futuros de esta póliza.

### 6.4 Editar una póliza

1. Encuentre la póliza en la tabla
2. Haga clic en el ícono de editar (lápiz ✏️) o en el número de póliza
3. Modifique los campos necesarios
4. Haga clic en **"Actualizar"**

> **Importante:** Cambiar la Prima de una póliza **no afecta cobros ya existentes**. Solo afecta los nuevos cobros que se generen.

### 6.5 Desactivar una póliza

Solo los **Administradores** pueden desactivar pólizas.

1. Encuentra la póliza
2. Haga clic en el ícono de eliminar (🗑️)
3. Confirme la acción en el diálogo
4. La póliza se desactiva (no se borra, queda en el historial)

### 6.6 Filtrar por aseguradora o frecuencia

Puede usar los filtros en la parte superior de la tabla para ver solo las pólizas de:
- Una aseguradora específica (INS, SAGICOR, etc.)
- Una frecuencia de cobro específica (Mensual, Anual, etc.)

---

## 7. Módulo de Cotizaciones

Las cotizaciones son propuestas comerciales que puede presentar a clientes potenciales. Una cotización aprobada puede convertirse directamente en póliza.

### 7.1 Estados de una cotización

```
PENDIENTE ──→ APROBADA ──→ CONVERTIDA (se crea la Póliza)
     └──────→ RECHAZADA
```

| Estado | Color | Significado |
|---|---|---|
| PENDIENTE | Amarillo | Esperando revisión o decisión del cliente |
| APROBADA | Verde | Cliente aceptó, lista para convertir |
| RECHAZADA | Rojo | Cliente rechazó la propuesta |
| CONVERTIDA | Azul | Ya se creó la póliza correspondiente |

### 7.2 Crear una cotización nueva

1. Haga clic en **"+ Nueva Cotización"**
2. Complete el formulario según el tipo de seguro:

**Datos del Cliente**
- Nombre completo
- Correo electrónico
- Teléfono

**Datos del Seguro**
- **Tipo de seguro:**
  - **AUTO** — para vehículos → ingrese Placa, Marca, Modelo, Año, Cilindraje
  - **VIDA** — seguro de vida → ingrese Fecha de nacimiento, Género, Ocupación
  - **HOGAR** — para inmuebles → ingrese Dirección, Tipo de inmueble, Valor del inmueble
  - **EMPRESARIAL** — para empresas
- **Modalidad** — BASICO, PLUS, PREMIUM, TOTAL
- **Monto asegurado** — valor del bien o cobertura solicitada (obligatorio)
- **Prima cotizada** — la prima que propone cobrar (opcional, puede dejarse en blanco si aún no ha calculado)
- **Moneda** — CRC, USD o EUR
- **Frecuencia de pago propuesta**
- **Aseguradora** — a través de quién cotizó
- **Vigencia:** fecha de inicio y fin propuesta
- **Observaciones**

3. Haga clic en **"Guardar"**

### 7.3 Gestionar una cotización existente

**Aprobar una cotización:**
1. Encuentre la cotización (estado PENDIENTE)
2. Haga clic en **"Aprobar"**
3. Confirme → el estado cambia a APROBADA

**Rechazar una cotización:**
1. Encuentre la cotización (estado PENDIENTE)
2. Haga clic en **"Rechazar"**
3. Puede agregar una nota de motivo

**Convertir en póliza:**
1. La cotización debe estar en estado APROBADA
2. Haga clic en **"Convertir a Póliza"**
3. El sistema creará automáticamente una nueva póliza con los datos de la cotización
4. La cotización queda en estado CONVERTIDA

### 7.4 Número de cotización

Las cotizaciones reciben un número automático con formato: `COT-{AÑO}-{NNN}`
**Ejemplo:** `COT-2026-001`, `COT-2026-042`

---

## 8. Módulo de Cobros

El módulo de cobros es donde controla toda la facturación de su cartera.

### 8.1 ¿Qué es un cobro?

Un cobro es el registro de un pago que un cliente debe realizar por su póliza. El sistema genera cobros automáticamente según la frecuencia de cada póliza.

### 8.2 Estados de un cobro

| Estado | Significado | Acción requerida |
|---|---|---|
| **Pendiente** | El cliente aún no ha pagado | Cobrar y registrar |
| **Pagado** | El cliente pagó | Ninguna |
| **Cobrado** | Variante de pagado | Ninguna |
| **Vencido** | Se pasó la fecha sin pagar | Gestionar mora |
| **Cancelado** | El cobro fue anulado | Ninguna |

### 8.3 Ver cobros

La tabla de cobros muestra:

| Columna | Descripción |
|---|---|
| Número Recibo | Identificador único (ej: `REC-202603-0042`) |
| Asegurado | Nombre del cliente |
| Monto Total | Prima a cobrar |
| Monto Cobrado | Lo que efectivamente pagó |
| Vencimiento | Fecha límite de pago |
| Estado | Estado actual |
| Método de Pago | Cómo pagó el cliente |

### 8.4 Generar cobros automáticos

1. Haga clic en **"Generar Cobros"**
2. El sistema revisará todas las pólizas activas
3. Para cada póliza, generará los cobros futuros que no existan aún
4. Verá un resumen: cuántos cobros nuevos se crearon

> **¿Cuándo usar esto?**
> - Cuando configure una póliza nueva
> - Al inicio de cada mes para asegurarse de tener todos los cobros del período
> - No se crean cobros duplicados — es seguro ejecutarlo múltiples veces

### 8.5 Registrar un pago

Cuando un cliente paga, registre el cobro así:

1. Encuentre el cobro en la tabla (estado: Pendiente o Vencido)
2. Haga clic en **"Registrar Pago"** o el ícono de cheque ✓
3. Complete:
   - **Monto cobrado** — lo que el cliente pagó (puede ser igual o diferente al monto total)
   - **Método de pago** — Efectivo, Tarjeta, Transferencia, Cheque u Otros
   - **Fecha de pago** — se autocompleta con la fecha actual
   - **Observaciones** — notas adicionales (ej: "Pagó con cheque #4521")
4. Haga clic en **"Confirmar"**
5. El estado cambia a **Pagado**

### 8.6 Estadísticas de cobros

En la parte superior del módulo verá un panel con:
- 📊 **Total de cobros** — cantidad total en el sistema
- ⏳ **Cobros pendientes** — cuántos están sin pagar
- ✅ **Cobros pagados** — cuántos se han cobrado
- 💰 **Monto total pendiente** — suma de lo que está por cobrar (en CRC)
- 💳 **Monto total cobrado** — suma de lo que ya ingresó
- 📈 **% cobrado** — porcentaje de efectividad

### 8.7 Número de recibo

Cada cobro tiene un número único con formato: `REC-{AÑOMES}-{NNNN}`
**Ejemplo:** `REC-202603-0042` = recibo número 42 de Marzo 2026

### 8.8 Enviar recordatorio de cobro

Para cobros pendientes o vencidos, puede enviar un email de recordatorio al cliente:
1. Seleccione el cobro
2. Haga clic en **"Enviar recordatorio"** o el ícono de email 📧
3. El sistema usará la plantilla de email configurada para cobros
4. Confirme y el email se enviará al correo registrado en la póliza

---

## 9. Módulo de Reclamos

Gestione todos los siniestros, quejas y solicitudes de sus clientes.

### 9.1 Estados de un reclamo

```
Pendiente → Abierto → En Revisión → En Proceso → Aprobado/Rechazado → Resuelto → Cerrado
```

| Estado | Color | Significado |
|---|---|---|
| Pendiente | Gris | Recién ingresado, sin asignar |
| Abierto | Azul | Asignado a un agente |
| En Revisión | Amarillo | Siendo evaluado |
| En Proceso | Naranja | Trámite en curso |
| Aprobado | Verde | Se aprobó el reclamo |
| Rechazado | Rojo | No procedió |
| Resuelto | Verde oscuro | Todo completo |
| Cerrado | Negro | Archivado |

### 9.2 Tipos de reclamo

| Tipo | Cuándo usarlo |
|---|---|
| **Siniestro** | Accidente, robo, incendio u otro evento cubierto |
| **Queja** | Insatisfacción con el servicio |
| **Sugerencia** | Propuesta de mejora |
| **Reclamo** | Disputa administrativa |
| **Otros** | Cualquier otro caso |

### 9.3 Prioridades

| Prioridad | Descripción | Tiempo de respuesta ideal |
|---|---|---|
| **Baja** | Consulta general | 5 días hábiles |
| **Media** | Situación normal | 3 días hábiles |
| **Alta** | Urgente | 24 horas |
| **Crítica** | Emergencia | Inmediato |

### 9.4 Crear un reclamo nuevo

1. Haga clic en **"+ Nuevo Reclamo"**
2. Complete el formulario:
   - **Número de Póliza** — la póliza asociada al reclamo
   - **Nombre del asegurado** — se puede autocompletar al ingresar la póliza
   - **Tipo de reclamo**
   - **Prioridad**
   - **Descripción** — detalle claro del problema o solicitud
   - **Fecha del reclamo** — cuándo ocurrió el evento
   - **Fecha límite de respuesta** — cuándo hay que dar respuesta
   - **Monto reclamado** — si aplica (para siniestros)
   - **Moneda**
   - **Observaciones**
   - **Documentos adjuntos** — nombre o referencia de documentos
3. Haga clic en **"Guardar"**

### 9.5 Actualizar el estado de un reclamo

1. Encuentre el reclamo
2. Haga clic en **"Cambiar Estado"**
3. Seleccione el nuevo estado
4. Agregue observaciones del cambio
5. Confirme

### 9.6 Asignar un reclamo a un usuario

Solo los **Administradores** pueden asignar reclamos:
1. Abra el reclamo
2. Haga clic en **"Asignar"**
3. Seleccione el usuario responsable
4. El estado cambia a Abierto automáticamente

### 9.7 Registrar monto aprobado

Cuando un siniestro es aprobado:
1. Abra el reclamo
2. Al cambiar estado a **"Aprobado"**, ingrese el **Monto Aprobado**
3. Este monto puede ser igual o diferente al reclamado inicialmente

---

## 10. Módulo de Usuarios (Admin)

> **Solo disponible para usuarios con rol Administrador**

### 10.1 Lista de usuarios

Muestra todos los usuarios del sistema con:
- Nombre completo
- Correo electrónico
- Rol asignado
- Estado (Activo/Inactivo)
- Último inicio de sesión

### 10.2 Crear un nuevo usuario

1. Haga clic en **"+ Nuevo Usuario"**
2. Complete:
   - **Nombre** y **Apellido**
   - **Correo electrónico** — será el usuario de acceso
   - **Contraseña temporal** — el usuario deberá cambiarla en su primer acceso
   - **Rol** — Admin, Agent o DataLoader
3. Haga clic en **"Crear"**
4. El sistema marcará al usuario con "requiere cambio de contraseña"
5. Comparta las credenciales con el usuario de forma segura

### 10.3 Roles disponibles

| Rol | Acceso | Ideal para |
|---|---|---|
| **Admin** | Todo el sistema | Jefe de agencia, gerente |
| **Agent** | Todo excepto Usuarios y Configuración | Agentes de seguros |
| **DataLoader** | Pólizas + Upload Excel | Personal de digitación |

### 10.4 Editar un usuario

1. Haga clic en editar (✏️) junto al usuario
2. Puede cambiar: nombre, correo, rol, estado activo/inactivo
3. Para restablecer contraseña: active "Forzar cambio de contraseña en próximo acceso"

### 10.5 Desactivar un usuario

Cuando un empleado ya no trabaja en la agencia:
1. Haga clic en editar
2. Desmarque "Activo"
3. El usuario no podrá iniciar sesión pero sus datos históricos se conservan

> **Buena práctica:** Siempre desactive usuarios (no los elimine) para mantener el historial de quién hizo qué.

---

## 11. Módulo de Emails

### 11.1 ¿Para qué sirve?

El módulo de emails le permite:
- Enviar notificaciones individuales a clientes
- Hacer envíos masivos a múltiples destinatarios
- Enviar recordatorios de cobros vencidos
- Ver el historial de todos los emails enviados

### 11.2 Enviar un email individual

1. Ir a **Emails → Enviar**
2. Complete:
   - **Para:** dirección de email del destinatario
   - **Asunto:** tema del mensaje
   - **Mensaje:** contenido del email
3. Haga clic en **"Enviar"**

### 11.3 Envío masivo

Para notificar a muchos clientes al mismo tiempo:
1. Ir a **Emails → Envío Masivo**
2. Suba una lista de destinatarios o seleccione un grupo (ej: todos los clientes con cobros vencidos)
3. Defina el asunto y el mensaje
4. Revise la lista antes de enviar
5. Confirme el envío

### 11.4 Enviar recordatorio de cobro vencido

1. Ir a **Emails → Cobro Vencido**
2. Seleccione el cobro vencido (o ingréselo manualmente)
3. El sistema pre-llena el mensaje con la plantilla configurada
4. Revise y envíe

### 11.5 Historial de emails

El historial guarda todos los emails enviados con:
- Fecha y hora de envío
- Destinatario
- Asunto
- Estado (Enviado / Error)

---

## 12. Módulo de Configuración (Admin)

> **Solo disponible para Administradores**

### 12.1 Configuración de email SMTP

Para que el sistema pueda enviar emails, debe configurar un servidor SMTP:

1. Ir a **Configuración → Email**
2. Haga clic en **"+ Nueva Configuración"**
3. Complete los datos de su servidor de correo:

| Campo | Descripción | Ejemplo |
|---|---|---|
| Nombre | Identificador para esta configuración | "Gmail Principal" |
| Servidor SMTP | Dirección del servidor | `smtp.gmail.com` |
| Puerto | Puerto de conexión | `587` (TLS) o `465` (SSL) |
| Usuario | Correo o usuario SMTP | `agencia@gmail.com` |
| Contraseña | Contraseña de la cuenta | `*****` |
| SSL/TLS | Tipo de cifrado | Según el servidor |
| Correo origen | De quién se envía | `noreply@miagencia.com` |
| Nombre origen | Nombre del remitente | `Agencia de Seguros XYZ` |

**Datos de la empresa (aparecen en los emails):**
- Nombre de la empresa
- Dirección
- Teléfono
- Sitio web
- Logo (URL de imagen)

4. Haga clic en **"Probar Conexión"** para verificar que funciona
5. Si la prueba es exitosa, haga clic en **"Guardar"**
6. Si tiene múltiples configuraciones, marque una como **"Predeterminada"**

### 12.2 Plantilla de email para cobros

Configure el formato del email que se envía a los clientes cuando tienen cobros pendientes:

1. En la configuración de email, busque la sección **"Plantilla de Cobros"**
2. Defina el **Asunto** del email
3. Redacte el **cuerpo** del mensaje

Use estas etiquetas para insertar datos dinámicos:
```
{NombreCliente}      → Nombre del asegurado
{NumeroPoliza}       → Número de la póliza
{NumeroRecibo}       → Número del recibo
{MontoTotal}         → Monto a pagar (con símbolo de moneda)
{FechaVencimiento}   → Fecha límite de pago
{CompanyName}        → Nombre de su agencia
{CompanyPhone}       → Teléfono de su agencia
```

**Ejemplo de plantilla:**
```
Asunto: Recordatorio de pago — Póliza {NumeroPoliza}

Estimado/a {NombreCliente},

Le recordamos que tiene un cobro pendiente:

📄 Póliza: {NumeroPoliza}
🧾 Recibo: {NumeroRecibo}  
💰 Monto: {MontoTotal}
📅 Vence: {FechaVencimiento}

Para consultas: {CompanyPhone}

{CompanyName}
```

### 12.3 Múltiples configuraciones SMTP

Si tiene varias cuentas de correo (ej: una para cobros, otra para reclamos):
1. Cree múltiples configuraciones con diferentes nombres
2. Marque la principal como "Predeterminada"
3. Las otras se pueden usar para envíos específicos

---

## 13. Módulo de Chat

### 13.1 ¿Para qué sirve el chat?

El chat es para comunicación **interna** del equipo de la agencia. No es para chatear con clientes.

Casos de uso:
- Coordinar entre agentes sobre casos específicos
- Consultas internas del equipo
- Comunicaciones de la gerencia al equipo

### 13.2 Crear una nueva conversación

1. Ir al módulo **Chat**
2. Haga clic en **"Nueva conversación"**
3. Ponga un título descriptivo (ej: "Caso póliza INS-2026-001234")
4. Haga clic en **"Crear"**

### 13.3 Enviar mensajes

1. Seleccione la conversación en el panel izquierdo
2. Escriba su mensaje en el campo de texto inferior
3. Presione **Enter** o haga clic en el botón de enviar

### 13.4 Mensajes en tiempo real

Los mensajes aparecen en tiempo real para todos los participantes activos en esa sesión. Verá un indicador de "escribiendo..." cuando otro usuario esté redactando un mensaje.

### 13.5 Reaccionar a un mensaje

Puede darle 👍 (like) o 👎 (dislike) a mensajes para indicar si la información fue útil.

---

## 14. Importación masiva de pólizas (Excel)

> **Disponible para usuarios con rol Admin o DataLoader**

Esta función le permite cargar cientos de pólizas de una vez desde un archivo Excel.

### 14.1 Acceder al módulo

Ir a **Pólizas → Importar desde Excel** (`/polizas/upload`)

### 14.2 Preparar el archivo Excel

Su archivo `.xlsx` debe tener:
- **Primera fila:** nombres de columnas (encabezados)
- **Siguientes filas:** datos de cada póliza

**Columnas recomendadas:**
| Columna | Obligatorio | Descripción |
|---|---|---|
| NumeroPoliza | ✅ | Número de la póliza |
| NombreAsegurado | ✅ | Nombre completo del asegurado |
| NumeroCedula | ✅ | Cédula o ID del cliente |
| Prima | ✅ | Prima mensual/según frecuencia |
| Moneda | ✅ | CRC, USD o EUR |
| Frecuencia | ✅ | MENSUAL, DM, ANUAL, etc. |
| FechaVigencia | ✅ | DD-MM-YYYY |
| Aseguradora | ✅ | INS, SAGICOR, ASSA, etc. |
| Modalidad | ❌ | BASICO, PLUS, PREMIUM, TOTAL |
| Correo | ❌ | Email del cliente |
| NumeroTelefono | ❌ | Teléfono del cliente |
| Placa | ❌ | Para seguros de automóvil |
| Marca | ❌ | Marca del vehículo |
| Modelo | ❌ | Modelo del vehículo |
| Año | ❌ | Año del vehículo |
| Observaciones | ❌ | Notas adicionales |

### 14.3 Realizar la importación

1. Haga clic en **"Seleccionar archivo"** o arrastre el archivo a la zona de carga
2. Solo se aceptan archivos `.xlsx` o `.xls`
3. Tamaño máximo: **10 MB**
4. Haga clic en **"Importar"**
5. El sistema procesará cada fila y mostrará un resumen:
   - ✅ Pólizas importadas correctamente
   - ⚠️ Filas con errores (con descripción del error)

### 14.4 Errores comunes en la importación

| Error | Causa | Solución |
|---|---|---|
| "Número de póliza duplicado" | Ya existe esa póliza | Verificar si ya fue importada |
| "Prima inválida" | Prima negativa o texto | Asegurar que sea un número positivo |
| "Fecha inválida" | Formato incorrecto | Usar formato DD-MM-YYYY |
| "Moneda no reconocida" | Valor diferente a CRC/USD/EUR | Corregir el valor en Excel |
| "Frecuencia no reconocida" | Valor no estándar | Usar: MENSUAL, DM, BIMESTRAL, TRIMESTRAL, SEMESTRAL, ANUAL |

---

## 15. Glosario de términos

| Término | Definición |
|---|---|
| **Póliza** | Contrato de seguro entre el cliente y la aseguradora, administrado por la agencia |
| **Prima** | Monto que el cliente paga por tener la cobertura del seguro |
| **Vigencia** | Período durante el cual la póliza tiene cobertura activa |
| **Aseguradora** | Empresa que emite y respalda la póliza (INS, SAGICOR, ASSA, etc.) |
| **Modalidad** | Nivel o plan del seguro: BASICO, PLUS, PREMIUM, TOTAL |
| **Frecuencia** | Periodicidad de los pagos: Mensual, Trimestral, Semestral, Anual, etc. |
| **Cobro** | Registro de un pago que un cliente debe realizar |
| **Recibo** | Numeración única de un cobro (`REC-YYYYMM-NNNN`) |
| **DM** | Débito mensual — frecuencia mensual de cobro |
| **Cotización** | Propuesta comercial de un seguro, antes de formalizarse como póliza |
| **Reclamo** | Solicitud del cliente por un siniestro u otra gestión |
| **Siniestro** | Evento cubierto por el seguro (accidente, robo, etc.) |
| **Monto Asegurado** | Valor máximo que cubre el seguro en caso de siniestro |
| **Monto Cobrado** | Pago efectivamente recibido del cliente |
| **Estado Vencido** | Un cobro que superó su fecha límite sin ser pagado |
| **NIT / Cédula** | Número de identificación del cliente |
| **Perfil** | Agrupación de pólizas por cliente o cartera |
| **CRC** | Colón costarricense (₡) |
| **Soft delete** | Desactivar un registro sin eliminarlo físicamente — conserva el historial |

---

## 16. Preguntas frecuentes

### Sobre el sistema en general

**¿Puedo usar el sistema desde mi celular?**
Sí, el sistema es compatible con dispositivos móviles, aunque para trabajar con tablas grandes se recomienda una computadora.

**¿Se guardan los cambios automáticamente?**
No. Debe hacer clic en "Guardar" o "Actualizar" para que los cambios se apliquen.

**¿Qué pasa si cierro el navegador sin guardar?**
Los cambios no guardados se perderán. El sistema no tiene guardado automático.

**¿Puedo acceder desde dos dispositivos al mismo tiempo?**
Sí, puede tener diferentes sesiones activas en distintos dispositivos.

### Sobre pólizas

**¿Puedo editar una póliza ya creada?**
Sí, puede editarla en cualquier momento. Los cambios en la Prima solo afectan cobros futuros.

**¿Qué pasa si desactivo una póliza?**
La póliza deja de aparecer en las listas activas pero sus cobros y reclamos históricos se conservan.

**¿Cuántas pólizas puede manejar el sistema?**
No tiene un límite definido. El sistema está diseñado para manejar miles de pólizas.

### Sobre cobros

**¿Cuándo debo generar cobros?**
- Al agregar una póliza nueva
- Al inicio de cada período (mes, trimestre, etc.)
- Cuando necesite cobros por adelantado para un período largo

**¿Se crean cobros duplicados si genero varias veces?**
No. El sistema detecta cobros ya existentes y no los duplica.

**¿Puedo registrar un pago parcial?**
Sí. Al registrar el pago, ingrese el monto real cobrado aunque sea menor al monto total.

**¿Cómo marco un cobro como vencido?**
El sistema lo cambia automáticamente cuando la fecha de vencimiento pasa y el cobro sigue pendiente.

### Sobre cotizaciones

**¿Puedo convertir una cotización rechazada en póliza?**
No. Solo las cotizaciones en estado APROBADA pueden convertirse en póliza. Puede duplicar y crear una nueva cotización.

**¿Se pueden editar cotizaciones ya convertidas?**
No. Una cotización convertida es de solo lectura.

### Sobre emails

**¿Por qué no llegan los emails?**
1. Verifique la configuración SMTP (ir a Configuración → Email → Probar Conexión)
2. El destinatario puede tener filtros de spam activos
3. Si usó Gmail, verifique que tiene habilitado "Acceso de apps menos seguras" o configure una contraseña de aplicación

**¿Hay un límite de emails que puedo enviar?**
Depende del proveedor SMTP que esté usando. Gmail permite ~500 emails diarios en cuentas normales.

### Sobre usuarios y accesos

**¿Puedo cambiar mi propio rol?**
No. Solo los administradores pueden cambiar roles.

**¿Qué pasa si olvido mi contraseña?**
Use la opción "¿Olvidó su contraseña?" en la pantalla de login o contacte al administrador.

**¿Un administrador puede ver mi contraseña?**
No. Las contraseñas están cifradas y **nadie** puede verlas, ni siquiera los administradores.

---

## 17. Solución de problemas comunes

### El sistema muestra "Error de conexión" o no carga

1. Verifique su conexión a internet
2. Recargue la página (F5 o Ctrl+R)
3. Limpie el caché del navegador (Ctrl+Shift+Delete → Caché/Cookies)
4. Intente con otro navegador
5. Si persiste, contacte al soporte técnico

### La sesión se cierra sola

El sistema cierra la sesión automáticamente después de **8 horas** de inactividad. Esto es normal y es una medida de seguridad.

### No puedo ver el módulo de Usuarios o Configuración

Estos módulos solo están disponibles para **Administradores**. Si necesita acceso, solicítelo al administrador del sistema.

### Los datos de una tabla no se actualizan

1. Haga clic en **"Actualizar"** o el ícono de recarga de la tabla
2. Recargue la página completa (F5)
3. Si el problema persiste, cierre sesión y vuelva a ingresar

### No puedo subir el archivo Excel

Verifique:
- El archivo es `.xlsx` o `.xls` (no `.csv`, no `.ods`)
- El tamaño no supera 10 MB
- Su usuario tiene rol **Admin** o **DataLoader**
- No tiene el archivo abierto en Excel al mismo tiempo que intenta subirlo

### Un cobro no aparece después de "Generar Cobros"

Posibles causas:
- La póliza tiene `Prima = 0` (se omiten las pólizas sin prima)
- La póliza está **inactiva**
- El cobro ya existía para ese período (no se duplican)

### Los emails no llegan al cliente

1. Ir a **Emails → Historial** y verificar si el email figura como enviado o con error
2. Ir a **Configuración → Email → Probar Conexión**
3. Verificar que el correo del cliente en la póliza esté correcto
4. Pedir al cliente que revise la carpeta de spam

---

## Apéndice — Atajos de teclado útiles

| Atajo | Acción |
|---|---|
| `Ctrl + F` (en tabla) | Buscar en la tabla actual |
| `F5` | Recargar la página |
| `Esc` | Cerrar un diálogo/modal abierto |
| `Enter` | Confirmar formularios (en algunos casos) |

---

## Contacto y soporte

Para soporte técnico o preguntas sobre el sistema, contacte a:

- **Soporte técnico:** [definir contacto del soporte]
- **Administrador del sistema:** [nombre del admin]
- **Email de soporte:** [email de soporte]

---

*Manual de Usuario SIINSEG — Versión 1.0 — Marzo 2026*
*Documento interno — No distribuir fuera de la organización*
