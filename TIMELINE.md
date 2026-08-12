# 📅 Seyir Mobil — Proje Timeline'ı

> Bu dosya, "Seyir Mobil - Araç Takip Sistemi" projesinin gelişim sürecini kronolojik olarak,
> tarih/saat bilgisiyle birlikte anlatır — ne yapıldı, neden, sırada ne var. Teknik geliştirme
> notları ayrı ve yerel bir çalışma alanında tutuluyor; burada projenin **nasıl geliştiği**
> anlatılıyor.

---

## 2026-08-03 — Proje Başlangıcı ve Mimari Kararlar

**Staj başladı.** Seyir Mobil bünyesinde, araç kilometre/plaka/kayıt takibi yapan bir sistem
geliştirilmesine karar verildi. İlk aşamada yerel bir veritabanı + masaüstü arayüzü hedefleniyor,
ilerleyen süreçte kurumdan alınacak resmi gereksinimler doğrultusunda web/çoklu platform desteğine
genişletilmesi planlanıyor.

**Mimari karar:** Sistem, birbirinden ayrı ama birlikte çalışan bileşenlerden oluşacak şekilde
tasarlandı:

- **Veritabanı:** SQL Server — sadece backend'den erişilir.
- **Backend:** ASP.NET Core Web API — tüm iş mantığı ve veritabanı erişimi burada toplanır.
- **Masaüstü istemci:** WinForms — sadece backend API'ye HTTP ile bağlanır, veritabanına doğrudan
  erişmez.
- **Web istemci (ileride):** Aynı backend API'yi kullanacak, iş mantığı ikinci kez yazılmayacak.

Bu yaklaşımın amacı: iş mantığının tek bir yerde toplanması, ileride web/mobil eklemenin
kolaylaşması ve veritabanının hiçbir istemciye doğrudan açılmadan güvenli kalması.

### Veritabanı Kurulumu
- SQL Server 2025 Express + SQL Server Management Studio (SSMS) kuruldu.
- `SeyirMobilDb` veritabanı ve `Vehicles` tablosu oluşturuldu (araç ID, plaka, toplam kilometre,
  kayıt tarihi).
- Tabloya 5 adet test (dummy) kaydı eklendi, hem script hem SSMS üzerinden görsel olarak
  doğrulandı.

### 2026-08-03 13:15 — .NET 10 SDK Kurulumu
Backend geliştirmeye başlayabilmek için .NET 10 SDK (güncel LTS sürüm) kuruldu ve doğrulandı.

### 2026-08-03 13:32 — Backend API Çalışıyor: `SeyirMobil.Api`

ASP.NET Core Web API projesi (.NET 10) oluşturuldu ve Entity Framework Core ile `SeyirMobilDb`
veritabanına bağlandı. `Vehicles` için temel uç noktalar (endpoint) yazıldı ve gerçekten
çalıştırılıp test edildi:

- `GET /api/vehicles` — tüm araçları listeler
- `GET /api/vehicles/{id}` — tek bir aracı getirir
- `POST /api/vehicles` — yeni araç ekler

Swagger arayüzü (`/swagger`) ile API'nin tarayıcıdan görsel olarak da denenebilmesi sağlandı.

### 2026-08-03 13:54 — Masaüstü Uygulaması Hazır: Listeleme, Ekleme, Silme

WinForms tabanlı masaüstü uygulaması (`SeyirMobil.Desktop`) geliştirildi ve gerçekten test edildi:

- Araç listesi bir tabloda (grid) gösteriliyor.
- Yeni araç eklenebiliyor — **plaka, gerçek Türkiye plaka formatına** (il kodu + harf + rakam)
  göre doğrulanıyor; toplam kilometre negatif olamıyor.
- Araç silinebiliyor (onay penceresiyle, yanlışlıkla silmeyi önlemek için).
- Liste "Yenile" butonuyla güncellenebiliyor.

Uygulama sadece backend API üzerinden çalışıyor, veritabanına hiç doğrudan bağlanmıyor.

---

## 2026-08-03 14:48 — Kurum Gereksinimi Geldi: Araç Hareket Raporu

Kurumdan resmi proje gereksinimi ulaştı: araçların zaman içindeki periyodik hareket kayıtlarından
(tarih, hız, kilometre sayacı), verilen bir plaka ve tarih aralığı için "o aralıkta kaç km yol
yapıldığını" gösteren bir rapor üretilmesi isteniyor.

Bu doğrultuda:

- Araçların zaman içindeki km sayacı okumalarını tutan yeni bir veri yapısı kuruldu, 100 satırlık
  çeşitli örnek veriyle dolduruldu.
- Backend'e, verilen plaka ve tarih aralığı için başlangıç km / bitiş km / yapılan km hesaplayan
  bir rapor uç noktası eklendi ve gerçek verilerle test edildi.

---

### 2026-08-03 15:14 — Rapor Ekranı ve Ana Ekran Güncellemesi

Masaüstü uygulamasına, kurum gereksiniminin karşılığı olan rapor ekranı eklendi:

- Birden fazla araç aynı anda seçilip tek seferde rapor alınabiliyor.
- Tarih aralığı seçimi kullanıcı hata yapamayacak şekilde tasarlandı (bitiş tarihi, başlangıçtan
  sonraki bir tarih olmak zorunda).
- Uygulamanın ana ekranı artık gerçek görev verisini (araçların zaman içindeki hareket kayıtlarını)
  gösteriyor.
- Arayüz responsive hale getirildi — pencere küçültülse bile hiçbir buton kaybolmuyor, düzen
  kendini otomatik olarak yeniden düzenliyor.

### 2026-08-03 15:41 — Yeni Kayıt Ekleme ve Silme

