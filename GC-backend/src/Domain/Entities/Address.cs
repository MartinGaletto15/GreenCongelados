using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums.Entities;

namespace Domain.Entities;
public class Address
{
    [Key]
    public int IdAddress { get; set; }

    [Required]
    public required int IdUser { get; set; }
    [ForeignKey("IdUser")]
    public required User User { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Street { get; set; }

    [Required]
    public required Cities City { get; set; }

    [Required]
    [MaxLength(10)]
    public required string ZipCode { get; set; }

    [MaxLength(10)]
    public string? Dpto { get; set; }

    public string? References { get; set; }
}