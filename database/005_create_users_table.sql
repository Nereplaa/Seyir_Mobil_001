-- ============================================================
-- Seyir Mobil - Auth (login/session/token) altyapisi: Users tablosu
-- Bireysel hesap + rol modeli (Admin / Viewer). Ilk admin kullanicisi
-- backend startup'inda (Program.cs) kod tarafinda seed edilir - sifre
-- hash'i BCrypt ile uretildigi icin SQL'de elle yazilmiyor.
-- ============================================================

USE SeyirMobilDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Username        VARCHAR(50)     NOT NULL UNIQUE,
        PasswordHash    VARCHAR(200)    NOT NULL,
        Role            VARCHAR(20)     NOT NULL,  -- 'Admin' veya 'Viewer'
        OlusturmaTarihi DATETIME        NOT NULL DEFAULT GETDATE()
    );
END
GO
