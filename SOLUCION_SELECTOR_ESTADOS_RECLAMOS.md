# Solución: Selector de Estados para Reclamos

## Problema
El usuario solicitó que en el módulo de reclamos aparezca un selector para seleccionar el estado al cual se va a cambiar, en lugar del cambio automático secuencial que existía anteriormente.

## Solución Implementada
Se implementó un selector interactivo de estados para reclamos que permite al usuario elegir específicamente el estado deseado.

### Cambios Realizados

#### 1. Modificación en `reclamos-dashboard.component.ts`
```typescript
cambiarEstado(reclamo: Reclamo): void {
  console.log('Cambiar estado:', reclamo);
  
  // Crear una lista de estados disponibles con íconos
  const estadosDisponibles = [
    { value: EstadoReclamo.Abierto, label: 'Abierto', icon: '📂' },
    { value: EstadoReclamo.EnProceso, label: 'En Proceso', icon: '⚙️' },
    { value: EstadoReclamo.Resuelto, label: 'Resuelto', icon: '✅' },
    { value: EstadoReclamo.Cerrado, label: 'Cerrado', icon: '🔒' },
    { value: EstadoReclamo.Rechazado, label: 'Rechazado', icon: '❌' },
    { value: EstadoReclamo.Escalado, label: 'Escalado', icon: '⬆️' }
  ];

  // Filtrar el estado actual para no mostrar la misma opción
  const estadosParaCambio = estadosDisponibles.filter(e => e.value !== reclamo.estado);

  if (estadosParaCambio.length === 0) {
    this.showMessage('No hay estados disponibles para cambiar');
    return;
  }

  // Crear mensaje con opciones numeradas
  let mensaje = `Seleccione el nuevo estado para el reclamo ${reclamo.numeroReclamo}:\n\n`;
  
  estadosParaCambio.forEach((estado, index) => {
    mensaje += `${index + 1}. ${estado.label} ${estado.icon}\n`;
  });

  const seleccion = prompt(mensaje + '\nIngrese el número del estado deseado:');
  
  if (seleccion) {
    const indice = parseInt(seleccion) - 1;
    
    if (indice >= 0 && indice < estadosParaCambio.length) {
      const nuevoEstado = estadosParaCambio[indice];
      
      if (confirm(`¿Está seguro de cambiar el estado del reclamo ${reclamo.numeroReclamo} a "${nuevoEstado.label}"?`)) {
        this.reclamosService.cambiarEstado(reclamo.id, nuevoEstado.value, `Estado cambiado a ${nuevoEstado.label}`).subscribe({
          next: (response) => {
            this.showMessage(`Estado del reclamo cambiado a "${nuevoEstado.label}" exitosamente`);
            this.loadReclamos(); // Recargar la lista
          },
          error: (error) => {
            console.error('Error cambiando estado:', error);
            this.showMessage('Error al cambiar el estado del reclamo');
          }
        });
      }
    } else {
      this.showMessage('Opción inválida');
    }
  }
}
```

### Funcionalidades Implementadas

#### 1. **Selector Interactivo**
- Lista numerada de estados disponibles
- Íconos visuales para cada estado
- Entrada por teclado del número deseado

#### 2. **Estados Disponibles**
- **Abierto** 📂 (EstadoReclamo.Abierto = 0)
- **En Proceso** ⚙️ (EstadoReclamo.EnProceso = 1)
- **Resuelto** ✅ (EstadoReclamo.Resuelto = 2)
- **Cerrado** 🔒 (EstadoReclamo.Cerrado = 3)
- **Rechazado** ❌ (EstadoReclamo.Rechazado = 4)
- **Escalado** ⬆️ (EstadoReclamo.Escalado = 5)

#### 3. **Validaciones**
- Excluye el estado actual del reclamo
- Valida que la selección sea un número válido
- Confirma el cambio antes de ejecutarlo

#### 4. **Integración con Backend**
- Utiliza el servicio `reclamosService.cambiarEstado()`
- Actualiza la lista automáticamente tras el cambio
- Maneja errores y muestra mensajes apropiados

### Soporte del Mock API Interceptor

El Mock API Interceptor ya tenía soporte completo para cambio de estados de reclamos:

- **Endpoint**: `PUT /api/reclamos/{id}/estado`
- **Método**: `handleChangeEstadoReclamo()`
- **Respuesta**: Actualiza estado, timestamps y observaciones

### Experiencia de Usuario

1. **Clic en "Cambiar Estado"**: El usuario hace clic en el botón de cambiar estado
2. **Selector Interactivo**: Aparece un prompt con opciones numeradas y íconos
3. **Selección**: El usuario ingresa el número del estado deseado
4. **Confirmación**: Se solicita confirmación del cambio
5. **Actualización**: La lista se actualiza automáticamente

### Estados del Reclamo

```typescript
export enum EstadoReclamo {
  Abierto = 0,      // 📂 Estado inicial
  EnProceso = 1,    // ⚙️ En trabajando
  Resuelto = 2,     // ✅ Solucionado
  Cerrado = 3,      // 🔒 Finalizado
  Rechazado = 4,    // ❌ No procede
  Escalado = 5      // ⬆️ Nivel superior
}
```

### Resultado
✅ **Implementación Exitosa**: 
- Selector de estados funcional e intuitivo
- Consistencia con el patrón implementado en cobros
- UX mejorada con control total del usuario
- Validación completa y manejo de errores

### Testing
- ✅ Compilación exitosa
- ✅ Despliegue a Azure Static Web Apps
- ✅ Funcionalidad disponible en: https://gentle-dune-0a2edab0f-preview.eastus2.3.azurestaticapps.net/reclamos

### Notas Técnicas
- Implementación idéntica al patrón de cobros
- Reutiliza infraestructura existente del Mock API
- Mantiene la integridad de datos y timestamps
- Logging completo para debugging