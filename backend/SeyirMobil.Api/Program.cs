using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Sinks.Graylog;
using SeyirMobil.Api.Data;
using SeyirMobil.Api.Models;
using SeyirMobil.Api.Services;
using SeyirMobil.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

// Merkezi log toplama (Madde 5, Eren bey 2. geri bildirimi, 2026-08-06): tum API cagrilari
// konsola YANI SIRA Graylog'a da (GELF/UDP, docker-compose'daki "graylog" servisi) gonderiliyor.
// "Graylog:Host" appsettings.json'da yoksa varsayilan "graylog" - docker-compose network'undeki
// servis adi. Graylog konteyneri ayakta degilse (ornegin Docker'siz yerel gelistirme) Serilog'un
// sink'i sessizce basarisiz olur, uygulamayi COKERTMEZ - konsol loglamasi her zaman calisir.
builder.Host.UseSerilog((context, services, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Graylog(new GraylogSinkOptions
    {
        HostnameOrAddress = context.Configuration["Graylog:Host"] ?? "graylog",
        Port = int.Parse(context.Configuration["Graylog:Port"] ?? "12201")
    }));

builder.Services.AddDbContext<SeyirMobilDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SeyirMobilDb")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Swagger UI'da "Authorize" kilidini ve token girişini aktif eder - eklenmeden once
    // korumali endpoint'ler Swagger'dan denenince Authorization header'i hic gitmedigi
    // icin hep 401 donuyordu.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Once /api/auth/login ile token al, sonra buraya SADECE token'in kendisini yapistir (\"Bearer \" onekini SEN eklemene gerek yok, Swagger ekliyor)."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

const string WebClientCorsPolicy = "WebClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(WebClientCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Auth altyapisi (2026-08-04): login/session/token sistemi. Tum arac-hareketleri endpoint'leri
// (2026-08-04 14:31'den itibaren) ve /api/users (kullanici yonetimi) kilitli - sadece
// /api/auth/login acik (aksi halde kimse giris yapamaz).
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtSigningKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
// Scoped (Singleton degil): SessionStore artik Sessions tablosuna yazmak icin
// SeyirMobilDbContext'e ihtiyac duyuyor, DbContext'in kendisi de scoped (istek basina).
builder.Services.AddScoped<SessionStore>();

var app = builder.Build();

// Her istegi (yontem, yol, durum kodu, sure) TEK bir yapilandirilmis log satirinda Graylog'a
// gonderir - "kim, ne zaman, hangi endpoint" sorusunun backend tarafi. Kullanici bilgisi
// (varsa) enrich callback'inde ekleniyor - bu callback istek TAMAMLANDIKTAN sonra calisiyor,
// o noktada UseAuthentication zaten context.User'i doldurmus oluyor.
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("Kullanici", httpContext.User.Identity?.Name ?? "anonim");
        diagnosticContext.Set("Rol", httpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "-");
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(WebClientCorsPolicy);
app.UseAuthentication();

// Sliding idle-timeout (2026-08-04, kullanici istegi): JWT'nin kendi suresi (Jwt:ExpiryMinutes,
// gunluk mutlak ust sinir) DEGISMIYOR, ama her JWT'ye login'de gomulen bir oturum kimligi ("sid"
// claim'i) uzerinden ayrica takip edilen bir "bosta kalma" siniri var (Jwt:IdleTimeoutMinutes).
// Kimlik dogrulanmis HER istek, oturumu SessionStore'da yeniler (DogrulaVeYenile) - bu yuzden
// istemci tarafinda ayri bir "refresh" cagrisina hic gerek yok, normal API kullanimi zaten
// oturumu canli tutuyor. Belirtilen sure boyunca hic istek gelmezse bir sonraki istek 401 doner.
var idleTimeout = TimeSpan.FromMinutes(double.Parse(jwtSection["IdleTimeoutMinutes"]!, CultureInfo.InvariantCulture));
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var sessionStore = context.RequestServices.GetRequiredService<SessionStore>();
        var sessionId = context.User.FindFirstValue("sid");
        if (sessionId is null || !await sessionStore.DogrulaVeYenileAsync(sessionId, idleTimeout))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
    }
    await next();
});

app.UseAuthorization();

// Ilk admin kullanicisini seed et (Users tablosu bossa) - boylece login sistemi
// tazeden kurulan bir veritabaninda da elle SQL yazmadan calisir durumda olur.
using (var seedScope = app.Services.CreateScope())
{
    var seedDb = seedScope.ServiceProvider.GetRequiredService<SeyirMobilDbContext>();
    if (!await seedDb.Users.AnyAsync())
    {
        seedDb.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin"
        });
        await seedDb.SaveChangesAsync();
    }
}

