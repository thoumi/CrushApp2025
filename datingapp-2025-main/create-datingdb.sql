-- Script pour créer la base de données datingdb
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'datingdb')
BEGIN
    CREATE DATABASE datingdb;
    PRINT 'Database datingdb created successfully';
END
ELSE
BEGIN
    PRINT 'Database datingdb already exists';
END
GO
