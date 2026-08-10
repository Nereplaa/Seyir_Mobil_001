# 🚚 Seyir Mobil — Araç Takip Sistemi

Araç kilometre, plaka ve kayıt durumlarını takip eden, ölçeklenebilir bir sistem. Seyir Mobil
bünyesinde staj kapsamında geliştirilmektedir.

### 📅 [Proje Timeline'ı → TIMELINE.md](TIMELINE.md)
Projenin nasıl geliştiğini, hangi kararların neden alındığını, tarih/saat bilgisiyle adım adım
görmek için tıklayın.

---

## Hakkında

Sistem ilk aşamada yerel bir veritabanı + masaüstü arayüzü olarak başlıyor, ilerleyen süreçte
kurumdan alınacak geri bildirimler doğrultusunda web ortamına taşınarak çoklu platform destekli
bir yapıya evrilecek.

## Mimari

```
        SQL Server (DB)
             ↑
   ASP.NET Core Web API   ← tüm iş mantığı, tüm veritabanı erişimi burada
             ↑  (HTTP/JSON)
   ┌─────────┼──────────────┐
Masaüstü    Web            (ileride: Mobil)
(WinForms)  (Angular)
```

Hiçbir istemci (masaüstü, web, ileride mobil) veritabanına doğrudan bağlanmaz — sadece backend API
SQL Server'a erişir. İstemciler API'yi HTTP ile çağırır.

## Teknoloji

