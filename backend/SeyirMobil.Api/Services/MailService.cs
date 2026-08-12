using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SeyirMobil.Api.Services;

// SMTP uzerinden mail gonderir (MailKit) - "Smtp:*" ayarlari appsettings/.env'den geliyor
// (Jwt:SecretKey ile AYNI desen: appsettings.json'da PLACEHOLDER, gercek deger sadece
// docker-compose environment -> .env'den). Gelistirme asamasinda Mailtrap gibi gercek
// kullaniciya mail GITMEYEN bir test SMTP servisi kullanilmasi hedefleniyor (feedback_001,
// 2026-08-11 karari) - ileride gercek bir SMTP hesabina gecis SADECE .env degisikligiyle
// yapilabilecek sekilde kuruldu, kod tarafinda degisiklik gerekmiyor.
//
// BILINCLI TASARIM: mail gonderimi BASARISIZ olursa (SMTP yapilandirilmamis, Mailtrap
// erisilemez vb.) cagiran endpoint COKMEZ - hata loglanir (Serilog -> konsol + Graylog,
// projenin geri kalaninda zaten kurulu olan AYNI log hatti), false doner. Bu, Graylog
// sink'inin "Graylog kapaliysa uygulama cokmez" ilkesiyle AYNI dayaniklilik felsefesi.
public class MailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailService> _logger;

    public MailService(IConfiguration config, ILogger<MailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> GonderAsync(string aliciEmail, string konu, string govdeMetni)
    {
        var host = _config["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("Mail gonderilemedi - Smtp:Host yapilandirilmamis (.env'de SMTP_HOST eksik olabilir).");
            return false;
        }

        try
        {
            var mesaj = new MimeMessage();
            mesaj.From.Add(new MailboxAddress(
                _config["Smtp:FromName"] ?? "Seyir Mobil",
                _config["Smtp:FromAddress"] ?? "no-reply@seyirmobil.local"));
            mesaj.To.Add(MailboxAddress.Parse(aliciEmail));
            mesaj.Subject = konu;
            mesaj.Body = new TextPart("plain") { Text = govdeMetni };

            using var client = new SmtpClient();
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTlsWhenAvailable);

            var kullanici = _config["Smtp:Username"];
            var sifre = _config["Smtp:Password"];
            if (!string.IsNullOrWhiteSpace(kullanici))
            {
                await client.AuthenticateAsync(kullanici, sifre ?? string.Empty);
            }

            await client.SendAsync(mesaj);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Mail gonderildi: Alici={Alici} Konu={Konu}", aliciEmail, konu);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mail gonderilemedi: Alici={Alici} Konu={Konu}", aliciEmail, konu);
            return false;
        }
    }
}
