-- ============================================================
-- Seyir Mobil - Users tablosuna sifre sifirlama token alanlari eklenmesi
-- Eren bey geri bildirimi (feedback_001): "Sifremi Unuttum" akisinin ikinci
-- yarisi - kullanici e-posta ile sifirlama istedikten sonra, o istegin
-- gecerliligini (ve son kullanma tarihini) saklayacak bir yer gerekiyor.
--
-- Ayri bir tablo YERINE Users'a iki alan eklendi (bilincli tercih, ayni
-- projede daha once "yeni bagimlilik/tablo yerine mevcut yapiyi kullan"
-- mantigiyla alinan Sessions tablosu kararindaki AYNI dusunce) - bir
-- kullanicinin ayni anda EN FAZLA bir aktif sifirlama istegi olabilir,
-- coklu-istek gecmisi tutmaya gerek yok, ayri bir tablo/join gereksiz
-- karmasiklik olurdu.
-- ============================================================

USE SeyirMobilDb;
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('Users') AND name = 'ResetToken'
)
BEGIN
    ALTER TABLE Users ADD ResetToken VARCHAR(200) NULL;
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('Users') AND name = 'ResetTokenExpiry'
)
BEGIN
    ALTER TABLE Users ADD ResetTokenExpiry DATETIME NULL;
END
GO
