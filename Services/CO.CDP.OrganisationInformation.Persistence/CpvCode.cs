using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationInformation.Persistence;

public class CpvCode
{
    [Key]
    [StringLength(20)]
    public required string Code { get; set; }

    [StringLength(256)]
    public required string Description { get; set; }
}
