// 401 (oturum gecersiz/suresi dolmus) hatasini component-bazli error callback'lerinde tespit
// etmek icin kullanilir - bu durumda auth-interceptor zaten net bir "Oturum süreniz doldu"
// mesaji gosterip kullaniciyi /login'e yonlendiriyor, component'in KENDI genel "Backend API
// çalışıyor mu?" mesajini AYRICA gostermesine gerek yok.
export function oturumHatasiMi(hata: unknown): boolean {
  return typeof hata === 'object' && hata !== null && (hata as { status?: number }).status === 401;
}
