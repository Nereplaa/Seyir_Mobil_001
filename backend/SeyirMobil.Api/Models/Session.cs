using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeyirMobil.Api.Models;

[Table("Sessions")]
public class Session
{
    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }

    public DateTime SonIslemZamani { get; set; }

    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
}
