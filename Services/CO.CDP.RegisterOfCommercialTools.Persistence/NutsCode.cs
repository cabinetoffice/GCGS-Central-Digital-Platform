using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.RegisterOfCommercialTools.Persistence;

[Table("commercial_tools_nuts_codes")]
public class NutsCode
{
    [Key]
    [Column("code")]
    [StringLength(10)]
    public required string Code { get; set; }

    [Column("description_en")]
    [StringLength(250)]
    public required string DescriptionEn { get; set; }

    [Column("description_cy")]
    [StringLength(250)]
    public required string DescriptionCy { get; set; }

    [Column("parent_code")]
    [StringLength(10)]
    public string? ParentCode { get; set; }

    [NotMapped]
    public string? ParentDescriptionEn { get; set; }

    [NotMapped]
    public string? ParentDescriptionCy { get; set; }

    [Column("level")]
    public int Level { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("is_selectable")]
    public bool IsSelectable { get; set; } = true;

    [Column("created_on")]
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_on")]
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.UtcNow;

    [ForeignKey(nameof(ParentCode))]
    public NutsCode? Parent { get; set; }

    public ICollection<NutsCode> Children { get; set; } = [];

    [NotMapped]
    public bool HasChildren { get; set; }

    public string GetDescription(Culture culture = Culture.English)
    {
        return culture.IsWelsh() ? DescriptionCy : DescriptionEn;
    }
}