namespace CO.CDP.EntityFrameworkCore.Timestamps;

public interface IEntityDate
{
    DateTimeOffset CreatedOn { get; set; }
    DateTimeOffset UpdatedOn { get; set; }
}