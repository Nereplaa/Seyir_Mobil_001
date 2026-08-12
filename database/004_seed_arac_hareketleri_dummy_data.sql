-- ============================================================
-- Seyir Mobil - Arac Hareketleri icin 100 satirlik cesitli dummy veri
-- 10 farkli arac, her biri farkli okuma sikligi/sayisiyla (toplam 100 satir)
-- KmSayaci her aracta zamanla monoton artiyor (gercek kilometre sayaci gibi)
-- ============================================================

USE SeyirMobilDb;
GO

IF NOT EXISTS (SELECT 1 FROM AracHareketleri)
BEGIN
    DECLARE @Vehicles TABLE (AracId INT, AracPlaka VARCHAR(15), BaseKm DECIMAL(10,2), ReadingCount INT, IntervalDays INT, StartDate DATE);
    INSERT INTO @Vehicles (AracId, AracPlaka, BaseKm, ReadingCount, IntervalDays, StartDate) VALUES
        (1,  '34 RN 5944', 88605.94,  10, 1, '2026-07-11'),
        (2,  '41 KT 593',  195309.51,  8, 2, '2026-07-04'),
        (3,  '34 EGB 248', 45309.33,  10, 1, '2026-07-11'),
        (4,  '06 AB 1234', 120450.00, 12, 1, '2026-06-23'),
        (5,  '35 C 4521',  15200.00,   9, 3, '2026-06-18'),
        (6,  '16 DFT 45',  210800.50,  7, 4, '2026-07-08'),
        (7,  '42 M 78901', 62000.00,  11, 2, '2026-06-28'),
        (8,  '07 BC 902',  5400.00,    8, 3, '2026-07-04'),
        (9,  '61 PS 3345', 175600.00, 10, 2, '2026-06-21'),
        (10, '27 T 4456',  33000.00,  15, 1, '2026-07-13');

    DECLARE @AracId INT, @Plaka VARCHAR(15), @Km DECIMAL(10,2), @Count INT, @Interval INT, @Date DATE, @i INT;

    DECLARE vehCursor CURSOR LOCAL FOR
        SELECT AracId, AracPlaka, BaseKm, ReadingCount, IntervalDays, StartDate FROM @Vehicles;

    OPEN vehCursor;
    FETCH NEXT FROM vehCursor INTO @AracId, @Plaka, @Km, @Count, @Interval, @Date;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @i = 0;
        WHILE @i < @Count
        BEGIN
            IF @i > 0
            BEGIN
                -- her okuma arasinda gunluk 50-900 km arasi rastgele artis (gecen gun sayisiyla olcekli)
                SET @Km = @Km + (50 + (ABS(CHECKSUM(NEWID())) % 850)) * @Interval;
                SET @Date = DATEADD(DAY, @Interval, @Date);
            END

            INSERT INTO AracHareketleri (AracId, AracPlaka, VeriTarihi, Hiz, KmSayaci)
            VALUES (@AracId, @Plaka, @Date, 20 + (ABS(CHECKSUM(NEWID())) % 121), @Km);

            SET @i = @i + 1;
        END

        FETCH NEXT FROM vehCursor INTO @AracId, @Plaka, @Km, @Count, @Interval, @Date;
    END

    CLOSE vehCursor;
    DEALLOCATE vehCursor;
END
GO
