-- ============================================================
-- Seyir Mobil - Adım 1: Veritabanı ve Vehicles Tablosu (V1)
-- Şema referansı: CLAUDE.md §4 (PDF gelene kadar başlangıç iskeleti, nihai değil)
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SeyirMobilDb')
BEGIN
    CREATE DATABASE SeyirMobilDb;
END
GO

USE SeyirMobilDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    CREATE TABLE Vehicles (
        aracid    INT IDENTITY(1,1) PRIMARY KEY,
        plaka     VARCHAR(15)   NOT NULL,
        totalkm   DECIMAL(10,2) NOT NULL DEFAULT 0,
        kayittrh  DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO
