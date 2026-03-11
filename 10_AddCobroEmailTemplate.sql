-- Migration 10: Add cobro email template columns to EmailConfigs table
-- Run this script against the production Azure SQL database

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'EmailConfigs' AND COLUMN_NAME = 'CobroEmailSubject'
)
BEGIN
    ALTER TABLE EmailConfigs
    ADD CobroEmailSubject NVARCHAR(500) NULL;

    PRINT 'Column CobroEmailSubject added to EmailConfigs.';
END
ELSE
BEGIN
    PRINT 'Column CobroEmailSubject already exists in EmailConfigs.';
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'EmailConfigs' AND COLUMN_NAME = 'CobroEmailBody'
)
BEGIN
    ALTER TABLE EmailConfigs
    ADD CobroEmailBody NVARCHAR(MAX) NULL;

    PRINT 'Column CobroEmailBody added to EmailConfigs.';
END
ELSE
BEGIN
    PRINT 'Column CobroEmailBody already exists in EmailConfigs.';
END