| Katman | Teknoloji |
|---|---|
| Veritabanı | SQL Server (Docker container) |
| Backend / API | ASP.NET Core Web API (.NET 10) |
| ORM | Entity Framework Core |
| Masaüstü istemci | WinForms (C#) |
| Web istemci | Angular 22 (SPA) |
| Auth | JWT + BCrypt, sliding idle-timeout, oturumlar SQL Server'da kalıcı |
| UI komponentleri | DevExtreme (Angular entegrasyonu) |
| Bildirim/uyarı pencereleri | SweetAlert2 |
| Excel export | ClosedXML (backend, masaüstü) / DevExtreme'in kendi export'u (web) |
| Merkezi log takibi | Graylog + MongoDB + OpenSearch, backend'de Serilog |
| Dağıtım / geliştirme ortamı | Docker Compose |

## Nasıl Çalıştırılır

Tüm sistem (veritabanı, backend, web, log altyapısı) tek bir komutla ayağa kalkar:

```
docker compose up -d
```

- Web: http://localhost:4200
- Backend API (Swagger): http://localhost:5080/swagger
- Log arayüzü (Graylog): http://localhost:9000 (`admin` / `Admin123!`)

İlk çalıştırmada veritabanı şemasının `database/` altındaki script'lerle (001'den başlayarak
sırayla, `sqlserver` container'ına) kurulması, ayrıca Graylog'da bir GELF UDP input'unun
(port 12201, Graylog arayüzünden veya `POST /api/system/inputs` ile) oluşturulması gerekir.
Masaüstü istemci (WinForms) Docker'a dahil değildir, ayrıca `desktop/SeyirMobil.Desktop`
üzerinden çalıştırılır.

## Güncel Durum

- ✅ Veritabanı kuruldu, temel + asıl görev tabloları oluşturuldu ve test verisiyle dolduruldu.
- ✅ .NET 10 SDK kuruldu.
- ✅ Backend API çalışıyor, Swagger ile test edildi.
- ✅ Kurumdan resmi gereksinim dokümanı geldi — araç hareket/kilometre raporu özelliği.
- ✅ WinForms masaüstü uygulaması: tüm araç hareketlerini listeliyor, çoklu plaka + tarih aralığı
  seçerek "başlangıç km / bitiş km / yapılan km" raporu oluşturuyor. Arayüz responsive — pencere
  daraltılınca kontroller alt satıra kayıyor, kaybolmuyor.
- ✅ Yeni araç hareketi ekleme (adım adım: plaka → tarih → hız → km sayacı — km sayacı, komşu
  kayıtlara göre tutarlı kalacak şekilde otomatik sınırlanıyor) ve kayıt silme.
- ✅ Ana ekranda plaka/tarih/hız/km'ye göre filtreleme.
- ✅ İlk demo Seyir Mobil'e yapıldı, geri bildirim doğrultusunda yol haritası netleşti: sırada
  detaylı rapor (masaüstü) → web uygulaması (temel altyapı) → arayüz geliştirme → Docker.
- ✅ Rapor ekranına "Detaylı Rapor (gün gün)" modu eklendi — seçilen tarih aralığındaki her okumanın
  bir öncekine göre kilometre artışını gösteriyor.
- ✅ Web uygulaması (Angular) yayında — masaüstündeki tüm özellikler (liste/filtre, ekleme/silme,
  rapor özet+detaylı) aynı backend API üzerinden web'de de çalışıyor. Görsel/arayüz geliştirmesi
  bir sonraki aşamada.
- ✅ Araç hareketleri listesine sayfalama eklendi (sayfa başına kayıt sayısı seçilebiliyor), hem
  web hem masaüstünde. Liste ve rapor ekranlarına Excel'e aktarma eklendi.
- ✅ Giriş (login) sistemi: JWT + rol bazlı yetkilendirme (Admin/Viewer), hem web hem masaüstünde.
  Oturumlar artık SQL Server'da kalıcı — sunucu yeniden başlasa bile kullanıcılar oturumdan atılmıyor.
- ✅ Web arayüzü tamamen DevExtreme bileşenlerine geçirildi (grid, tarih seçici, form alanları) ve
  bildirim/uyarı pencereleri SweetAlert2 ile yenilendi.
- ✅ Yönetici Paneli: kullanıcı hesapları buradan görüntülenip eklenip silinebiliyor, roller
  genişleyebilecek bir yapıda yönetiliyor.
- ✅ Tüm sistem Docker Compose ile konteynerleştirildi, merkezi log takibi (Graylog) eklendi —
  backend API çağrıları ve web'deki tüm etkileşimler kaydediliyor.
- ✅ Excel'den toplu veri girişi (hem web hem masaüstü) — dosya yüklenip doğrulanıyor, hatalı
  satırlar düzenlenebiliyor, çakışan kayıtlar için kullanıcı karar veriyor.

Detaylı ilerleme için bkz. [TIMELINE.md](TIMELINE.md).

## Proje Yapısı

Backend, masaüstü, web ve veritabanı katmanları en üst seviyede net şekilde ayrılmıştır:

```
├── database/                    ← SQL script'leri (sıralı, ör. 001_..., 002_...)
│   ├── 001_create_vehicles_table.sql        (tarihsel — 006 ile tablo kaldırıldı)
│   ├── 002_seed_dummy_data.sql              (tarihsel — 006 ile tablo kaldırıldı)
│   ├── 003_create_arac_hareketleri_table.sql
│   ├── 004_seed_arac_hareketleri_dummy_data.sql
│   ├── 005_create_users_table.sql
│   ├── 006_drop_vehicles_table.sql
│   └── 007_create_sessions_table.sql
├── backend/
│   └── SeyirMobil.Api/          ← ASP.NET Core Web API + EF Core (tüm iş mantığı, DB erişimi)
│       └── Dockerfile
├── desktop/
│   └── SeyirMobil.Desktop/      ← WinForms masaüstü istemcisi (araç hareketleri listesi + rapor)
├── web/
│   ├── src/                     ← Angular web istemcisi kaynak kodu (aynı backend API'yi kullanır)
│   └── Dockerfile
├── docker-compose.yml            ← tüm sistemi (DB + backend + web + log altyapısı) ayağa kaldırır
├── SeyirMobil.slnx               ← .NET solution (backend + desktop projelerini kapsar)
├── README.md
└── TIMELINE.md
```

## Geliştirici

Alperen Yağmur — Kocaeli Sağlık ve Teknoloji Üniversitesi, Yazılım Mühendisliği — Seyir Mobil
stajyeri.
