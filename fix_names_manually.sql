-- Script para corregir nombres específicos con caracteres mal codificados

-- Actualizar MarÃ­a a María
UPDATE Cobros 
SET clienteNombre = 'María'
WHERE clienteNombre = 'MarÃ­a';

-- Actualizar GonzÃ¡lez a González  
UPDATE Cobros 
SET clienteApellido = 'González'
WHERE clienteApellido = 'GonzÃ¡lez';

-- Actualizar RodrÃ­guez a Rodríguez
UPDATE Cobros 
SET clienteApellido = 'Rodríguez'
WHERE clienteApellido = 'RodrÃ­guez';

-- Actualizar JimÃ©nez a Jiménez
UPDATE Cobros 
SET clienteApellido = 'Jiménez'
WHERE clienteApellido = 'JimÃ©nez';

-- Verificar los cambios
SELECT clienteNombre, clienteApellido FROM Cobros;