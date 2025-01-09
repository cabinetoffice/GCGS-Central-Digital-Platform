using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
public class Mou : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string FilePath{ get; set; }   
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }    
}