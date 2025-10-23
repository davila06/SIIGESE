# 🧪 Pruebas del Formulario de Usuarios - Verificación de Estado Vacío

## 📋 Lista de Verificación

### ✅ **Primer Ingreso - Formulario Vacío**
- [ ] Al cargar la página, el formulario debe estar completamente vacío
- [ ] No debe mostrar errores de validación inicialmente
- [ ] Todos los campos deben estar en estado "pristine" y "untouched"
- [ ] El botón "Crear Usuario" debe estar habilitado solo cuando el formulario sea válido

### ✅ **Botón "Nuevo Usuario"**
- [ ] Al hacer clic en "Nuevo Usuario", debe limpiar el formulario completamente
- [ ] Debe hacer scroll al formulario
- [ ] Debe cambiar el título a "Crear Nuevo Usuario"
- [ ] Debe mostrar el botón "Limpiar Formulario"

### ✅ **Modo Edición**
- [ ] Al hacer clic en "Editar" un usuario, debe cargar los datos correctamente
- [ ] Debe cambiar el título a "Editar Usuario"
- [ ] Debe remover los validadores de contraseña
- [ ] Debe mostrar el botón "Cancelar Edición"

### ✅ **Cancelar Edición**
- [ ] Al hacer clic en "Cancelar Edición", debe limpiar el formulario
- [ ] Debe restaurar todos los validadores de contraseña
- [ ] Debe cambiar de vuelta al modo "Crear Usuario"
- [ ] Debe mostrar el botón "Limpiar Formulario" en lugar de "Cancelar Edición"

### ✅ **Después de Crear Usuario**
- [ ] Después de crear exitosamente, debe resetear el formulario
- [ ] Debe mostrar mensaje de éxito
- [ ] El formulario debe quedar limpio para el siguiente usuario

### ✅ **Después de Actualizar Usuario**
- [ ] Después de actualizar exitosamente, debe resetear el formulario
- [ ] Debe mostrar mensaje de éxito
- [ ] Debe salir del modo edición

### ✅ **Limpiar Formulario Manual**
- [ ] El botón "Limpiar Formulario" debe estar visible en modo creación
- [ ] Debe limpiar todos los campos al hacer clic
- [ ] Debe limpiar todos los estados de validación

## 🔍 **Pasos de Prueba Detallados**

### **Prueba 1: Carga Inicial**
1. Abrir http://localhost:4200
2. Hacer login como administrador
3. Ir a "Usuarios"
4. **Verificar:** Formulario completamente vacío
5. **Verificar:** No hay errores mostrados
6. **Verificar:** Título dice "Crear Nuevo Usuario"

### **Prueba 2: Botón Nuevo Usuario**
1. Si hay datos en el formulario, hacer clic en "Nuevo Usuario"
2. **Verificar:** Formulario se limpia completamente
3. **Verificar:** Scroll automático al formulario
4. **Verificar:** Botón "Limpiar Formulario" visible

### **Prueba 3: Crear Usuario**
1. Llenar todos los campos requeridos:
   - Nombre de Usuario: `test_user`
   - Email: `test@example.com`
   - Nombre: `Test`
   - Apellido: `User`
   - Contraseña: `123456`
   - Confirmar Contraseña: `123456`
   - Roles: Seleccionar al menos uno
   - Activo: Marcado
2. Hacer clic en "Crear Usuario"
3. **Verificar:** Mensaje "Usuario creado exitosamente"
4. **Verificar:** Formulario se resetea automáticamente
5. **Verificar:** Todos los campos están vacíos

### **Prueba 4: Editar Usuario**
1. En la tabla, hacer clic en "Editar" de cualquier usuario
2. **Verificar:** Datos del usuario cargan en el formulario
3. **Verificar:** Título cambia a "Editar Usuario"
4. **Verificar:** Campos de contraseña no son requeridos
5. **Verificar:** Botón "Cancelar Edición" visible

### **Prueba 5: Cancelar Edición**
1. Con un usuario en edición, hacer clic en "Cancelar Edición"
2. **Verificar:** Formulario se limpia completamente
3. **Verificar:** Título cambia a "Crear Nuevo Usuario"
4. **Verificar:** Validadores de contraseña restaurados
5. **Verificar:** Botón "Limpiar Formulario" visible

### **Prueba 6: Actualizar Usuario**
1. Editar un usuario existente
2. Cambiar algunos datos (ej: nombre, email)
3. Hacer clic en "Actualizar Usuario"
4. **Verificar:** Mensaje "Usuario actualizado exitosamente"
5. **Verificar:** Formulario se resetea automáticamente
6. **Verificar:** Sale del modo edición

### **Prueba 7: Limpiar Manual**
1. En modo creación, llenar algunos campos
2. Hacer clic en "Limpiar Formulario"
3. **Verificar:** Todos los campos se limpian
4. **Verificar:** No hay errores de validación mostrados
5. **Verificar:** Estado pristine/untouched restaurado

## 🎯 **Comportamientos Esperados**

### **Estados del Formulario:**
- **Pristine:** No modificado por el usuario
- **Untouched:** No ha sido enfocado/tocado
- **Valid/Invalid:** Estado de validación correcto
- **Clean:** Sin errores visuales mostrados

### **Botones Contextuales:**
- **Modo Creación:** "Crear Usuario" + "Limpiar Formulario"
- **Modo Edición:** "Actualizar Usuario" + "Cancelar Edición"

### **Validaciones:**
- **Modo Creación:** Todos los campos + contraseñas requeridas
- **Modo Edición:** Todos los campos excepto contraseñas

### **Transiciones:**
- **Crear → Después de crear:** Formulario limpio automático
- **Editar → Cancelar:** Formulario limpio + validadores restaurados
- **Editar → Actualizar:** Formulario limpio + modo creación

## 🚨 **Errores Comunes a Verificar**

❌ **NO debe ocurrir:**
- Datos residuales en formulario después de crear/actualizar
- Errores de validación mostrados en formulario limpio
- Validadores incorrectos después de cancelar edición
- Botones incorrectos mostrados según el modo
- Títulos incorrectos según el contexto

✅ **DEBE ocurrir:**
- Formulario completamente limpio en carga inicial
- Reseteo automático después de operaciones exitosas
- Transiciones correctas entre modos
- Validaciones apropiadas según el contexto
- Estados de formulario limpios (pristine/untouched)

---
**🕒 Fecha:** Octubre 23, 2025  
**🔧 Sistema:** SIIGESE v1.0  
**📱 URL:** http://localhost:4200/usuarios