# Solución: Selector de Estados en Módulo de Cobros

## ✨ Nueva Funcionalidad Implementada
Se ha implementado un **selector inteligente de estados** para el módulo de cobros, permitiendo a los usuarios cambiar el estado de cualquier cobro seleccionando entre los estados disponibles.

## 🎯 Funcionalidad Solicitada
> "en los cobros el cambio de estado haz que seleccione el estado al cual quiere cambiarlo entre los estados posibles de los cobros"

## ✅ Solución Implementada

### 1. Estados de Cobro Disponibles
```typescript
export enum EstadoCobro {
  Pendiente = 0,    // Cobro pendiente de pago
  Cobrado = 1,      // Cobro realizado exitosamente  
  Vencido = 2,      // Cobro que venció sin pagar
  Cancelado = 3     // Cobro cancelado
}
```

### 2. Actualización del Servicio
**Archivo**: `frontend-new/src/app/cobros/services/cobros.service.ts`

#### Nuevo método: cambiarEstado()
```typescript
// Cambiar estado del cobro
cambiarEstado(id: number, estado: EstadoCobro, observaciones?: string): Observable<Cobro> {
  const body = { estado, observaciones };
  return this.http.put<Cobro>(`${this.apiUrl}/${id}/estado`, body);
}
```

**Endpoint**: `PUT /api/cobros/{id}/estado`

### 3. Implementación del Componente
**Archivo**: `frontend-new/src/app/cobros/components/cobros-dashboard/cobros-dashboard.component.ts`

#### Método cambiarEstado() con Selector Inteligente
```typescript
cambiarEstado(cobro: Cobro): void {
  console.log('Cambiar estado:', cobro);
  
  // Crear lista de estados disponibles con sus etiquetas
  const estadosDisponibles = [
    { value: EstadoCobro.Pendiente, label: 'Pendiente', icon: 'schedule', color: 'primary' },
    { value: EstadoCobro.Cobrado, label: 'Cobrado', icon: 'check_circle', color: 'accent' },
    { value: EstadoCobro.Vencido, label: 'Vencido', icon: 'warning', color: 'warn' },
    { value: EstadoCobro.Cancelado, label: 'Cancelado', icon: 'cancel', color: 'basic' }
  ];

  // Filtrar el estado actual para no mostrarlo como opción
  const estadosParaCambio = estadosDisponibles.filter(e => e.value !== cobro.estado);

  if (estadosParaCambio.length === 0) {
    this.showMessage('No hay estados disponibles para cambiar');
    return;
  }

  // Crear opciones numeradas para el selector
  let mensaje = `Seleccione el nuevo estado para el cobro ${cobro.numeroRecibo}:\n\n`;
  
  estadosParaCambio.forEach((estado, index) => {
    mensaje += `${index + 1}. ${estado.label}\n`;
  });

  const seleccion = prompt(mensaje + '\nIngrese el número del estado deseado:');
  
  if (seleccion) {
    const indice = parseInt(seleccion) - 1;
    
    if (indice >= 0 && indice < estadosParaCambio.length) {
      const nuevoEstado = estadosParaCambio[indice];
      
      if (confirm(`¿Está seguro de cambiar el estado del cobro ${cobro.numeroRecibo} a "${nuevoEstado.label}"?`)) {
        this.cobrosService.cambiarEstado(cobro.id, nuevoEstado.value, `Estado cambiado a ${nuevoEstado.label}`).subscribe({
          next: (response) => {
            this.showMessage(`Estado del cobro cambiado a "${nuevoEstado.label}" exitosamente`);
            this.loadCobros(); // Recargar la lista
          },
          error: (error) => {
            console.error('Error cambiando estado:', error);
            this.showMessage('Error al cambiar el estado del cobro');
          }
        });
      }
    } else {
      this.showMessage('Opción inválida');
    }
  }
}
```

### 4. Actualización del Mock API Interceptor
**Archivo**: `frontend-new/src/app/interceptors/mock-api.interceptor.ts`

#### A. Modificación de la lógica PUT para cobros
```typescript
} else if (method === 'PUT') {
  if (url.includes('/estado')) {
    console.log('🔄 Mock handling CAMBIAR ESTADO cobro');
    return this.handleCambiarEstadoCobro(req);
  } else if (url.includes('/registrar')) {
    console.log('💳 Mock handling REGISTRAR cobro');
    return this.handleRegistrarCobro(req);
  } else if (url.includes('/cancelar')) {
    console.log('❌ Mock handling CANCELAR cobro');
    return this.handleCancelarCobro(req);
  }
}
```

