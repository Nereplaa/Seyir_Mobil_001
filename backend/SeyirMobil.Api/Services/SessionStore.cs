using Microsoft.EntityFrameworkCore;
using SeyirMobil.Api.Data;
using SeyirMobil.Api.Models;

namespace SeyirMobil.Api.Services;

// Aktif oturumlarin "son islem zamani"ni SQL Server'daki Sessions tablosunda tutar -
// sliding idle timeout icin. Onceki in-memory (ConcurrentDictionary) halinin yerini alir:
// artik backend yeniden baslatilsa bile (JWT hala gecerliyse) oturumlar hayatta kalir.
public class SessionStore
{
    private readonly SeyirMobilDbContext _db;

    public SessionStore(SeyirMobilDbContext db)
    {
        _db = db;
    }

    public async Task OturumBaslatAsync(string sessionId, int userId)
    {
        _db.Sessions.Add(new Session
        {
            Id = sessionId,
            UserId = userId,
            SonIslemZamani = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    // Oturum hala gecerliyse (bulundu VE bosta kalma suresini asmadiysa) son islem
    // zamanini simdiye guncelleyip true doner - her API cagrisi boylece oturumu yeniler.
    public async Task<bool> DogrulaVeYenileAsync(string sessionId, TimeSpan bostaKalmaSiniri)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null)
        {
            return false;
        }
        if (DateTime.UtcNow - session.SonIslemZamani > bostaKalmaSiniri)
        {
            _db.Sessions.Remove(session);
            await _db.SaveChangesAsync();
            return false;
        }
        session.SonIslemZamani = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task OturumBitirAsync(string sessionId)
    {
        var session = await _db.Sessions.FindAsync(sessionId);
        if (session is not null)
        {
            _db.Sessions.Remove(session);
            await _db.SaveChangesAsync();
        }
    }
}
