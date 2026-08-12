using System.ComponentModel.DataAnnotations;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Models;

public sealed class ManifestInputModel
{
    [Required]
    [Display(Name = "Manifest JSON")]
    public string Json { get; set; } = "";

    public uint Version { get; set; }
}