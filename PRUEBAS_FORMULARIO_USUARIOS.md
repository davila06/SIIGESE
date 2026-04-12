# ðŸ§ª Pruebas del Formulario de Usuarios - VerificaciÃ³n de Estado VacÃ­o

## ðŸ“‹ Lista de VerificaciÃ³n

### âœ… **Primer Ingreso - Formulario VacÃ­o**
- [ ] Al cargar la pÃ¡gina, el formulario debe estar completamente vacÃ­o
- [ ] No debe mostrar errores de validaciÃ³n inicialmente
- [ ] Todos los campos deben estar en estado "pristine" y "untouched"
- [ ] El botÃ³n "Crear Usuario" debe estar habilitado solo cuando el formulario sea vÃ¡lido

### âœ… **BotÃ³n "Nuevo Usuario"**
- [ ] Al hacer clic en "Nuevo Usuario", debe limpiar el formulario completamente
- [ ] Debe hacer scroll al formulario
- [ ] Debe cambiar el tÃ­tulo a "Crear Nuevo Usuario"
- [ ] Debe mostrar el botÃ³n "Limpiar Formulario"

### âœ… **Modo EdiciÃ³n**
- [ ] Al hacer clic en "Editar" un usuario, debe cargar los datos correctamente
- [ ] Debe cambiar el tÃ­tulo a "Editar Usuario"
- [ ] Debe remover los validadores de contraseÃ±a
- [ ] Debe mostrar el botÃ³n "Cancelar EdiciÃ³n"

### âœ… **Cancelar EdiciÃ³n**
- [ ] Al hacer clic en "Cancelar EdiciÃ³n", debe limpiar el formulario
- [ ] Debe restaurar todos los validadores de contraseÃ±a
- [ ] Debe cambiar de vuelta al modo "Crear Usuario"
- [ ] Debe mostrar el botÃ³n "Limpiar Formulario" en lugar de "Cancelar EdiciÃ³n"

### âœ… **DespuÃ©s de Crear Usuario**
- [ ] DespuÃ©s de crear exitosamente, debe resetear el formulario
- [ ] Debe mostrar mensaje de Ã©xito
- [ ] El formulario debe quedar limpio para el siguiente usuario

### âœ… **DespuÃ©s de Actualizar Usuario**
- [ ] DespuÃ©s de actualizar exitosamente, debe resetear el formulario
- [ ] Debe mostrar mensaje de Ã©xito
- [ ] Debe salir del modo ediciÃ³n

### âœ… **Limpiar Formulario Manual**
- [ ] El botÃ³n "Limpiar Formulario" debe estar visible en modo creaciÃ³n
- [ ] Debe limpiar todos los campos al hacer clic
- [ ] Debe limpiar todos los estados de validaciÃ³n

## ðŸ” **Pasos de Prueba Detallados**

### **Prueba 1: Carga Inicial**
1. Abrir http://localhost:4200
2. Hacer login como administrador
3. Ir a "Usuarios"
4. **Verificar:** Formulario completamente vacÃ­o
5. **Verificar:** No hay errores mostrados
6. **Verificar:** TÃ­tulo dice "Crear Nuevo Usuario"

### **Prueba 2: BotÃ³n Nuevo Usuario**
1. Si hay datos en el formulario, hacer clic en "Nuevo Usuario"
2. **Verificar:** Formulario se limpia completamente
3. **Verificar:** Scroll automÃ¡tico al formulario
4. **Verificar:** BotÃ³n "Limpiar Formulario" visible

### **Prueba 3: Crear Usuario**
1. Llenar todos los campos requeridos:
   - Nombre de Usuario: `test_user`
   - Email: `test@example.com`
   - Nombre: `Test`
   - Apellido: `User`
   - ContraseÃ±a: `123456`
   - Confirmar ContraseÃ±a: `123456`
   - Roles: Seleccionar al menos uno
   - Activo: Marcado
