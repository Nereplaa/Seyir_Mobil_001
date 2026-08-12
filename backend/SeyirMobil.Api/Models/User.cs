using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeyirMobil.Api.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Viewer";

    // Nullable: mevcut kullanicilarda (goc oncesi olusturulanlar) email yok.
    // Yeni kullanicilar icin zorunluluk kod tarafinda (Program.cs, POST /api/users).
    [MaxLength(200)]
    public string? Email { get; set; }

    // Sifre sifirlama akisi (feedback_001) - kullanicinin AYNI ANDA en fazla bir aktif
    // sifirlama istegi olabilir, o yuzden ayri bir tablo degil dogrudan alan.
    [MaxLength(200)]
    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
}