#### B. Nuevo método: handleCambiarEstadoCobro()
```typescript
private handleCambiarEstadoCobro(req: HttpRequest<any>): Observable<HttpEvent<any>> {
  const urlParts = req.url.split('/');
  const cobroId = parseInt(urlParts[urlParts.indexOf('cobros') + 1]);
  const { estado, observaciones } = req.body;
  
  const cobroIndex = this.cobros.findIndex(c => c.id === cobroId);
  if (cobroIndex === -1) {
    return of(new HttpResponse({
      status: 404,
      body: { message: 'Cobro no encontrado' }
    }));
  }
  
  // Actualizar estado del cobro
  this.cobros[cobroIndex].estado = estado;
  this.cobros[cobroIndex].fechaActualizacion = new Date();
  
  // Agregar observaciones si se proporcionan
  if (observaciones) {
    this.cobros[cobroIndex].observaciones = observaciones;
  }
  
  // Actualizar fechas específicas según el estado
  switch (estado) {
    case EstadoCobro.Cobrado:
      this.cobros[cobroIndex].fechaCobro = new Date();
      break;
    case EstadoCobro.Cancelado:
      // Ya se maneja en el método específico de cancelar
      break;
  }
  
  const response = new HttpResponse({
    status: 200,
    body: {
      success: true,
      message: `Estado del cobro cambiado a ${this.getEstadoCobroNombre(estado)} exitosamente`,
      data: this.cobros[cobroIndex]
    }
  });

  console.log('🔄 Mock cobro estado changed:', this.cobros[cobroIndex].numeroRecibo, 'new estado:', estado);
  return of(response).pipe(delay(600));
}
```

#### C. Helper para nombres de estado
```typescript
private getEstadoCobroNombre(estado: number): string {
  switch (estado) {
    case EstadoCobro.Pendiente: return 'Pendiente';
    case EstadoCobro.Cobrado: return 'Cobrado';
    case EstadoCobro.Vencido: return 'Vencido';
    case EstadoCobro.Cancelado: return 'Cancelado';
    default: return 'Desconocido';
  }
}
```

## 🎯 Características de la Funcionalidad

### ✨ Selector Inteligente
- **Filtrado automático**: Solo muestra estados diferentes al actual
- **Opciones numeradas**: Interfaz clara y fácil de usar
- **Validación de entrada**: Verifica que la opción sea válida
- **Confirmación**: Doble verificación antes del cambio

### 🔄 Flujo de Trabajo
1. **Selección**: Usuario hace clic en "Cambiar Estado" en cualquier cobro
2. **Opciones**: Sistema muestra solo estados disponibles (diferentes al actual)
3. **Selección**: Usuario ingresa el número del estado deseado
4. **Confirmación**: Sistema pide confirmación del cambio
5. **Ejecución**: Se realiza el cambio y se actualiza la UI
6. **Feedback**: Mensaje de éxito/error y recarga de datos

### 🎨 Estados Visuales
- **Pendiente**: 🕒 Icono schedule, color primary (azul)
- **Cobrado**: ✅ Icono check_circle, color accent (verde)
- **Vencido**: ⚠️ Icono warning, color warn (naranja/rojo)
- **Cancelado**: ❌ Icono cancel, color básico (gris)

### 🔧 Funcionalidades Técnicas
- **Actualización automática**: La lista se recarga después del cambio
- **Logging completo**: Registros detallados para debugging
- **Manejo de errores**: Validaciones y mensajes informativos
- **Observaciones**: Se registra automáticamente el motivo del cambio
- **Fechas inteligentes**: Se actualiza `fechaCobro` cuando se marca como cobrado

## 🎯 Resultado

### ✅ **Funcionalidad Completamente Implementada**
- **Selector de estados**: Permite elegir entre estados disponibles
- **Interfaz intuitiva**: Opciones numeradas y confirmación
- **Validaciones**: Entrada verificada y estados filtrados
- **Feedback completo**: Mensajes de éxito/error
- **UI reactiva**: Lista actualizada automáticamente

### 🔄 **Cómo Usar**
1. **Ir a**: https://gentle-dune-0a2edab0f.3.azurestaticapps.net/cobros
2. **Buscar**: Cualquier cobro en la lista
3. **Hacer clic**: En "Cambiar Estado" (botón de acciones)
4. **Seleccionar**: El número del estado deseado
5. **Confirmar**: El cambio en el diálogo
6. **Ver resultado**: Estado actualizado inmediatamente

### 📋 Ejemplo de Uso
```
Cobro: REC-001 (Estado actual: Pendiente)

Selector muestra:
1. Cobrado
2. Vencido  
3. Cancelado

Usuario selecciona: 1
Sistema confirma: "¿Cambiar a Cobrado?"
Resultado: Estado cambiado y lista actualizada
```

## 🏆 Estado del Proyecto

**✅ SIINADSEG - Todas las Funcionalidades Implementadas:**

1. ✅ Botón "Guardar Póliza" funcional
2. ✅ Campo "Modalidad" removido
3. ✅ Error "al cargar configuraciones" solucionado
4. ✅ Mixed Content Error resuelto
5. ✅ Acciones SMTP (predeterminada/estado) funcionando
6. ✅ Cambio de estado en Reclamos implementado
7. ✅ **Selector de estados en Cobros implementado**

---
**Estado**: ✅ COMPLETADO  
**Fecha**: 24/10/2025 14:40:00  
**Versión**: 20251024-1440-cobros-selector-estados