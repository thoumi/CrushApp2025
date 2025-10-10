-- Script d'initialisation de la base de données
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CrushAppDb')
BEGIN
    CREATE DATABASE CrushAppDb;
    PRINT 'Database CrushAppDb created successfully';
END
ELSE
BEGIN
    PRINT 'Database CrushAppDb already exists';
END
GO

