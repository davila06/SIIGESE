-- Script para corregir la codificación de caracteres en nombres de clientes
-- Este script corrige los caracteres mal codificados en la tabla Cobros

UPDATE Cobros 
SET 
    clienteNombre = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
        clienteNombre,
        'Ã¡', 'á'),
        'Ã©', 'é'),
        'Ã­', 'í'),
        'Ã³', 'ó'),
        'Ãº', 'ú'),
        'Ã±', 'ñ'),
        'Ã', 'Á'),
        'Ã‰', 'É'),
    clienteApellido = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
        clienteApellido,
        'Ã¡', 'á'),
        'Ã©', 'é'),
        'Ã­', 'í'),
        'Ã³', 'ó'),
        'Ãº', 'ú'),
        'Ã±', 'ñ'),
        'Ã', 'Á'),
        'Ã‰', 'É')
WHERE 
    clienteNombre LIKE '%Ã%' OR clienteApellido LIKE '%Ã%';

-- Verificar los cambios
SELECT clienteNombre, clienteApellido FROM Cobros;