Ana ekrana, adım adım açılan bir "yeni araç hareketi ekle" akışı eklendi: önce araç seçiliyor,
sonra tarih (bugün önerilir, değiştirilebilir), sonra hız, en son kilometre sayacı. Kilometre
sayacı girilirken sistem, o aracın o tarihe en yakın önceki ve sonraki kayıtlarını otomatik bulup
girilen değerin bu ikisi arasında, gerçekçi kalmasını sağlıyor — böylece kilometre sayacı asla
zaman içinde geriye gitmiyor. Kayıt silme de eklendi.

### 2026-08-03 16:10 — Filtreleme ve Arayüz İyileştirmeleri

Ana ekrandaki listeye bir filtre eklendi — plaka, tarih, hız veya kilometre değerine göre listeyi
daraltmak artık mümkün. Ayrıca arayüz, pencere boyutundan bağımsız olarak her zaman doğru
görünecek şekilde yeniden yapılandırıldı.

---

## 2026-08-03 16:56 — İlk Demo ve Yeni Yol Haritası

Bugüne kadar tamamlanan sistem (araç hareketleri listesi, filtreleme, ekleme/silme, çoklu araç
raporu) Seyir Mobil'den Eren bey'e gösterildi — projenin ilk demosu. Demo sonrası alınan geri
bildirimler doğrultusunda, sıradaki çalışma sırası netleşti:

1. **Detaylı Rapor** — mevcut rapor ekranına, seçilen tarih aralığında gün gün kilometre artışını
   gösteren daha ayrıntılı bir mod eklenecek.
2. **Web uygulaması** — masaüstündeki tüm özellikleri kapsayan, tek sayfalık bir web arayüzü
   geliştirilecek. Önce temel işlevsellik tamamlanacak, görsel/arayüz geliştirmesi ayrı bir
   aşamada ele alınacak.
3. **Arayüz geliştirme** — web altyapısı tamamlandıktan sonra, kurumun önerdiği hazır bileşen
   kütüphaneleriyle (DevExtreme ve muhtemelen Angular) görsel kalite artırılacak.
4. **Docker** — sistemin tek bir komutla kurulup çalıştırılabilmesi için konteynerleştirme, en
   sonda ayrı bir aşamada ele alınacak.

---

## 2026-08-04 08:10 — Detaylı Rapor Özelliği Tamamlandı

İlk demoda alınan geri bildirimin ilk maddesi hayata geçirildi: "Araç Hareket Raporu" ekranına
**"Detaylı Rapor (gün gün)"** seçeneği eklendi. İşaretlendiğinde, seçilen tarih aralığındaki her
gerçek okuma tek tek listeleniyor ve her satır bir öncekine göre ne kadar kilometre yapıldığını
gösteriyor — mevcut özet raporun (başlangıç/bitiş/toplam) yanında, daha ayrıntılı bir alternatif
olarak sunuluyor.

Ayrıca, önceki demoda gündeme gelen iki açık soru netleşti:
- Web arayüzünün **Angular** ile geliştirileceği doğrulandı.
- **DevExtreme** (önerilen hazır arayüz bileşen kütüphanesi) ücretsiz olmadığı araştırıldı ve
  doğrulandı — kurumla lisans konusunun görüşülmesi gerekiyor.

---

## 2026-08-04 09:08 — Web Uygulaması Yayında: Tüm Özellikler Angular'a Taşındı

Kullanıcının Detaylı Rapor özelliğini onaylamasının ardından, masaüstündeki tüm işlevleri kapsayan
bir web arayüzü geliştirildi — araç hareketleri listesi ve filtreleme, yeni kayıt ekleme sihirbazı,
kayıt silme, ve rapor ekranı (özet ve detaylı mod ikisi de). Web arayüzü aynı backend API'sini
kullanıyor; ikinci bir API yazılmadı.

Bu aşamada amaç görsel tasarım değil, işlevsel eksiksizlik — masaüstünde yapılabilen her şeyin
web'de de yapılabilmesi. Görsel/arayüz geliştirmesi (kurumun önerdiği DevExtreme bileşen
kütüphanesiyle) bir sonraki aşamada ayrıca ele alınacak; o aşamada kurumsal lisans netleşene kadar
kişisel bir deneme hesabıyla ilerlenecek.

---

## 2026-08-04 09:52 — Sayfalama ve Excel'e Aktarma (Web + Masaüstü)

Kullanıcı geri bildirimiyle: araç hareketleri listesine sayfalama (sayfa başına gösterilecek kayıt
sayısı seçilebiliyor) ve Excel'e aktarma özelliği eklendi — hem web hem masaüstü uygulamasında.
Bir filtre uygulanmışsa, sadece o filtreye uyan kayıtlar Excel'e aktarılıyor. Rapor ekranında da
benzer şekilde Excel'e aktarma eklendi; kullanıcı isterse her aracı kendi başlığı altında ayrı bir
bölüm olarak, isterse tüm araçları tek bir tabloda dışa aktarabiliyor.

Excel dosyaları backend'de üretiliyor, böylece web ve masaüstü aynı mantığı paylaşıyor — ikisi de
aynı sonucu üretiyor.

---

## 2026-08-04 13:20 — Giriş (Login) Sisteminin Temelleri Atıldı

