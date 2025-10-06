-- Script directo para actualizar nombres específicos usando UPDATE con WHERE exacto

-- Verificar datos actuales
SELECT TOP 5 Id, clienteNombre, clienteApellido FROM Cobros;

-- Actualizar registro por registro usando IDs específicos
-- Primero obtenemos los IDs de los registros problemáticos
UPDATE Cobros SET clienteNombre = N'María' WHERE Id = 1;
UPDATE Cobros SET clienteApellido = N'González' WHERE Id = 1;

UPDATE Cobros SET clienteApellido = N'Rodríguez' WHERE Id = 2;

UPDATE Cobros SET clienteApellido = N'Jiménez' WHERE Id = 3;

-- Verificar cambios
SELECT TOP 5 Id, clienteNombre, clienteApellido FROM Cobros;