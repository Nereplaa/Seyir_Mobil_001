# Gelistirme icin: SDK image (runtime-only degil), "dotnet watch" ile dosya degisikliklerinde
# konteyner icinde otomatik yeniden derleme/restart - host'ta `dotnet run` calistirmanin
# konteynerlestirilmis karsiligi. Kaynak kod docker-compose'da bir volume olarak bagliyor,
# image'in kendisi sadece NuGet paketlerini onceden restore ediyor (build hizi icin).
FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /src

COPY SeyirMobil.Api.csproj .
RUN dotnet restore

COPY . .

EXPOSE 5080

ENTRYPOINT ["dotnet", "watch", "run", "--no-launch-profile", "--urls", "http://0.0.0.0:5080"]
