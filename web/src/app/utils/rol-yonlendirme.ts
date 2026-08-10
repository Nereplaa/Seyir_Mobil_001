// Her rolun giris sonrasi baslangic ekrani burada, TEK bir yerde tanimli. Eren bey'in acik
// istegi (2026-08-05 toplanti): "admin true ise admine ata, degilse viewer kaliyor" gibi iki
// degerli (binary) bir kisayol DEGIL - roller ilerde cogalabilecegi icin, her rolu ayri ayri
// ele alan bir mekanizma. Yeni bir rol eklendiginde (or. "Editor") tek yapilacak sey bu haritaya
// bir satir eklemek - baska hicbir yerde (login, guard'lar) yeni bir if/else dali GEREKMEZ.
const ROL_BASLANGIC_ROTALARI: Record<string, string> = {
  Admin: '/admin',
  Viewer: '/',
};

const VARSAYILAN_ROTA = '/';

export function rolBaslangicRotasi(rol: string | null): string {
  if (rol && rol in ROL_BASLANGIC_ROTALARI) {
    return ROL_BASLANGIC_ROTALARI[rol];
  }
  return VARSAYILAN_ROTA;
}
