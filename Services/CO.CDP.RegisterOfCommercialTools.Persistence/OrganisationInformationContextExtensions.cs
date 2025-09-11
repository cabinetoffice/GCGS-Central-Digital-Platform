using Microsoft.EntityFrameworkCore;

namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public static class OrganisationInformationContextExtensions
{
    public static void ConfigureCpvCodes(this ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CpvCode>();

        entity.HasKey(e => e.Code);

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(8);

        entity.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(250);

        entity.Property(e => e.ParentCode)
            .HasMaxLength(8);

        entity.Property(e => e.Level)
            .IsRequired();

        entity.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        entity.Property(e => e.CreatedOn)
            .IsRequired();

        entity.Property(e => e.UpdatedOn)
            .IsRequired();

        entity.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentCode)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(e => e.ParentCode);
        entity.HasIndex(e => new { e.Code, e.IsActive });
    }
}