using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeyirMobil.Api.Models;

[Table("AracHareketleri")]
public class AracHareket
{
    [Key]
    public int Id { get; set; }

    public int AracId { get; set; }

    [Required]
    [MaxLength(15)]
    public string AracPlaka { get; set; } = string.Empty;

    public DateOnly VeriTarihi { get; set; }

    public int Hiz { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal KmSayaci { get; set; }
}
