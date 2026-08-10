-- ============================================================
-- Seyir Mobil - Software Developer I Proje: Arac Hareketleri Tablosu
-- Kaynak: kurumdan gelen resmi gereksinim PDF'i (2026-08-03)
-- Kolon adlari PDF'teki orneğe birebir uyacak sekilde birebir korundu.
-- ============================================================

USE SeyirMobilDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AracHareketleri')
BEGIN
    CREATE TABLE AracHareketleri (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        AracId      INT             NOT NULL,
        AracPlaka   VARCHAR(15)     NOT NULL,
        VeriTarihi  DATE            NOT NULL,
        Hiz         INT             NOT NULL,
        KmSayaci    DECIMAL(10,2)   NOT NULL
    );

    CREATE INDEX IX_AracHareketleri_Plaka_Tarih ON AracHareketleri (AracPlaka, VeriTarihi);
END
GO
