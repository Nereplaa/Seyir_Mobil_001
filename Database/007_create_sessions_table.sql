-- ============================================================
-- Seyir Mobil - Oturum (session) kaliciligi: Sessions tablosu
-- SessionStore.cs'nin eski in-memory (ConcurrentDictionary) halinin yerini alir -
-- artik oturumlar SQL Server'da tutuluyor, backend restart olsa bile
-- (JWT hala gecerliyse) kullanicilar oturumdan atilmiyor.
-- ============================================================

USE SeyirMobilDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
BEGIN
    CREATE TABLE Sessions (
        Id              VARCHAR(32)  NOT NULL PRIMARY KEY,  -- Guid.NewGuid("N") formati
        UserId          INT          NOT NULL REFERENCES Users(Id),
        SonIslemZamani  DATETIME     NOT NULL,
        OlusturmaTarihi DATETIME     NOT NULL DEFAULT GETDATE()
    );
END
GO
