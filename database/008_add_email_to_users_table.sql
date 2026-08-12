-- ============================================================
-- Seyir Mobil - Users tablosuna Email alani eklenmesi
-- Eren bey geri bildirimi (feedback_001, 2026-08-11): "Sifremi Unuttum" +
-- e-posta ile sifre sifirlama akisinin on kosulu - kullanicinin sifirlama
-- baglantisinin gonderilecegi bir e-posta adresi olmasi gerekiyor.
--
-- BILINCLI OLARAK NULL'A IZIN VERILIYOR (NOT NULL DEGIL): mevcut kullanicilarda
-- (admin, alperen, viewer test hesaplari vb.) email yok, geriye donuk zorunlu
-- yapmak onlari bozardi. Yeni kullanicilar icin backend (Program.cs, POST
-- /api/users) email'i ZORUNLU kiliyor - kisitlama kod tarafinda, semada degil.
-- ============================================================

USE SeyirMobilDb;
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('Users') AND name = 'Email'
)
BEGIN
    ALTER TABLE Users ADD Email VARCHAR(200) NULL;
END
GO