2. Hacer clic en "Crear Usuario"
3. **Verificar:** Mensaje "Usuario creado exitosamente"
4. **Verificar:** Formulario se resetea automÃ¡ticamente
5. **Verificar:** Todos los campos estÃ¡n vacÃ­os

### **Prueba 4: Editar Usuario**
1. En la tabla, hacer clic en "Editar" de cualquier usuario
2. **Verificar:** Datos del usuario cargan en el formulario
3. **Verificar:** TÃ­tulo cambia a "Editar Usuario"
4. **Verificar:** Campos de contraseÃ±a no son requeridos
5. **Verificar:** BotÃ³n "Cancelar EdiciÃ³n" visible

### **Prueba 5: Cancelar EdiciÃ³n**
1. Con un usuario en ediciÃ³n, hacer clic en "Cancelar EdiciÃ³n"
2. **Verificar:** Formulario se limpia completamente
3. **Verificar:** TÃ­tulo cambia a "Crear Nuevo Usuario"
4. **Verificar:** Validadores de contraseÃ±a restaurados
5. **Verificar:** BotÃ³n "Limpiar Formulario" visible

### **Prueba 6: Actualizar Usuario**
1. Editar un usuario existente
2. Cambiar algunos datos (ej: nombre, email)
3. Hacer clic en "Actualizar Usuario"
4. **Verificar:** Mensaje "Usuario actualizado exitosamente"
5. **Verificar:** Formulario se resetea automÃ¡ticamente
6. **Verificar:** Sale del modo ediciÃ³n

### **Prueba 7: Limpiar Manual**
1. En modo creaciÃ³n, llenar algunos campos
2. Hacer clic en "Limpiar Formulario"
3. **Verificar:** Todos los campos se limpian
4. **Verificar:** No hay errores de validaciÃ³n mostrados
5. **Verificar:** Estado pristine/untouched restaurado

## ðŸŽ¯ **Comportamientos Esperados**

### **Estados del Formulario:**
- **Pristine:** No modificado por el usuario
- **Untouched:** No ha sido enfocado/tocado
- **Valid/Invalid:** Estado de validaciÃ³n correcto
- **Clean:** Sin errores visuales mostrados

### **Botones Contextuales:**
- **Modo CreaciÃ³n:** "Crear Usuario" + "Limpiar Formulario"
- **Modo EdiciÃ³n:** "Actualizar Usuario" + "Cancelar EdiciÃ³n"

### **Validaciones:**
- **Modo CreaciÃ³n:** Todos los campos + contraseÃ±as requeridas
- **Modo EdiciÃ³n:** Todos los campos excepto contraseÃ±as

### **Transiciones:**
- **Crear â†’ DespuÃ©s de crear:** Formulario limpio automÃ¡tico
- **Editar â†’ Cancelar:** Formulario limpio + validadores restaurados
- **Editar â†’ Actualizar:** Formulario limpio + modo creaciÃ³n

## ðŸš¨ **Errores Comunes a Verificar**

âŒ **NO debe ocurrir:**
- Datos residuales en formulario despuÃ©s de crear/actualizar
- Errores de validaciÃ³n mostrados en formulario limpio
- Validadores incorrectos despuÃ©s de cancelar ediciÃ³n
- Botones incorrectos mostrados segÃºn el modo
- TÃ­tulos incorrectos segÃºn el contexto

âœ… **DEBE ocurrir:**
- Formulario completamente limpio en carga inicial
- Reseteo automÃ¡tico despuÃ©s de operaciones exitosas
- Transiciones correctas entre modos
- Validaciones apropiadas segÃºn el contexto
- Estados de formulario limpios (pristine/untouched)

---
**ðŸ•’ Fecha:** Octubre 23, 2025  
**ðŸ”§ Sistema:** OmnIA v1.0  
**ðŸ“± URL:** http://localhost:4200/usuarios
