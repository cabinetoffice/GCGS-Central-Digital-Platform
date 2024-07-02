// using CO.CDP.OrganisationInformation.Persistence.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Migrations;
//
// namespace CO.CDP.OrganisationInformation.Persistence.Forms;
//
// public class FormsContext(DbContextOptions<OrganisationInformationContext> options)
//     : DbContext(options)
// {
//
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//     }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//     {
//         optionsBuilder.UseSnakeCaseNamingConvention();
//         optionsBuilder.ReplaceService<IHistoryRepository, CamelCaseHistoryContext>();
//         base.OnConfiguring(optionsBuilder);
//     }
//
//     public override int SaveChanges()
//     {
//         UpdateTimestamps();
//         return base.SaveChanges();
//     }
//
//     public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//     {
//         UpdateTimestamps();
//         return base.SaveChangesAsync(cancellationToken);
//     }
//
//     private void UpdateTimestamps()
//     {
//         var entries = ChangeTracker
//             .Entries<IEntityDate>()
//             .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
//
//         foreach (var entityEntry in entries)
//         {
//             if (entityEntry.State == EntityState.Added)
//             {
//                 entityEntry.Entity.CreatedOn = DateTimeOffset.UtcNow;
//             }
//
//             entityEntry.Entity.UpdatedOn = DateTimeOffset.UtcNow;
//         }
//     }
// }
