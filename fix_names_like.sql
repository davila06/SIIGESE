-- Script para corregir nombres usando LIKE patterns

-- Actualizar MarÃ­a a María
UPDATE Cobros 
SET clienteNombre = 'María'
WHERE clienteNombre LIKE '%Mar%a%';

-- Actualizar GonzÃ¡lez a González  
UPDATE Cobros 
SET clienteApellido = 'González'
WHERE clienteApellido LIKE '%Gonz%lez%';

-- Actualizar RodrÃ­guez a Rodríguez
UPDATE Cobros 
SET clienteApellido = 'Rodríguez'
WHERE clienteApellido LIKE '%Rodr%guez%';

-- Actualizar JimÃ©nez a Jiménez
UPDATE Cobros 
SET clienteApellido = 'Jiménez'
WHERE clienteApellido LIKE '%Jim%nez%';

-- Verificar los cambios
SELECT clienteNombre, clienteApellido FROM Cobros;