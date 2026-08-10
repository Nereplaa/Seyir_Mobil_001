using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SeyirMobil.Desktop.Models;

namespace SeyirMobil.Desktop.Services;

public class AracHareketApiClient
{
    private readonly HttpClient _http;

    public AracHareketApiClient()
    {
        // OturumHandler, HER cevapta 401 kontrolu yapip TokenStore'u haberdar ediyor - istemci
        // formlarinin ayri ayri 401 kontrolu yazmasina gerek kalmiyor.
        _http = new HttpClient(new OturumHandler())
        {
            BaseAddress = new Uri("http://localhost:5080/")
        };
        if (TokenStore.Token is not null)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.Token);
        }
    }

    public async Task<LoginResponseDto> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequestDto(username, password));
        if (!response.IsSuccessStatusCode)
        {
            var mesaj = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                ? "Kullanıcı adı veya şifre hatalı."
                : await OkuHataMesajiAsync(response);
            throw new InvalidOperationException(mesaj);
        }
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return result ?? throw new InvalidOperationException("Sunucudan geçersiz bir yanıt alındı.");
    }

    // Basarili login sonrasi, o anki HttpClient'a token'i hemen uygular - AracHareketleriForm
    // ayni oturumda tekrar bir ApiClient olusturmadan (constructor zaten TokenStore'dan okur,
    // ama LoginForm'un KENDI client'i icin bu gerekli) devam edebilsin diye.
    public void UygulaToken(string token) =>
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public async Task LogoutAsync()
    {
        try
        {
            await _http.PostAsync("api/auth/logout", null);
        }
        catch
        {
            // Sunucuya ulasilamasa bile onemli degil - yerel oturum zaten cagiran tarafindan temizlenir.
        }
    }

    public async Task<List<AracHareketDto>> GetTumHareketlerAsync()
    {
        var result = await _http.GetFromJsonAsync<List<AracHareketDto>>("api/arac-hareketleri");
        return result ?? [];
    }

    public async Task<List<AracPlakaLookupDto>> GetPlakalarAsync()
    {
        var result = await _http.GetFromJsonAsync<List<AracPlakaLookupDto>>("api/arac-hareketleri/plakalar");
        return result ?? [];
    }

    public async Task<List<AracRaporSonucuDto>> GetRaporTopluAsync(List<string> plakalar, DateOnly baslangic, DateOnly bitis)
    {
        var request = new RaporTopluRequestDto(plakalar, baslangic, bitis);
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/rapor-toplu", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<AracRaporSonucuDto>>();
        return result ?? [];
    }

    public async Task<List<AracHareketDetayRaporSatiriDto>> GetDetayRaporuAsync(List<string> plakalar, DateOnly baslangic, DateOnly bitis)
    {
        var request = new RaporTopluRequestDto(plakalar, baslangic, bitis);
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/rapor-detay", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<AracHareketDetayRaporSatiriDto>>();
        return result ?? [];
    }

    public async Task<byte[]> ExportHareketlerAsync(List<AracHareketDto> hareketler)
    {
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/export", hareketler);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]> ExportRaporAsync(RaporExportRequestDto request)
    {
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/rapor-export", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<AracHareketSinirlarDto> GetSinirlarAsync(string plaka, DateOnly tarih)
    {
        var url = $"api/arac-hareketleri/sinirlar?plaka={Uri.EscapeDataString(plaka)}&tarih={tarih:yyyy-MM-dd}";
        var result = await _http.GetFromJsonAsync<AracHareketSinirlarDto>(url);
        return result ?? new AracHareketSinirlarDto(false, null, null, null, null);
    }

    public async Task CreateHareketAsync(CreateAracHareketRequestDto request)
    {
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri", request);
        if (!response.IsSuccessStatusCode)
        {
            var mesaj = await OkuHataMesajiAsync(response);
            throw new InvalidOperationException(mesaj);
        }
    }

    public async Task DeleteHareketAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/arac-hareketleri/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<ImportOnizlemeYanitiDto> ImportOnizleAsync(string dosyaYolu)
    {
        using var form = new MultipartFormDataContent();
        await using var stream = File.OpenRead(dosyaYolu);
        using var dosyaIcerik = new StreamContent(stream);
        form.Add(dosyaIcerik, "dosya", Path.GetFileName(dosyaYolu));

        var response = await _http.PostAsync("api/arac-hareketleri/import-onizleme", form);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await OkuHataMesajiAsync(response));
        }
        var result = await response.Content.ReadFromJsonAsync<ImportOnizlemeYanitiDto>();
        return result ?? new ImportOnizlemeYanitiDto([]);
    }

    public async Task<ImportOnizlemeYanitiDto> ImportYenidenDogrulaAsync(List<ImportHamSatirDto> satirlar)
    {
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/import-yeniden-dogrula", new ImportYenidenDogrulaRequestDto(satirlar));
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await OkuHataMesajiAsync(response));
        }
        var result = await response.Content.ReadFromJsonAsync<ImportOnizlemeYanitiDto>();
        return result ?? new ImportOnizlemeYanitiDto([]);
    }

    public async Task<ImportOnaylaSonucDto> ImportOnaylaAsync(List<ImportOnaylaSatiriDto> satirlar)
    {
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/import-onayla", new ImportOnaylaRequestDto(satirlar));
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await OkuHataMesajiAsync(response));
        }
        var result = await response.Content.ReadFromJsonAsync<ImportOnaylaSonucDto>();
        return result ?? new ImportOnaylaSonucDto(0, 0, 0);
    }

    private static async Task<string> OkuHataMesajiAsync(HttpResponseMessage response)
    {
        try
        {
            var gövde = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(gövde);
            if (doc.RootElement.TryGetProperty("message", out var mesajElemani))
            {
                return mesajElemani.GetString() ?? response.ReasonPhrase ?? "Bilinmeyen hata";
            }
        }
        catch
        {
            // gövde JSON degilse asagida genel mesaja dusulur
        }
        return response.ReasonPhrase ?? "Bilinmeyen hata";
    }
}
