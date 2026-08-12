-- ============================================================
-- Seyir Mobil - Adım 2: Vehicles Tablosuna Dummy (Test) Veri
-- ============================================================

USE SeyirMobilDb;
GO

IF NOT EXISTS (SELECT 1 FROM Vehicles)
BEGIN
    INSERT INTO Vehicles (plaka, totalkm, kayittrh) VALUES
    ('41SM001', 145500.50, '2026-01-15 09:30:00'),
    ('34AB123', 98230.00,  '2026-02-20 14:10:00'),
    ('06CD456', 210450.75, '2025-11-05 08:00:00'),
    ('35EF789', 45000.00,  '2026-06-01 10:00:00'),
    ('16GH321', 178320.25, '2026-03-12 16:45:00');
END
GO