// Tarih aralığı raporu: verilen plaka + [baslangic, bitis] araliginda ilk ve son km sayaci
// okumasi bulunur, farklari "yapilan km" olarak donulur. Filtreleme/siralama EF Core LINQ
// uzerinden SQL Server'a birakiliyor (SELECT TOP 1 ... ORDER BY) - butun okumalari C#'a
// cekip bellekte aramak yerine, sadece 2 satir istemciye donuyor.
app.MapGet("/api/arac-hareketleri/rapor", async (string plaka, DateOnly baslangic, DateOnly bitis, SeyirMobilDbContext db) =>
{
    var baslangicKayit = await db.AracHareketleri
        .Where(h => h.AracPlaka == plaka && h.VeriTarihi >= baslangic)
        .OrderBy(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    var bitisKayit = await db.AracHareketleri
        .Where(h => h.AracPlaka == plaka && h.VeriTarihi <= bitis)
        .OrderByDescending(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    if (baslangicKayit is null || bitisKayit is null)
    {
        return Results.NotFound(new { message = "Bu plaka ve tarih aralığında veri bulunamadı." });
    }

    return Results.Ok(new
    {
        aracPlaka = plaka,
        baslangicTarihi = baslangicKayit.VeriTarihi,
        baslangicKm = baslangicKayit.KmSayaci,
        bitisTarihi = bitisKayit.VeriTarihi,
        bitisKm = bitisKayit.KmSayaci,
        yapilanKm = bitisKayit.KmSayaci - baslangicKayit.KmSayaci
    });
})
.WithName("GetAracHareketRaporu")
.RequireAuthorization();

// Ana ekranin listesi: tum arac hareketlerini (butun okumalari) donuyor.
// Siralama: tarihe gore (en yeni ustte), ayni tarihte birden fazla kayit varsa Id kucuk olan ustte.
app.MapGet("/api/arac-hareketleri", async (SeyirMobilDbContext db) =>
    await db.AracHareketleri
        .OrderByDescending(h => h.VeriTarihi)
        .ThenBy(h => h.Id)
        .ToListAsync())
    .WithName("GetAracHareketleri")
    .RequireAuthorization();

// Bir plaka + tarih icin en yakin onceki/sonraki okumayi (ve ayni tarihte kayit olup olmadigini)
// donuyor - masaustu, yeni kayit eklerken km sayaci icin gecerli araligi buradan hesapliyor.
app.MapGet("/api/arac-hareketleri/sinirlar", async (string plaka, DateOnly tarih, SeyirMobilDbContext db) =>
{
    var ayniTarihVarMi = await db.AracHareketleri
        .AnyAsync(h => h.AracPlaka == plaka && h.VeriTarihi == tarih);

    var onceki = await db.AracHareketleri
        .Where(h => h.AracPlaka == plaka && h.VeriTarihi < tarih)
        .OrderByDescending(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    var sonraki = await db.AracHareketleri
        .Where(h => h.AracPlaka == plaka && h.VeriTarihi > tarih)
        .OrderBy(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    return Results.Ok(new AracHareketSinirlar(
        ayniTarihVarMi,
        onceki?.VeriTarihi, onceki?.KmSayaci,
        sonraki?.VeriTarihi, sonraki?.KmSayaci));
})
.WithName("GetAracHareketSinirlari")
.RequireAuthorization();

// Yeni bir arac hareketi (okuma) ekler - km sayacinin, ayni plakanin en yakin onceki/sonraki
// okumalari arasinda (KESIN sinirlarla, esit degil) kalmasi sunucu tarafinda da dogrulanir -
// client tarafindaki NumericUpDown sinirlamasi sadece UX kolayligi, gercek kaynak burasi.
app.MapPost("/api/arac-hareketleri", async (CreateAracHareketRequest request, SeyirMobilDbContext db) =>
{
    if (request.Hiz < 0 || request.Hiz > 300)
    {
        return Results.BadRequest(new { message = "Hız 0-300 aralığında olmalı." });
    }

    var ayniTarihVarMi = await db.AracHareketleri
        .AnyAsync(h => h.AracPlaka == request.AracPlaka && h.VeriTarihi == request.VeriTarihi);
    if (ayniTarihVarMi)
    {
        return Results.BadRequest(new { message = "Bu plaka için bu tarihte zaten bir kayıt var." });
    }

    var onceki = await db.AracHareketleri
        .Where(h => h.AracPlaka == request.AracPlaka && h.VeriTarihi < request.VeriTarihi)
        .OrderByDescending(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    var sonraki = await db.AracHareketleri
        .Where(h => h.AracPlaka == request.AracPlaka && h.VeriTarihi > request.VeriTarihi)
        .OrderBy(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    if (onceki is not null && request.KmSayaci <= onceki.KmSayaci)
    {
        return Results.BadRequest(new
        {
            message = $"Km sayacı, {onceki.VeriTarihi:dd.MM.yyyy} tarihli {onceki.KmSayaci:N2} km değerinden büyük olmalı."
        });
    }
    if (sonraki is not null && request.KmSayaci >= sonraki.KmSayaci)
    {
        return Results.BadRequest(new
        {
            message = $"Km sayacı, {sonraki.VeriTarihi:dd.MM.yyyy} tarihli {sonraki.KmSayaci:N2} km değerinden küçük olmalı."
        });
    }
    if (request.KmSayaci < 0)
    {
        return Results.BadRequest(new { message = "Km sayacı negatif olamaz." });
    }

    var hareket = new AracHareket
    {
        AracId = request.AracId,
        AracPlaka = request.AracPlaka,
        VeriTarihi = request.VeriTarihi,
        Hiz = request.Hiz,
        KmSayaci = request.KmSayaci
    };
    db.AracHareketleri.Add(hareket);
    await db.SaveChangesAsync();
    return Results.Created($"/api/arac-hareketleri/{hareket.Id}", hareket);
})
.WithName("CreateAracHareket")
.RequireAuthorization();

app.MapDelete("/api/arac-hareketleri/{id:int}", async (int id, SeyirMobilDbContext db) =>
{
    var hareket = await db.AracHareketleri.FindAsync(id);
    if (hareket is null)
    {
        return Results.NotFound();
    }
    db.AracHareketleri.Remove(hareket);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteAracHareket")
.RequireAuthorization();

// Masaustu rapor ekranindaki plaka secim listesi icin: her aracin benzersiz AracId'si +
// plakasi. Ayni AracId birden cok satirda gectigi icin Distinct() ile tekillestiriliyor.
app.MapGet("/api/arac-hareketleri/plakalar", async (SeyirMobilDbContext db) =>
{
    // EF Core, Select+Distinct+OrderBy zincirini ozel bir record tipine dogrudan SQL'e
    // ceviremiyor (bilinen sinirlama) - once anonim tipe projekte edip SQL'de calistiriyoruz,
    // isimli AracPlakaLookup'a donusturme sorgudan SONRA (bellekte, LINQ to Objects) yapiliyor.
    var plakalar = await db.AracHareketleri
        .Select(h => new { h.AracId, h.AracPlaka })
        .Distinct()
        .OrderBy(p => p.AracPlaka)
        .ToListAsync();

    var sonuc = plakalar.Select(p => new AracPlakaLookup(p.AracId, p.AracPlaka));
    return Results.Ok(sonuc);
})
.WithName("GetAracPlakalari")
.RequireAuthorization();

// ---------- Excel'den toplu veri girisi (Madde 6, Eren bey 2. geri bildirimi) ----------

// Dosya yuklenince: parse + tam dogrulama TEK adimda. Sonuc, hem web hem masaustunun
// duzenlenebilir bir onizleme grid'inde gosterdigi ayni model (ImportSatiriSonuc).
app.MapPost("/api/arac-hareketleri/import-onizleme", async (IFormFile dosya, SeyirMobilDbContext db) =>
{
    string[] beklenenBasliklar = ["AracPlaka", "VeriTarihi", "Hiz", "KmSayaci"];

    using var stream = dosya.OpenReadStream();
    using var workbook = new XLWorkbook(stream);
    var sheet = workbook.Worksheets.First();

    // Sutunlari POZISYONA gore degil, BASLIK METNINE gore kontrol ediyoruz - aksi halde
    // (ornegin) rapor export'unun 4 sutunu ("Araç Plakası, Başlangıç Km, Bitiş Km, Yapılan Km")
    // sessizce yanlis anlamlarla okunup anlamsiz hatalarla dolu bir tablo uretiyordu (gercek
    // kullanici testinde yakalandi, 2026-08-06).
    var bulunanBasliklar = beklenenBasliklar
        .Select((_, i) => sheet.Cell(1, i + 1).GetString().Trim())
        .ToArray();
    var basliklarEslesiyorMu = beklenenBasliklar
        .Zip(bulunanBasliklar, (beklenen, bulunan) => PlakaValidator.Normalize(beklenen) == PlakaValidator.Normalize(bulunan))
        .All(esit => esit);

    if (!basliklarEslesiyorMu)
    {
        var mesaj = $"Bu dosyanın sütun başlıkları beklenen formatla eşleşmiyor. " +
            $"Beklenen: {string.Join(", ", beklenenBasliklar)}. " +
            $"Bulunan: {string.Join(", ", bulunanBasliklar.Select(b => string.IsNullOrWhiteSpace(b) ? "(boş)" : b))}. " +
            "Aşağıdaki \"Şablon İndir\" ile doğru formatta bir dosya indirip kullanabilirsiniz.";
        return Results.Ok(new ImportOnizlemeYaniti([], mesaj));
    }

    var hamSatirlar = new List<ImportHamSatir>();
    var satirNo = 1;
    foreach (var row in sheet.RowsUsed().Skip(1)) // ilk satir baslik
    {
        var plaka = row.Cell(1).GetString().Trim();
        string? tarihStr = row.Cell(2).TryGetValue<DateTime>(out var tarihDeger)
            ? tarihDeger.ToString("yyyy-MM-dd")
            : row.Cell(2).GetString().Trim();
        int? hiz = row.Cell(3).TryGetValue<int>(out var hizDeger) ? hizDeger : null;
        decimal? km = row.Cell(4).TryGetValue<decimal>(out var kmDeger) ? kmDeger : null;

        if (string.IsNullOrWhiteSpace(plaka) && string.IsNullOrWhiteSpace(tarihStr))
        {
            continue; // tamamen bos satir, dosyanin sonundaki bos satirlar icin
        }
        hamSatirlar.Add(new ImportHamSatir(satirNo, plaka, tarihStr ?? "", hiz, km));
        satirNo++;
    }
    return Results.Ok(new ImportOnizlemeYaniti(await ImportSatirlariDogrulaAsync(hamSatirlar, db)));
})
.WithName("ImportOnizleme")
.RequireAuthorization()
// IFormFile parametre baglamasi .NET'te varsayilan olarak antiforgery middleware istiyor
// (cookie tabanli auth icin CSRF korumasi) - biz JWT Bearer kullaniyoruz, cookie yok, CSRF
// riski bu senaryoda gecerli degil, bu yuzden bilincli olarak kapatiliyor.
.DisableAntiforgery();

// Kullanicinin dogru formati gormesi icin ornek/bos bir sablon .xlsx uretir - "AracPlaka,
// VeriTarihi, Hiz, KmSayaci" basliklariyla, bir ornek satirla. Yanlis dosya yukleme hatasinin
// (rapor export'unun import'a yuklenmesi gibi) tekrarlanma olasiligini azaltir.
app.MapGet("/api/arac-hareketleri/import-sablon", () =>
{
    using var workbook = new XLWorkbook();
    var sheet = workbook.Worksheets.Add("Şablon");
    var satirNo = YazKolonBasliklari(sheet, 1, ["AracPlaka", "VeriTarihi", "Hiz", "KmSayaci"]);
    sheet.Cell(satirNo, 1).Value = "34 AB 123";
    YazTarihHucresi(sheet.Cell(satirNo, 2), DateOnly.FromDateTime(DateTime.Today));
    sheet.Cell(satirNo, 3).Value = 60;
    YazKmHucresi(sheet.Cell(satirNo, 4), 12345.67m);
    sheet.Columns().AdjustToContents();
    return ExcelDosyasiSonucu(workbook, "import-sablon.xlsx");
})
.WithName("ImportSablonIndir")
.RequireAuthorization();

// Kullanici onizleme grid'inde bir hucreyi duzenledikten sonra ("Yeniden Dogrula" butonu) -
// dosyayi tekrar yuklemeden, sadece degisen veriyi AYNI dogrulama fonksiyonundan tekrar gecirir.
app.MapPost("/api/arac-hareketleri/import-yeniden-dogrula", async (ImportYenidenDogrulaRequest request, SeyirMobilDbContext db) =>
    Results.Ok(new ImportOnizlemeYaniti(await ImportSatirlariDogrulaAsync(request.Satirlar, db))))
.WithName("ImportYenidenDogrula")
.RequireAuthorization();

// Son onay - once AYNI dogrulama TEKRAR sunucu tarafinda calisir (istemciye guvenilmez,
// onizleme ile onayla arasinda baska bir kullanici/islem araya girmis olabilir), sonra
// sadece hatasiz VE (cakismasi yoksa ya da cakisma icin acik bir aksiyon secilmisse) satirlar
// islenir. Cakisma icin aksiyon secilmemisse o satir hata olarak geri bildirilir - kullanicinin
// KENDISI karar vermeli (2026-08-06 kullanici karari), sistem varsayilan bir davranis SECMEZ.
app.MapPost("/api/arac-hareketleri/import-onayla", async (ImportOnaylaRequest request, SeyirMobilDbContext db) =>
{
    var hamSatirlar = request.Satirlar
        .Select(s => new ImportHamSatir(s.SatirNo, s.AracPlaka, s.VeriTarihi ?? "", s.Hiz, s.KmSayaci))
        .ToList();
    var dogrulama = (await ImportSatirlariDogrulaAsync(hamSatirlar, db)).ToDictionary(d => d.SatirNo);

    var eklenen = 0;
    var guncellenen = 0;
    var atlanan = 0;
    var hatali = new List<string>();

    foreach (var satir in request.Satirlar)
    {
        var dogru = dogrulama[satir.SatirNo];

        // Kullanici bu satiri zaten ATLAYACAGINI soyledeyse ("Atla"), satirin hatali olup
        // olmamasi ya da gercekten bir cakismasi olup olmamasi ONEMSIZ - hicbir sekilde
        // veritabanina yazilmayacak. Bu yuzden hata kontrolunden ONCE calisir (kullanici
        // karari, 2026-08-07): boylece ayni dosyada bozuk BIR satiri "atla" diyerek gecip
        // digerlerini iceri alabiliyor, tek bir hatali satir yuzunden butun ice aktarma
        // reddedilmiyor.
        if (satir.CakismaAksiyonu == "Atla")
        {
            atlanan++;
            continue;
        }

        if (dogru.Hatalar.Count > 0)
        {
            hatali.Add($"Satır {satir.SatirNo}: {string.Join(" ", dogru.Hatalar)}");
            continue;
        }

        var tarih = DateOnly.Parse(dogru.VeriTarihi!);

        if (dogru.CakismaVarMi)
        {
            if (satir.CakismaAksiyonu != "UzerineYaz")
            {
                hatali.Add($"Satır {satir.SatirNo}: Çakışma için \"üzerine yaz\" veya \"atla\" seçilmedi.");
                continue;
            }
            var mevcutKayit = await db.AracHareketleri
                .FirstAsync(h => h.AracId == dogru.AracId && h.VeriTarihi == tarih);
            // dogru.Hatalar.Count == 0 zaten dogrulandi (satir 439) - Hiz/KmSayaci'nin dolu
            // oldugu garanti, ".Value" burada guvenli.
            mevcutKayit.Hiz = satir.Hiz!.Value;
            mevcutKayit.KmSayaci = satir.KmSayaci!.Value;
            guncellenen++;
        }
        else
        {
            db.AracHareketleri.Add(new AracHareket
            {
                AracId = dogru.AracId!.Value,
                AracPlaka = dogru.KanonikAracPlaka,
                VeriTarihi = tarih,
                Hiz = satir.Hiz!.Value,
                KmSayaci = satir.KmSayaci!.Value
            });
            eklenen++;
        }
    }
    await db.SaveChangesAsync();

    if (hatali.Count > 0)
    {
        return Results.BadRequest(new
        {
            message = "Bazı satırlar içe aktarılamadı, düzeltip tekrar deneyin.",
            hatalar = hatali,
            eklenenSayisi = eklenen,
            guncellenenSayisi = guncellenen,
            atlananSayisi = atlanan
        });
    }
    return Results.Ok(new ImportOnaylaSonuc(eklenen, guncellenen, atlanan));
})
.WithName("ImportOnayla")
.RequireAuthorization();

// Coklu plaka secimi icin toplu rapor - her plaka icin ayni "ilk/son okuma farki" mantigi
// tekrarlanir, tek bir istekte hepsi donulur (masaustunun N kere API'ye gitmesi yerine).
app.MapPost("/api/arac-hareketleri/rapor-toplu", async (RaporTopluRequest request, SeyirMobilDbContext db) =>
    Results.Ok(await HesaplaOzetRaporAsync(request.Plakalar, request.Baslangic, request.Bitis, db)))
    .WithName("GetAracHareketRaporuToplu")
    .RequireAuthorization();

// Detayli rapor: secilen plaka+tarih araligindaki HER gercek okumayi, bir
// onceki okumaya gore km farkiyla birlikte donuyor. Interpolasyon YOK -
// mevcut veri granularitesi aynen kullaniliyor (kullanici onayli mantik,
// 2026-08-04).
app.MapPost("/api/arac-hareketleri/rapor-detay", async (RaporTopluRequest request, SeyirMobilDbContext db) =>
    Results.Ok(await HesaplaDetayRaporAsync(request.Plakalar, request.Baslangic, request.Bitis, db)))
    .WithName("GetAracHareketDetayRaporu")
    .RequireAuthorization();

// Ana liste export'u (2026-08-04) - istemci ekranda o an gosterdigi (filtreli
// olabilir) satirlari gonderir, backend sadece tek tablolu bir .xlsx uretir.
app.MapPost("/api/arac-hareketleri/export", (List<AracHareketExportSatiri> satirlar) =>
{
    using var workbook = new XLWorkbook();
    var sheet = workbook.Worksheets.Add("Araç Hareketleri");

    var satirNo = YazKolonBasliklari(sheet, 1, ["Araç ID", "Araç Plaka", "Veri Tarihi", "Hız", "Km Sayacı"]);
    foreach (var s in satirlar)
    {
        sheet.Cell(satirNo, 1).Value = s.AracId;
        sheet.Cell(satirNo, 2).Value = s.AracPlaka;
        YazTarihHucresi(sheet.Cell(satirNo, 3), s.VeriTarihi);
        sheet.Cell(satirNo, 4).Value = s.Hiz;
        YazKmHucresi(sheet.Cell(satirNo, 5), s.KmSayaci);
        satirNo++;
    }

    sheet.Columns().AdjustToContents();
    return ExcelDosyasiSonucu(workbook, "arac-hareketleri.xlsx");
})
.WithName("ExportAracHareketleri")
.RequireAuthorization();

// Rapor export'u (2026-08-04) - ozet/detayli VE ayri-plaka-bazli/tumu-birlesik
// modlarinin 4 kombinasyonunu da uretir. Rapor hesabi ayni
// HesaplaOzetRaporAsync/HesaplaDetayRaporAsync fonksiyonlarini kullanir -
// ekrandaki rapor ile export arasinda mantik SAPMASI olmaz.
app.MapPost("/api/arac-hareketleri/rapor-export", async (RaporExportRequest request, SeyirMobilDbContext db) =>
{
    using var workbook = new XLWorkbook();
    var sheet = workbook.Worksheets.Add("Rapor");
    var satirNo = 1;

    if (request.DetayliMi)
    {
        var detaySonuclar = await HesaplaDetayRaporAsync(request.Plakalar, request.Baslangic, request.Bitis, db);
        string[] basliklar = ["Araç Plaka", "Veri Tarihi", "Km Sayacı", "Bir Önceki Okumaya Göre Artış"];

        void YazDetaySatirlari(IEnumerable<AracHareketDetayRaporSatiri> kaynak)
        {
            foreach (var s in kaynak)
            {
                sheet.Cell(satirNo, 1).Value = s.AracPlaka;
                YazTarihHucresi(sheet.Cell(satirNo, 2), s.VeriTarihi);
                YazKmHucresi(sheet.Cell(satirNo, 3), s.KmSayaci);
                if (s.Artis.HasValue)
                {
                    YazKmHucresi(sheet.Cell(satirNo, 4), s.Artis.Value);
                }
                else
                {
                    sheet.Cell(satirNo, 4).Value = "-";
                }
                satirNo++;
            }
        }

        if (request.AyriPlakaBazliMi)
        {
            foreach (var plaka in request.Plakalar)
            {
                satirNo = YazPlakaBasligi(sheet, satirNo, plaka, basliklar.Length);
                satirNo = YazKolonBasliklari(sheet, satirNo, basliklar);
                YazDetaySatirlari(detaySonuclar.Where(s => s.AracPlaka == plaka));
                satirNo++; // bloklar arasi bos satir
            }
        }
        else
        {
            satirNo = YazKolonBasliklari(sheet, satirNo, basliklar);
            YazDetaySatirlari(detaySonuclar);
        }
    }
    else
    {
        var ozetSonuclar = await HesaplaOzetRaporAsync(request.Plakalar, request.Baslangic, request.Bitis, db);
        string[] basliklar = ["Araç Plakası", "Başlangıç Km", "Bitiş Km", "Yapılan Km"];

        void YazOzetSatirlari(IEnumerable<AracRaporSonucu> kaynak)
        {
            foreach (var s in kaynak)
            {
                sheet.Cell(satirNo, 1).Value = s.AracPlaka;
                if (s.BulunduMu)
                {
                    YazKmHucresi(sheet.Cell(satirNo, 2), s.BaslangicKm!.Value);
                    YazKmHucresi(sheet.Cell(satirNo, 3), s.BitisKm!.Value);
                    YazKmHucresi(sheet.Cell(satirNo, 4), s.YapilanKm!.Value);
                }
                else
                {
                    sheet.Cell(satirNo, 2).Value = "Veri yok";
                    sheet.Cell(satirNo, 3).Value = "Veri yok";
                    sheet.Cell(satirNo, 4).Value = "Veri yok";
                }
                satirNo++;
            }
        }

        if (request.AyriPlakaBazliMi)
        {
            foreach (var s in ozetSonuclar)
            {
                satirNo = YazPlakaBasligi(sheet, satirNo, s.AracPlaka, basliklar.Length);
                satirNo = YazKolonBasliklari(sheet, satirNo, basliklar);
                YazOzetSatirlari([s]);
                satirNo++;
            }
        }
        else
        {
            satirNo = YazKolonBasliklari(sheet, satirNo, basliklar);
            YazOzetSatirlari(ozetSonuclar);
        }
    }

    sheet.Columns().AdjustToContents();
    return ExcelDosyasiSonucu(workbook, "rapor.xlsx");
})
.WithName("ExportRapor")
.RequireAuthorization();

// ---------- Auth (login/session/token) ----------

app.MapPost("/api/auth/login", async (LoginRequest request, SeyirMobilDbContext db, IConfiguration config, SessionStore sessionStore) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var sessionId = Guid.NewGuid().ToString("N");
    await sessionStore.OturumBaslatAsync(sessionId, user.Id);

    var (token, expiresAt) = JwtOlustur(user, config, sessionId);
    return Results.Ok(new LoginResponse(token, expiresAt, user.Username, user.Role));
})
.WithName("Login");

app.MapGet("/api/auth/me", (ClaimsPrincipal principal) =>
    Results.Ok(new
    {
        username = principal.Identity?.Name,
        role = principal.FindFirstValue(ClaimTypes.Role)
    }))
.RequireAuthorization()
.WithName("GetCurrentUser");

// Oturumu (token'in gomulu "sid"si uzerinden) sunucu tarafinda sonlandirir - istemcinin
// kendi tarafinda token'i silmesinin yaninda, sunucu da idle-timeout tablosundan kaydi kaldirir.
app.MapPost("/api/auth/logout", async (ClaimsPrincipal principal, SessionStore sessionStore) =>
{
    var sessionId = principal.FindFirstValue("sid");
    if (sessionId is not null)
    {
        await sessionStore.OturumBitirAsync(sessionId);
    }
    return Results.NoContent();
})
.RequireAuthorization()
.WithName("Logout");

// ---------- Kullanici yonetimi (Admin rolu zorunlu - self servis kayit YOK, kasitli) ----------

app.MapGet("/api/users", async (SeyirMobilDbContext db) =>
    await db.Users
        .OrderBy(u => u.Username)
        .Select(u => new UserSummary(u.Id, u.Username, u.Role, u.OlusturmaTarihi))
        .ToListAsync())
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("GetUsers");

app.MapPost("/api/users", async (CreateUserRequest request, SeyirMobilDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Username))
    {
        return Results.BadRequest(new { message = "Kullanıcı adı boş olamaz." });
    }
    if (request.Password.Length < 6)
    {
        return Results.BadRequest(new { message = "Şifre en az 6 karakter olmalı." });
    }
    if (request.Role != "Admin" && request.Role != "Viewer")
    {
        return Results.BadRequest(new { message = "Rol 'Admin' veya 'Viewer' olmalı." });
    }

    var kullaniciAdiVarMi = await db.Users.AnyAsync(u => u.Username == request.Username);
    if (kullaniciAdiVarMi)
    {
        return Results.BadRequest(new { message = "Bu kullanıcı adı zaten kayıtlı." });
    }

    var user = new User
    {
        Username = request.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        Role = request.Role
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", new UserSummary(user.Id, user.Username, user.Role, user.OlusturmaTarihi));
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("CreateUser");

app.MapDelete("/api/users/{id:int}", async (int id, SeyirMobilDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("DeleteUser");

// ---------- Frontend etkilesim loglari (Madde 5, Eren bey 2. geri bildirimi) ----------

// Web'deki HER tiklama/etkilesim buraya gonderiliyor - istemci hicbir zaman Graylog'a
// DOGRUDAN baglanmiyor (mimari kural: §2, istemciler sadece API'yi cagirir), backend bunu
// kendi Serilog->Graylog hattina (yukaridaki UseSerilogRequestLogging ile AYNI pipeline)
// aktariyor. Auth ZORUNLU DEGIL - login ekranindaki tiklamalar da (henuz token yokken)
// loglanabilsin diye; kullanici bilgisi varsa (token gonderildiyse) otomatik ekleniyor.
app.MapPost("/api/frontend-log", (FrontendLogRequest request, ClaimsPrincipal principal, ILogger<Program> logger) =>
{
    logger.LogInformation(
        "Frontend etkilesim: {Eylem} | {Detay} | Sayfa={Sayfa} | Kullanici={Kullanici}",
        request.Eylem,
        request.Detay ?? "-",
        request.Sayfa,
        principal.Identity?.Name ?? "anonim");
    return Results.NoContent();
})
.WithName("FrontendLog");

app.Run();

// ---------- JWT uretimi (login endpoint'i kullanir) ----------

static (string Token, DateTime ExpiresAt) JwtOlustur(User user, IConfiguration config, string sessionId)
{
    var jwtSection = config.GetSection("Jwt");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiryMinutes"]!, CultureInfo.InvariantCulture));

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("sid", sessionId)
    };

    var token = new JwtSecurityToken(
        issuer: jwtSection["Issuer"],
        audience: jwtSection["Audience"],
        claims: claims,
        expires: expiresAt,
        signingCredentials: creds);

    return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
}

// ---------- Excel import dogrulama (import-onizleme/yeniden-dogrula/onayla ortak kullanir) ----------

// Bir dosyadaki (veya kullanici tarafindan duzenlenmis) TUM satirlari tek seferde dogrular.
// Tek satirlik ekleme endpoint'inden (POST /api/arac-hareketleri) farki: burada AYNI plaka icin
// BIRDEN FAZLA yeni okuma AYNI ANDA gelebiliyor - km tutarliligi sadece DB'deki mevcut kayitlara
// gore degil, dosyadaki DIGER satirlara gore de (kronolojik siraya sokulup) kontrol edilmeli.
static async Task<List<ImportSatiriSonuc>> ImportSatirlariDogrulaAsync(List<ImportHamSatir> hamSatirlar, SeyirMobilDbContext db)
{
    // 1. Mevcut (normalize plaka -> AracId + orijinal DB plaka string'i) eslemesi.
    var mevcutPlakalar = await db.AracHareketleri
        .Select(h => new { h.AracId, h.AracPlaka })
        .Distinct()
        .ToListAsync();

    var normalizeToMevcut = new Dictionary<string, (int AracId, string OrijinalPlaka)>();
    foreach (var p in mevcutPlakalar)
    {
        normalizeToMevcut.TryAdd(PlakaValidator.Normalize(p.AracPlaka), (p.AracId, p.AracPlaka));
    }
    var sonrakiYeniAracId = mevcutPlakalar.Count > 0 ? mevcutPlakalar.Max(p => p.AracId) + 1 : 1;

    // 2. Her satirin plaka/AracId'sini coz - AYNI normalize plaka (dosya icinde tekrar etse bile)
    // HER ZAMAN ayni AracId'yi alir (duplicate arac olusmasini onleyen mekanizma, kullanici karari).
    var yeniPlakaAtamalari = new Dictionary<string, int>();
    var cozumlenmis = new List<(ImportHamSatir Ham, string NormPlaka, int AracId, bool Yeni, string KanonikPlaka, DateOnly? Tarih, List<string> Hatalar)>();

    foreach (var satir in hamSatirlar)
    {
        var hatalar = new List<string>();
        var normPlaka = PlakaValidator.Normalize(satir.AracPlaka ?? "");

        if (string.IsNullOrWhiteSpace(satir.AracPlaka) || !PlakaValidator.IsValid(satir.AracPlaka))
        {
            hatalar.Add("Geçersiz plaka formatı. İl kodu 01-81 arasında olmalı. Ardından en fazla 3 harf gelir (Q, W, X harfleri kullanılmaz), şu kalıplardan biriyle devam eder: \"99 X 9999\", \"99 X 99999\", \"99 XX 999\", \"99 XX 9999\", \"99 XXX 99\" veya \"99 XXX 999\". Örnek: \"34 AB 141\".");
        }

        DateOnly? tarih = DateOnly.TryParse(satir.VeriTarihi, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTarih)
            ? parsedTarih
            : null;
        if (tarih is null)
        {
            hatalar.Add("Geçersiz veya boş tarih.");
        }

        if (satir.Hiz is null || satir.Hiz < 0 || satir.Hiz > 300)
        {
            hatalar.Add("Hız 0-300 aralığında olmalı.");
        }
        if (satir.KmSayaci is null || satir.KmSayaci < 0)
        {
            hatalar.Add("Km sayacı negatif olamaz veya boş.");
        }

        int aracId;
        bool yeni;
        string kanonikPlaka;
        if (normalizeToMevcut.TryGetValue(normPlaka, out var mevcut))
        {
            aracId = mevcut.AracId;
            yeni = false;
            kanonikPlaka = mevcut.OrijinalPlaka;
        }
        else if (yeniPlakaAtamalari.TryGetValue(normPlaka, out var atanmisId))
        {
            aracId = atanmisId;
            yeni = true;
            kanonikPlaka = PlakaValidator.Format(normPlaka);
        }
        else
        {
            aracId = sonrakiYeniAracId++;
            yeniPlakaAtamalari[normPlaka] = aracId;
            yeni = true;
            kanonikPlaka = PlakaValidator.Format(normPlaka);
        }

        cozumlenmis.Add((satir, normPlaka, aracId, yeni, kanonikPlaka, tarih, hatalar));
    }

    // 3. Dosya/duzenleme icinde ayni plaka+tarih TEKRARI (iki satir ayni araca ayni gun icin).
    foreach (var grup in cozumlenmis.Where(s => s.Tarih.HasValue).GroupBy(s => (s.AracId, s.Tarih)))
    {
        if (grup.Count() > 1)
        {
            foreach (var s in grup)
            {
                s.Hatalar.Add("Bu dosyada aynı plaka + tarih için birden fazla satır var.");
            }
        }
    }

    // 4. Arac (AracId) bazinda: DB'deki mevcut okumalar + bu grubun gecerli satirlari BIRLIKTE
    // kronolojik siraya konup km sayacinin artan oldugu dogrulanir, ayrica DB ile birebir
    // tarih cakismasi tespit edilir.
    var sonuclar = new List<ImportSatiriSonuc>();
    foreach (var grup in cozumlenmis.GroupBy(s => s.AracId))
    {
        var orijinalDbPlaka = normalizeToMevcut.Values.FirstOrDefault(v => v.AracId == grup.Key).OrijinalPlaka;
        var dbOkumalari = orijinalDbPlaka is not null
            ? await db.AracHareketleri.Where(h => h.AracPlaka == orijinalDbPlaka).ToListAsync()
            : [];

        var zamanSirali = dbOkumalari
            .Select(h => (VeriTarihi: h.VeriTarihi, KmSayaci: h.KmSayaci, SatirNo: (int?)null))
            .Concat(grup
                .Where(s => s.Tarih.HasValue && s.Ham.KmSayaci.HasValue && s.Hatalar.Count == 0)
                .Select(s => (VeriTarihi: s.Tarih!.Value, KmSayaci: s.Ham.KmSayaci!.Value, SatirNo: (int?)s.Ham.SatirNo)))
            .OrderBy(z => z.VeriTarihi)
            .ToList();

        foreach (var s in grup)
        {
            var dbEslesen = dbOkumalari.FirstOrDefault(h => h.VeriTarihi == s.Tarih);
            var cakismaVarMi = dbEslesen is not null;

            if (s.Tarih.HasValue && s.Ham.KmSayaci.HasValue && s.Hatalar.Count == 0 && !cakismaVarMi)
            {
                var index = zamanSirali.FindIndex(z => z.SatirNo == s.Ham.SatirNo);
                if (index > 0 && s.Ham.KmSayaci <= zamanSirali[index - 1].KmSayaci)
                {
                    s.Hatalar.Add($"Km sayacı, {zamanSirali[index - 1].VeriTarihi:dd.MM.yyyy} tarihli {zamanSirali[index - 1].KmSayaci:N2} km değerinden büyük olmalı.");
                }
                if (index < zamanSirali.Count - 1 && s.Ham.KmSayaci >= zamanSirali[index + 1].KmSayaci)
                {
                    s.Hatalar.Add($"Km sayacı, {zamanSirali[index + 1].VeriTarihi:dd.MM.yyyy} tarihli {zamanSirali[index + 1].KmSayaci:N2} km değerinden küçük olmalı.");
                }
            }

            sonuclar.Add(new ImportSatiriSonuc(
                s.Ham.SatirNo,
                s.Ham.AracPlaka,
                s.KanonikPlaka,
                s.AracId,
                s.Yeni,
                s.Tarih?.ToString("yyyy-MM-dd"),
                s.Ham.Hiz,
                s.Ham.KmSayaci,
                cakismaVarMi,
                dbEslesen?.Hiz,
                dbEslesen?.KmSayaci,
                s.Hatalar));
        }
    }

    return [.. sonuclar.OrderBy(s => s.SatirNo)];
}

// ---------- Rapor hesaplama (rapor-toplu/rapor-detay VE export endpoint'leri ortak kullanir) ----------

static async Task<List<AracRaporSonucu>> HesaplaOzetRaporAsync(List<string> plakalar, DateOnly baslangic, DateOnly bitis, SeyirMobilDbContext db)
{
    var sonuclar = new List<AracRaporSonucu>();

    foreach (var plaka in plakalar)
    {
        // Ikisi de AYNI [baslangic, bitis] araligiyla sinirli olmali - onceden baslangicKayit
        // sadece ">= baslangic" (ustsinir YOKTU) ve bitisKayit sadece "<= bitis" (altsinir YOKTU)
        // ile ariyordu. Aracin bu aralikta HIC okumasi olmayip once/sonrasinda okumalari varsa,
        // ikisi ayri ayri araligin DISINDAN birer kayit buluyor, buldugu iki kaydin tarihleri
        // TERS siraya (baslangic > bitis) dusup "Yapilan Km" NEGATIF cikiyordu (1000 satirlik
        // test verisiyle bulunan gercek bug, 2026-08-07).
        var baslangicKayit = await db.AracHareketleri
            .Where(h => h.AracPlaka == plaka && h.VeriTarihi >= baslangic && h.VeriTarihi <= bitis)
            .OrderBy(h => h.VeriTarihi)
            .FirstOrDefaultAsync();

        var bitisKayit = await db.AracHareketleri
            .Where(h => h.AracPlaka == plaka && h.VeriTarihi >= baslangic && h.VeriTarihi <= bitis)
            .OrderByDescending(h => h.VeriTarihi)
            .FirstOrDefaultAsync();

        if (baslangicKayit is null || bitisKayit is null)
        {
            sonuclar.Add(new AracRaporSonucu(plaka, false, null, null, null, null, null));
            continue;
        }

        sonuclar.Add(new AracRaporSonucu(
            plaka,
            true,
            baslangicKayit.VeriTarihi,
            baslangicKayit.KmSayaci,
            bitisKayit.VeriTarihi,
            bitisKayit.KmSayaci,
            bitisKayit.KmSayaci - baslangicKayit.KmSayaci));
    }

    return sonuclar;
}

static async Task<List<AracHareketDetayRaporSatiri>> HesaplaDetayRaporAsync(List<string> plakalar, DateOnly baslangic, DateOnly bitis, SeyirMobilDbContext db)
{
    var sonuc = new List<AracHareketDetayRaporSatiri>();

    foreach (var plaka in plakalar)
    {
        var okumalar = await db.AracHareketleri
            .Where(h => h.AracPlaka == plaka && h.VeriTarihi >= baslangic && h.VeriTarihi <= bitis)
            .OrderBy(h => h.VeriTarihi)
            .ToListAsync();

        decimal? oncekiKm = null;
        foreach (var okuma in okumalar)
        {
            var artis = oncekiKm.HasValue ? okuma.KmSayaci - oncekiKm.Value : (decimal?)null;
            sonuc.Add(new AracHareketDetayRaporSatiri(plaka, okuma.VeriTarihi, okuma.KmSayaci, artis));
            oncekiKm = okuma.KmSayaci;
        }
    }

    return sonuc;
}

// ---------- Excel yazma yardımcıları (ClosedXML) ----------

static int YazKolonBasliklari(IXLWorksheet sheet, int satirNo, IReadOnlyList<string> basliklar)
{
    for (var i = 0; i < basliklar.Count; i++)
    {
        var hucre = sheet.Cell(satirNo, i + 1);
        hucre.Value = basliklar[i];
        hucre.Style.Font.Bold = true;
        hucre.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
    }
    return satirNo + 1;
}

// Plaka adini, kolon sayisi kadar hucreyi birlestirerek kalin bir "bölüm başlığı" olarak yazar.
static int YazPlakaBasligi(IXLWorksheet sheet, int satirNo, string plaka, int kolonSayisi)
{
    var hucre = sheet.Cell(satirNo, 1);
    hucre.Value = plaka;
    hucre.Style.Font.Bold = true;
    hucre.Style.Font.FontSize = 13;
    hucre.Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
    sheet.Range(satirNo, 1, satirNo, kolonSayisi).Merge();
    return satirNo + 1;
}

static void YazTarihHucresi(IXLCell hucre, DateOnly tarih)
{
    hucre.Value = tarih.ToDateTime(TimeOnly.MinValue);
    hucre.Style.DateFormat.Format = "dd.MM.yyyy";
}

static void YazKmHucresi(IXLCell hucre, decimal km)
{
    hucre.Value = km;
    hucre.Style.NumberFormat.Format = "#,##0.00";
}

static IResult ExcelDosyasiSonucu(XLWorkbook workbook, string dosyaAdi)
{
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    return Results.File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        dosyaAdi);
}