Daha önce kurumla konuşulmuş olan kullanıcı girişi (login), oturum ve yetkilendirme sisteminin
**backend altyapısı** kuruldu: kullanıcı hesapları (kullanıcı adı, şifre, rol — Admin/Viewer)
artık veritabanında tutuluyor, şifreler güvenli şekilde şifrelenmiş (hash'lenmiş) olarak
saklanıyor, girişte güvenli bir oturum anahtarı (token) üretiliyor.

Mevcut ekranlar (araç hareketleri listesi, raporlar) bu aşamada **değişmedi ve kilitlenmedi** —
masaüstü ve web arayüzü hâlâ olduğu gibi çalışıyor. Giriş ekranlarının kendisi (masaüstünde ve
web'de) ve mevcut ekranların girişe kilitlenmesi, bir sonraki aşamada ele alınacak.

---

## 2026-08-04 14:01 — Oturum Zaman Aşımı: Hareketsizlikte Otomatik Çıkış

Giriş sistemine bir güvenlik katmanı daha eklendi: kullanıcı sistemde bir süre (5 dakika) hiçbir
işlem yapmazsa oturumu otomatik olarak sona eriyor; sistemi aktif kullandığı sürece (herhangi bir
ekranı açması, bir işlem yapması) oturumu kendiliğinden yenileniyor — ayrıca bir şey yapmasına
gerek yok. Ayrıca kullanıcının isteğe bağlı olarak oturumu anında sonlandırabilmesi (çıkış yapma)
için bir uç nokta eklendi.

---

## 2026-08-04 14:31 — Giriş Ekranları Yayında: Web ve Masaüstünde Login Zorunlu

Giriş sistemi artık uçtan uca çalışıyor. Hem web hem masaüstü uygulamasına bir giriş ekranı
eklendi; sisteme erişebilmek için önce kullanıcı adı ve şifreyle giriş yapmak gerekiyor. Web'de
oturum bilgisi tarayıcıda saklanıyor (sayfa yenilense de kaybolmuyor); masaüstünde "Beni Hatırla"
seçeneği işaretlenirse bir sonraki açılışta otomatik giriş yapılıyor, işaretlenmezse her açılışta
tekrar giriş istenir. Her iki arayüzde de bir "Çıkış Yap" seçeneği var.

Bu adımla birlikte, daha önce herkese açık olan tüm ekranlar (araç hareketleri, raporlar) artık
sadece giriş yapmış kullanıcılara açık.

---

## 2026-08-04 15:49 — Kurumdan İlk Detaylı Geri Bildirim: Tasarım Aşamasına Geçildi

Seyir Mobil'den Eren bey, güncel sistemi (giriş ekranları dahil) test edip ilk kez detaylı bir geri
bildirim verdi: işlevsellik ve mimari (masaüstü ve web'in aynı backend'i paylaşması) beğenildi.
Ana geri bildirim, arayüzün şu an temel HTML bileşenleriyle yapılmış olması ve daha profesyonel,
hazır bir bileşen kütüphanesiyle (DevExtreme) geliştirilmesi gerektiği yönünde — giriş/tarih
seçiciler, menü yapısı gibi alanlar özellikle işaret edildi.

Bu geri bildirim doğrultusunda, yeni özellik eklemeye ara verilip **web arayüzünün görsel/UX
kalitesini artırma aşamasına** geçiliyor.

---

## 2026-08-04 16:02 — DevExtreme Entegrasyonu Başladı: İlk Bileşen Canlıda

Kurumun önerdiği DevExtreme bileşen kütüphanesi projeye eklendi (kayıt gerektirmeyen 30 günlük
deneme sürümüyle). İlk uygulama: rapor ekranındaki başlangıç/bitiş tarih seçicileri, tek ve daha
akıcı bir "tarih aralığı" seçiciyle değiştirildi. Geri kalan ekranlar (liste, menü yapısı) aynı
yaklaşımla adım adım güncellenecek.

---

## 2026-08-04 16:51 — Ana Liste Ekranı Gelişmiş Tabloya Kavuştu

Araç hareketleri listesi artık DevExtreme'in gelişmiş tablo bileşeniyle çalışıyor: sütun
başlıklarına tıklayarak sıralama, serbest metin arama ve sayfalama artık hazır ve daha akıcı.

---

## 2026-08-04 17:11 — DevExtreme ile Uçtan Uca Arayüz Yenilemesi Tamamlandı

Web uygulamasının tüm ekranları (giriş, araç hareketleri listesi, rapor) kurumun önerdiği
DevExtreme bileşen kütüphanesiyle yeniden tasarlandı — form alanları, seçiciler, butonlar artık
tutarlı, profesyonel bir görünüme sahip. Bu süreçte, önceden elle yazılmış bazı özel arayüz
parçaları (plaka arama/seçim kutusu gibi) tamamen kaldırılıp DevExtreme'in hazır bileşenleriyle
değiştirildi — kod hem sadeleşti hem de daha tutarlı bir kullanıcı deneyimi sağladı.

---

## 2026-08-05 13:21 — Vehicles Tablosu ve Kullanılmayan CRUD Kaldırıldı

Projenin ilk/alıştırma tablosu olan `Vehicles` (araç ID, plaka, toplam kilometre) hiçbir istemci
tarafından kullanılmıyordu — masaüstü ve web tamamen `AracHareketleri`'ne geçtiğinden beri arka
planda atıl duruyordu. Backend'deki `Vehicles` uç noktaları (`GET/POST/DELETE /api/vehicles`),
ilgili model ve `DbContext` kaydı kaldırıldı; veritabanındaki tablo yeni bir migration script'iyle
(`006_drop_vehicles_table.sql`) silindi. Önceki oluşturma script'leri (`001`, `002`) geçmiş kaydı
olarak saklandı, sadece yeni script'le geri alındı.

---

## 2026-08-05 16:00 — Eren Bey ile İkinci Geri Bildirim Toplantısı

Eren bey ile 16:00-16:20 arası, mülakat tarzında (soru-cevap şeklinde) bir toplantı yapıldı.
Toplantıda altı ayrı konu gündeme geldi: oturumun sunucu yeniden başlatılınca düşmemesi için
kalıcı bir oturum takibi, bildirim/uyarı ekranlarının daha modern bir kütüphaneyle (SweetAlert)
yenilenmesi, Excel'e aktarma sisteminin sadeleştirilmesi, Excel'den toplu veri içe aktarma, rol
bazlı (yönetici/görüntüleyici) bir yönetici paneli ve kapsamlı bir kullanıcı hareket günlüğü
(Graylog ile). Notlar toplantı sırasında alındı, sonraki adımlarda düzenlenecek.

---

## 2026-08-05 16:45 — Toplantı Sonrası: Rapor Ekranı Grid'leri DevExtreme'e Taşındı, Kısa Araştırma

Ofisten çıkmadan önce (17:00), toplantıda gündeme gelen konulardan birkaçı (SweetAlert, Graylog
gibi) hakkında kısa bir ön araştırma yapıldı. Aynı zamanda rapor ekranındaki iki sonuç tablosu
(özet ve gün-gün detaylı rapor) da DevExtreme'in gelişmiş tablo bileşenine taşındı — web
arayüzünde artık hiçbir yerde eski tip düz HTML tablo/form elemanı kalmadı, tamamı tutarlı bir
bileşen kütüphanesi üzerinden geliyor.

---

## 2026-08-06 09:16 — Toplantı Notları Temize Çekildi, Roadmap ve Uygulama Sırası Netleşti

Toplantıdan alınan ham notlar düzenlenip yerel bir çalışma dosyasına yazıldı, ardından altı
maddenin her biri netleştirildi: oturum kalıcılığı için SQL Server'da yeni bir tablo (Redis
DEĞİL — artı/eksileri karşılaştırılıp karar verildi), Excel'e aktarmanın hem ana listede hem
rapor ekranında DevExtreme'e taşınması, kullanıcı hareket günlüğü için gerçek Graylog altyapısı
(bu vesileyle Docker konusu da gündeme alındı) ve Excel'den toplu veri içe aktarmanın hem web hem
masaüstünde geliştirilmesi. Uygulama sırası birlikte kararlaştırıldı, ilk adıma (oturum
kalıcılığı) başlamadan önce onay bekleniyor.

---

## 2026-08-06 09:54 — Oturum Bilgisi Artık Kalıcı: SQL Server'da Yeni Bir Tablo

Yeni roadmap'in ilk maddesi tamamlandı. Daha önce kullanıcı oturumları sunucunun belleğinde
tutuluyordu — bu da API her yeniden başlatıldığında (bakım, güncelleme, çökme gibi durumlarda)
tüm kullanıcıların anında oturumdan atılıp yeniden giriş yapmak zorunda kalması anlamına
geliyordu. Şimdi oturum bilgisi veritabanında yeni bir tabloda tutuluyor; API yeniden başlasa
bile geçerli bir oturumu olan kullanıcılar sisteme giriş yapmaya devam edebiliyor. Gerçek bir
yeniden başlatma testiyle doğrulandı.

---

## 2026-08-06 10:35 — Bildirim/Uyarı Ekranları SweetAlert ile Yenilendi

Roadmap'in ikinci maddesi tamamlandı. Web arayüzündeki tüm tarayıcının kendi (sade, tek tip)
uyarı/onay pencereleri kaldırıldı; yerine tutarlı, ikonlu ve markaya uygun görünen SweetAlert
pencereleri geldi — hata, bilgilendirme ve "silmek istediğinize emin misiniz?" onayı için ayrı
ayrı tasarlanmış üç görünüm.

---

## 2026-08-06 10:44 — Web'de Excel'e Aktarma Sadeleşti

Roadmap'in üçüncü maddesi tamamlandı. Web arayüzündeki "Excel'e Aktar" artık tablo bileşeninin
kendi yerleşik özelliği üzerinden, tamamen tarayıcıda çalışıyor — ayrı bir buton veya sunucu
isteği gerekmiyor. Rapor ekranındaki eski "her plaka için ayrı bölüm mü, tek tabloda mı"
seçeneği bu geçişle kayboldu, artık tek ve tutarlı bir düzen kullanılıyor. Masaüstü uygulamasının
Excel'e aktarma özelliği bu değişiklikten etkilenmedi, eskisi gibi çalışmaya devam ediyor.

---

## 2026-08-06 11:02 — Yönetici Paneli ve Role Göre Karşılama Ekranı

Roadmap'in dördüncü maddesi tamamlandı. Artık ayrı bir Yönetici Paneli var: kullanıcı hesapları
buradan görüntülenip yeni hesap eklenebiliyor ve silinebiliyor — bu özellik backend'de bir süredir
hazırdı ama şimdiye kadar hiçbir ekranı yoktu. Ayrıca giriş yapan kullanıcı, rolüne göre farklı bir
ekranla karşılanıyor artık: yöneticiler doğrudan Yönetici Paneli'ne, diğer kullanıcılar mevcut ana
ekrana yönleniyor. Bu yönlendirme, ileride yeni roller eklendiğinde kolayca genişleyebilecek bir
yapıda kuruldu.

---

## 2026-08-06 12:01 — Sistem Docker'a Taşındı, Merkezi Log Takibi Eklendi

Roadmap'in beşinci ve en kapsamlı maddesi tamamlandı — 6 maddelik listenin sonuncusu hariç
hepsi bitti. Sistemin tüm parçaları (veritabanı, backend, web) artık tek bir komutla ayağa
kalkan konteynerler halinde çalışıyor; bu, kurulumu ve dağıtımı önemli ölçüde basitleştiriyor.
Ayrıca merkezi bir log takip sistemi (Graylog) devreye alındı: backend'e gelen her istek ve web
arayüzündeki her tıklama artık kaydediliyor — kim, ne zaman, hangi işlemi yaptı sorusuna cevap
verebilecek bir izlenebilirlik katmanı eklendi. Yönetici Paneli'ne bu kayıtları görüntülemek
için Graylog'un kendi arayüzüne giden bir bağlantı eklendi.

---

## 2026-08-06 13:30 — Graylog Kullanımı: Arama, Zaman Dilimi ve Kullanıcı Yönetimi

Öğle arasının ardından, yeni devreye alınan log takip ekranı (Graylog) birlikte incelendi ve
gerçek kullanım üzerinden birkaç konu netleşti: arama kutusunun kendi sözdizimi kuralları,
saatlerin doğru dilimde (İstanbul, UTC+3) gösterilmesi için yapılan ayar ve günlük kullanım için
yerleşik yönetici hesabından ayrı, düzenlenebilir bir kullanıcı hesabı açılması. Bu sayede log
arama ekranı artık rahatça günlük kullanıma hazır.

---

## 2026-08-06 15:00 — Excel'den Toplu Veri Girişi: Roadmap'in Son Maddesi de Tamamlandı

6 maddelik yol haritasının tamamı bitti. Artık hem web hem masaüstü uygulamasında bir Excel
dosyası yüklenerek onlarca araç hareketi tek seferde sisteme aktarılabiliyor. Yüklenen dosya
önce ayrıntılı şekilde kontrol ediliyor (geçersiz plakalar, tarihler, tutarsız kilometre
değerleri, sistemde zaten var olan kayıtlarla çakışma) ve kullanıcıya düzenlenebilir bir önizleme
sunuluyor — hatalı satırlar düzeltilebiliyor, çakışan kayıtlar için "üzerine yaz" veya "atla"
seçilebiliyor. Sistemde henüz kayıtlı olmayan bir plaka görülürse otomatik olarak yeni bir araç
olarak ekleniyor, aynı yazım hatasının yanlışlıkla birden fazla araç oluşturmasını önleyen bir
kontrol de bu sırada devrede.

---

## 2026-08-07 10:23 — Excel İçe Aktarma Ekranında Kullanıcı Deneyimi İnce Ayarları

Bir önceki gün tamamlanan Excel içe aktarma özelliği, gerçek kullanım sırasında bulunan birkaç
pürüzle cilalandı: dosya önizlemesinin bazı durumlarda boş görünmesine yol açan bir hata giderildi,
dosya seçme/şablon indirme butonlarının hizası düzeltildi ve iş kuralı netleştirildi — "atla"
olarak işaretlenen bir satırın içeriği hatalı olsa bile artık içe aktarmanın tamamını
engellemiyor, sadece o satır atlanıyor. Ayrıca ekrandaki sabit bilgi kutusu kaldırılıp geçersiz
plaka hata mesajı, plaka kuralını tam olarak anlatacak şekilde zenginleştirildi.

---

## 2026-08-07 11:33 — 1000 Satırlık Gerçekçi Veriyle Stres Testi, Gerçek Bir Rapor Hatası Bulundu

Sistem, 40 araca yayılan 1000 satırlık gerçekçi bir veri kümesiyle test edildi. Bu test sırasında
özet rapor hesaplamasında gerçek bir hata ortaya çıktı: bir aracın seçilen tarih aralığında hiç
kaydı yokken öncesinde/sonrasında kayıtları varsa, rapor negatif kilometre ve ters tarih sırasıyla
yanlış bir sonuç üretiyordu. Kök neden bulunup düzeltildi ve hem hatalı hem normal senaryolarla
doğrulandı. Aynı gün, ana ekrandaki filtreleme de genişletildi — artık tek bir araç ve tek bir gün
yerine, aynı anda birden fazla araç ve bir tarih aralığı seçilerek filtreleme yapılabiliyor
(rapor ekranındaki filtreleme mantığıyla tutarlı).

---

## 2026-08-07 17:34 — Kurum Geri Bildirimi Üzerine Web Arayüzünün Tasarım Dili Yenilendi

Kurumdan gelen geri bildirim netti: sistemin işlevselliği ve tutarlılığı beğenilmişti, ama genel
görünüm ilk bakışta "şablon"/yapay zeka ile üretilmiş bir arayüz hissi veriyordu. Bu geri bildirim
üzerine web arayüzünün tamamı, aynı renk paletini koruyarak ama kompozisyonu ve bileşen dilini
değiştirerek yeniden tasarlandı:

- **Giriş ekranı** ortalanmış tek bir kart yerine, ekranın tamamını kullanan asimetrik bir
  bölünmeye kavuştu — bir tarafta filonun farklı lokasyonlara dağılan araçlarını temsil eden bir
  rota grafiği, diğer tarafta giriş formu.
- **Tüm ekranlarda** (Araç Hareketleri, Araç Hareket Raporu, Excel İçe Aktar, Admin Paneli), her
  bölümü saran yuvarlak köşeli/gölgeli "kart" görünümü kaldırıldı, yerine daha sade, düz bir bölüm
  düzeni geldi.
- **Ana liste ekranında** filtreleme alanları ve tüm işlem butonları (Filtrele, Temizle, Yenile,
  Sil, Excel'den Veri Aktar) tek bir satırda toplandı — önceden iki ayrı bölümde, farklı
  hizalarda duruyorlardı.
- Buton ve giriş alanlarının görünümü, tablo sütun genişlikleri ve genel renk tonları gerçek
  kullanım sırasında bulunan çok sayıda küçük pürüzle (metnin kenara çok yakın olması, bazı
  sütunların içeriği kesmesi, başlıkla tablo arasındaki boşluk gibi) birlikte ince ayarlandı ve
  gerçek bir tarayıcıda uçtan uca doğrulandı.

Masaüstü uygulaması bu turun kapsamı dışında, native görünümünü koruyor.

---

## 2026-08-10 11:40 — Güvenlik: GitHub'a Sızmış Gerçek Bilgiler Tespit Edildi ve Giderildi

Yayınlanan proje dosyaları arasında, Docker yapılandırması ve API ayarlarında gerçek, düz metin
şifre/anahtar bilgisi bulunduğu fark edildi. Sızan tüm bilgiler (veritabanı yönetici şifresi, log
sistemi şifreleri, oturum doğrulama anahtarı) canlı sistem üzerinde derhal değiştirildi ve
değişikliğin gerçekten devreye girdiği doğrulandı. Projenin gizli bilgi yönetimi biçimi de kalıcı
olarak değiştirildi — artık hiçbir gerçek şifre/anahtar GitHub'a giden dosyalarda düz metin olarak
durmuyor, bunun yerine yerel ve GitHub'a hiç gitmeyen bir yapılandırma dosyasından okunuyor.

---

## 2026-08-10 11:53 — Proje Geçmişi Temizlendi: Yeni, Temiz Bir GitHub Deposu

Sızan bilgilerin GitHub'ın geçmiş kayıtlarında iz bırakmadığından tamamen emin olmak için proje
deposu sıfırdan, tek ve temiz bir başlangıç noktasıyla yeniden oluşturuldu. Eski depo (sızıntı
geçmişini taşıdığı için) herkese kapatıldı. Yeni depoda katkı sağlayan tek hesabın proje sahibinin
kendi hesabı olduğu ayrıca doğrulandı.

---

## 2026-08-10 13:09 — Çalışma Kuralı Güncellendi: Sürüm Kontrolü Artık Onaylı Şekilde Yürütülüyor

Projenin çalışma kuralları güncellendi: sürüm kontrol (git) işlemleri artık, önce ne yapılacağı
açıkça anlatılıp onay alındıktan sonra, her zaman projenin sahibinin kendi hesabı/kimliğiyle
yürütülüyor.

---

## 2026-08-10 15:02 — Arayüz İnce Ayarları: Giriş Alanları, Plaka Filtresi ve Hizalama Düzeltmeleri

Web arayüzünde bir dizi kullanılabilirlik ve hizalama düzeltmesi yapıldı:

- Tüm ekranlardaki giriş alanları ve butonlar (yükseklik, checkbox hizası) daha rahat kullanılabilir
  hale getirildi.
- **Plaka filtresi** (Araç Hareketleri ve Araç Hareket Raporu ekranları) yeniden tasarlandı: seçilen
  plakalar artık giriş kutusunun kendi içinde değil, altında ayrılmış, taşmaya duyarlı bir alanda
  (en fazla 20 "baloncuk", fazlası "+N daha" olarak özetleniyor) gösteriliyor; giriş kutusunun
  kendisi ise sadece kaç plaka seçildiğini gösteren bir sayı gösteriyor. Bu alanın sağ kenarı da
  artık her iki ekranda ilgili son alanla (Araç Hareketleri'nde Km Sayacı, Araç Hareket Raporu'nda
  Rapor Oluştur butonu) tam hizalı.
- Plaka arama kutusundaki yer tutucu metninin ("Tümü", "Plaka ara...") dikey ortalanmama sorunu
  giderildi.

Bu turda, ilk denenen bir arayüz değişikliğinin (açılır listenin çalışmaz hale gelmesi) gerçek
kullanım sırasında fark edilip anında geri alındığı bir düzeltme de yaşandı — arayüz artık hem
görsel hem işlevsel olarak beklendiği gibi çalışıyor.

---

## 2026-08-10 16:48 — Plaka Filtresinde Kök Neden Düzeltmesi, Excel İçe Aktarma İnce Ayarı ve Graylog Sistem Panosu

Öğleden sonranın devamında üç ayrı iş tamamlandı:

- **Plaka filtresi — kalıcı kök neden düzeltmesi.** Bir önceki turda "düzeltildi" denen yer tutucu
  ortalama sorunu (Araç Hareketleri ve Araç Hareket Raporu ekranlarında "Tümü"/"Plaka ara..."
  metninin üst kenara yakın durması) gerçekte tam çözülmemişti. Kullanılan arayüz bileşen
  kütüphanesinin kendi iç davranışı incelenerek asıl neden bulundu: kutunun görünen (40px)
  yüksekliği ile içindeki gerçek içerik kutusunun yüksekliği arasında bir uyumsuzluk vardı — dış
  kutu büyümüş görünüyordu ama iç kutu büyümemişti, metin de o küçük iç kutuya göre ortalandığı
  için üstte kalıyordu. İç kutuya doğrudan bir minimum yükseklik verilerek kalıcı olarak
  düzeltildi. Aynı incelemede, seçili plaka sayısını gösteren göstergenin ("10 seçildi" gibi)
  kutunun solunda arama yazısıyla üst üste bindiği fark edildi, sağ kenara taşındı.
- **Excel İçe Aktarma ekranında küçük bir düzeltme.** Önizleme tablosundaki satır numarası
  sütunu, 3 haneli sayılara zor sığıyordu; artık gerektiğinde otomatik genişleyen, esnek bir
  genişliğe geçirildi.
- **Graylog'da "Seyir Mobil - Sistem Panosu" adında yeni bir sistem izleme panosu (dashboard)
  oluşturuldu — 8 grafik/tablo widget'ı içeriyor:** istek hacmi (zaman içinde), en çok kullanılan
  uç noktalar, HTTP durum kodu dağılımı, ortalama yanıt süresi, en yavaş 10 istek, kullanıcı bazlı
  aktivite, rol dağılımı (Admin/Viewer), en çok tıklanan sayfa/eylem. Bu pano, aylar önce kurulan
  merkezi log altyapısının (backend'in her API isteğini ve web'deki her tıklamayı Graylog'a
  kaydetmesi) üzerine kurulan ilk görsel izleme katmanı — sistemin gerçek kullanım verisiyle
  test edildi, doğru sonuçlar üretti.

---

## 2026-08-11 15:19 — Yeni Geri Bildirim Turu: Şifre Sıfırlama, Çoklu Dil, Tasarım İnce Ayarı

Kurumdan (Eren bey), bir önceki gün paylaşılan Graylog panosu ve tasarımın son haline dört
maddelik yeni bir geri bildirim geldi. Genel değerlendirme olumlu ("Graylog tarafı oldukça iyi
olmuş") — aksiyon gerektiren dört madde ve uygulama sırası netleşti:

1. **Login ekranına "Şifremi Unuttum" akışı** eklenecek.
2. **E-posta ile şifre sıfırlama** — kullanıcı, kayıtlı e-posta adresine gelen bir bağlantıyla
   yeni şifre belirleyebilecek. Geliştirme aşamasında gerçek kullanıcılara mail gitmeyen, test
   amaçlı ücretsiz bir e-posta servisi kullanılacak; ileride gerçek bir mail sunucusuna geçiş
   kolay olacak şekilde kurulacak.
3. **Çoklu dil desteği** — kademeli bir süreç olarak, önce altyapısı kurulacak, ilk hedef dil
   İngilizce olacak.
4. **Arayüzün "yapay zeka ile üretilmiş" hissini kırma çalışması devam edecek** — özellikle yazı
   tipi (font) seçimi üzerinde durulacak, birkaç seçenek değerlendirilip birlikte karar
   verilecek.

Sıradaki adım: 1. ve 2. maddeyle (şifre sıfırlama akışı) başlanması.

---

## 2026-08-11 15:36 — Şifre Sıfırlama Özelliğinin İlk Adımı: Kullanıcı Kayıtlarına E-posta Eklendi

Şifre sıfırlama özelliğinin çalışabilmesi için önce kullanıcı kayıtlarının bir e-posta adresi
taşıması gerekiyordu — bu adım tamamlandı. Veritabanındaki kullanıcı tablosuna yeni bir e-posta
alanı eklendi (mevcut kullanıcılar etkilenmedi), Yönetici Paneli'ndeki "Yeni Kullanıcı Ekle"
formuna e-posta girişi eklendi ve artık yeni bir kullanıcı oluşturulurken e-posta adresi zorunlu.
Değişiklik gerçek veritabanı üzerinde uygulanıp test edildi.

---

## 2026-08-11 16:19 — "Şifremi Unuttum" Özelliği Tamamlandı

Giriş ekranına "Şifremi Unuttum" bağlantısı eklendi. Artık bir kullanıcı şifresini unutursa,
kayıtlı e-posta adresini girerek bir sıfırlama bağlantısı isteyebiliyor; bu bağlantıyla açılan
sayfada yeni bir şifre belirleyip tekrar giriş yapabiliyor. Sıfırlama bağlantısı güvenlik için
1 saat geçerli ve yalnızca bir kez kullanılabiliyor. Sistem, bir e-posta adresinin kayıtlı olup
olmadığını belli etmeyecek şekilde tasarlandı — bu da hesap bilgisi sızdırmayı önleyen bilinen
bir güvenlik pratiği.

Mail gönderimi için altyapı hazır (test amaçlı, gerçek kullanıcılara mail göndermeyen bir servis
üzerinden çalışacak şekilde kuruldu) — gerçek bir e-posta hesabı bağlanana kadar sistem sıfırlama
bağlantısını üretmeye devam ediyor, sadece mail gönderimi devre dışı kalıyor, başka hiçbir şeyi
etkilemiyor.

Bu çalışma sırasında, Yönetici Paneli'nde daha önce giriş yapmış bir kullanıcıyı silmenin hataya
yol açtığı fark edildi ve aynı gün içinde düzeltildi — artık bir kullanıcı silindiğinde aktif
oturumu da birlikte sonlandırılıyor.

---

## 2026-08-11 16:33 — Çoklu Dil Desteğinin Altyapısı Kuruldu

Uygulamanın birden fazla dilde kullanılabilmesi için gerekli altyapı hazırlandı ve üst menüde
bir dil seçici (TR/EN) eklendi. Kademeli bir yaklaşım benimsendi — kurumun "yavaş yavaş dil
ekleyelim" isteğine uygun olarak, bu turda sadece üst menüdeki ekran başlıkları gerçekten
İngilizce'ye çevrildi; sistemin geri kalanı önümüzdeki günlerde aynı yöntemle adım adım
çevrilecek. Seçilen dil tercihi tarayıcıda hatırlanıyor, bir sonraki ziyarette otomatik olarak
uygulanıyor.

---

## 2026-08-12 10:36 — "Şifremi Unuttum" Gerçek Mail ile Uçtan Uca Doğrulandı

Daha önce tamamlanan şifre sıfırlama akışı, bu kez gerçek bir SMTP servisi (Mailtrap, test amaçlı
bir e-posta sağlayıcısı) üzerinden baştan sona denendi: e-posta adresi girildi, sıfırlama maili
gerçekten gönderildi, gelen bağlantıyla yeni şifre belirlendi ve yeni şifreyle giriş yapılabildiği
doğrulandı. Daha önceki testler sıfırlama bağlantısını doğrudan veritabanından okuyarak
yapılıyordu — artık gerçek mail gönderimi de çalıştığı kanıtlanmış oldu.

---

## 2026-08-12 11:11 — Çoklu Dil Desteği Tüm Ekranlara Yayıldı

Daha önce sadece üst menüde çalışan çoklu dil (Türkçe/İngilizce) desteği, artık uygulamanın
tamamında geçerli: giriş ekranı, şifre sıfırlama akışı, ana liste ekranı, rapor ekranı, Excel içe
aktarma ekranı ve yönetici paneli — hepsi dil seçiciyle birlikte anında değişiyor (tablo başlıkları,
buton/etiket metinleri, hata ve durum mesajları dahil). Dil seçimi tarayıcıda hatırlanmaya devam
ediyor.

---

## 2026-08-12 13:51 — Çoklu Dil Desteği Backend'e de Yayıldı, Bir Oturum Açma Sorunu Giderildi

Bir önceki adımda tüm ekranlara yayılan çoklu dil desteği test edilirken gerçek bir hata ortaya
çıktı: uygulama açılışında bir bağımlılık çakışması nedeniyle hem çeviriler yüklenemiyor hem de
giriş yapılamıyordu. Kök neden bulunup düzeltildi, sistem yeniden test edilip doğrulandı.

Ardından, içe aktarma ekranındaki doğrulama hata mesajlarının (örn. "kilometre sayacı şu değerden
büyük olmalı" gibi satır bazlı uyarılar) İngilizce arayüzde bile Türkçe kaldığı fark edildi — bu
mesajlar sunucu tarafında üretiliyordu ve önceki tur sadece arayüz metinlerini kapsıyordu. Bu
eksik giderildi: artık sunucu tarafındaki tüm doğrulama ve hata mesajları da seçili dile göre
(Türkçe/İngilizce) geliyor — şifre sıfırlama e-postasının içeriği dahil.

---

## 2026-08-12 14:19 — Giriş Ekranlarına Dil Seçici Eklendi, İnce Ayarlar Yapıldı

Giriş, şifremi unuttum ve şifre sıfırlama ekranlarında üst menü gizli olduğu için dil
değiştirilemiyordu — bu üç ekrana da kendi dil seçicileri eklendi. Test sırasında bulunan iki
küçük sorun da giderildi: hızlı dil değişimlerinde arka planda bir adım geriden gelen dil bilgisi
düzeltildi, ve içe aktarma ekranındaki durum etiketlerinin İngilizce'de kesilmesi önlendi.

---

## 2026-08-12 15:18 — Roadmap Madde 3 Başladı: Yeni Yazı Tipleri ve Giriş Ekranının Yenilenmesi

Kurumun "yapay zeka ile üretilmiş hissi" geri bildirimini gidermek için üzerinde çalışılan
son madde başladı. Kullanıcı, ücretsiz font kütüphanelerini kendisi inceleyip **Clash Display**
(başlıklar) ve **Clash Grotesk** (gövde metni) çiftine karar verdi; siteye entegre edildi.

Ardından giriş ekranı yeniden tasarlandı: sayfa aynalandı (marka paneli artık sağda, giriş
formu solda), "Seyir Mobil" logosu büyütülüp yeni fontla iki satırlık, göze çarpan bir biçimde
yeniden düzenlendi. Şifremi Unuttum ve Şifre Sıfırlama ekranları da (aynı görsel kimliği
paylaştıkları için) aynı şekilde güncellendi.

---

## 2026-08-12 17:00 — Tasarım Kararı İçin Karşılaştırma Sayfası Hazırlandı

Yeni giriş ekranı tasarımı üzerine hangi görsel stilin benimseneceğine karar verebilmek için
geçici bir karşılaştırma sayfası hazırlandı: mevcut kurumsal kimlik (logo ve rota grafiği)
korunarak, aynı formun 6 farklı kart/görünüm stili (düz koyu, yalın/flat, Material tasarım,
minimalist, neo-brütalist, koyu neumorfizm) yan yana denenip karşılaştırılabiliyor. Bu, kalıcı
bir özellik değil — nihai tasarım kararı verildiğinde kaldırılacak bir çalışma aracı.

Gün burada tamamlandı, tasarım kararı yarın netleştirilecek.

---

## 🔜 Sıradaki Adımlar

- [x] Web istemcisinin eklenmesi (aynı backend API üzerinden)
- [x] Sayfalama ve Excel'e aktarma (web + masaüstü)
- [x] Giriş (login) sistemi — backend altyapısı
- [x] Oturum zaman aşımı (hareketsizlikte otomatik çıkış)
- [x] Giriş (login) sistemi — masaüstü ve web arayüzü
- [x] UI/UX iyileştirmeleri (DevExtreme ile) — tüm web ekranları güncellendi (grid'ler dahil)
- [x] Oturum bilgisinin kalıcı (SQL Server tablosu) tutulması
- [x] Bildirim/uyarı ekranlarının SweetAlert ile yenilenmesi
- [x] Excel'e aktarmanın DevExtreme üzerinden sadeleştirilmesi (web)
- [x] Yönetici paneli + rol bazlı yönlendirme (web)
- [x] Docker ile konteynerleştirme + kullanıcı hareket günlüğü (Graylog)
- [x] Excel'den toplu veri içe aktarma (web + masaüstü)
- [x] Web arayüzünün tasarım dilinin yenilenmesi (kurum geri bildirimi üzerine)
- [x] Graylog üzerinde sistem izleme panosu (dashboard) kurulması
- [ ] Üst menü/sekme yapısının DevExtreme'e taşınması (isteğe bağlı, henüz yapılmadı)
- [x] "Şifremi Unuttum" akışı + e-posta ile şifre sıfırlama (gerçek mail testiyle doğrulandı)
- [x] Çoklu dil desteği — tüm ekranlar İngilizce/Türkçe arasında geçiş yapabiliyor
- [~] Arayüz tasarımında "yapay zeka hissi"ni kırma çalışması — yeni fontlar seçilip entegre
      edildi, giriş ekranı yeniden tasarlandı; nihai görsel stil kararı için karşılaştırma
      sayfası hazırlandı, karar henüz verilmedi
