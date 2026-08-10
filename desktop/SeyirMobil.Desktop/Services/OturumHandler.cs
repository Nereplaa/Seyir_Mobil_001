using System.Net;

namespace SeyirMobil.Desktop.Services;

// HttpClient pipeline'ina eklenen bir katman: 401 donen HER cevapta oturumu (token'i) gecersiz
// sayip TokenStore uzerinden UI katmanina haber verir - boylece her form kendi 401 kontrolunu
// ayri ayri yazmak zorunda kalmaz (login denemesi basarisiz oldugunda da 401 doner, ama o an
// TokenStore.Token zaten bos oldugu ve hicbir form olaya abone olmadigi icin zararsizdir).
public class OturumHandler : DelegatingHandler
{
    public OturumHandler() : base(new HttpClientHandler()) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            TokenStore.GecersizOldu();
        }
        return response;
    }
}
