namespace CO.CDP.EntityFrameworkCore.Timestamps;

public interface IEntityDate
{
    DateTime CreatedOn { get; set; }
    DateTime UpdatedOn { get; set; }
}