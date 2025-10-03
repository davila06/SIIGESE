-- =============================================
-- Script para crear la base de datos SINSEG
-- SQL Server Express
-- Fecha: 2025-10-01
-- =============================================

USE master;
GO

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SinsegAppDb')
BEGIN
    CREATE DATABASE SinsegAppDb;
    PRINT 'Base de datos SinsegAppDb creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La base de datos SinsegAppDb ya existe.';
END
GO

-- Usar la base de datos
USE SinsegAppDb;
GO

-- Crear tabla de control de migraciones de Entity Framework
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory' AND xtype='U')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

PRINT 'Iniciando creación de tablas...';
GO