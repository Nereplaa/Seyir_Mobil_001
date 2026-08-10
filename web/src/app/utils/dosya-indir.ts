// Backend'den blob olarak gelen bir dosyayi tarayicida "indir" olarak tetikler.
// Gecici bir <a download> elemani olusturup tikliyor, sonra temizliyor -
// ek bir kutuphaneye (file-saver vb.) gerek kalmadan calisan standart yontem.
export function dosyaIndir(blob: Blob, dosyaAdi: string): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = dosyaAdi;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
