-- ============================================================
-- Seyir Mobil - Adım: Vehicles Tablosunu Kaldır
-- Vehicles, projenin ilk/alıştırma tablosuydu (001/002). Kurumdan gelen asıl
-- gereksinim (CLAUDE.md §1.1) AracHareketleri'ne geçince hiçbir istemci
-- (masaüstü/web) Vehicles'ı kullanmaz oldu - artık backend'den de kaldırılıyor.
-- 001/002 dosyaları geçmiş kaydı olarak silinmedi, sadece bu script'le geri alınıyor.
-- ============================================================

USE SeyirMobilDb;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    DROP TABLE Vehicles;
END
GO
