using Microsoft.EntityFrameworkCore;

namespace CO.CDP.EntityVerification.Persistence;

[Index(nameof(PponId), IsUnique = true)]
public class Ppon
{
    public int Id { get; set; }
    public required string PponId { get; set; }
}