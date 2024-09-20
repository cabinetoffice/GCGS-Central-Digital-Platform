using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePponSchemeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE identifiers SET scheme = 'GB-PPON' WHERE scheme = 'CDP-PPON';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE identifiers SET scheme = 'CDP-PPON' WHERE scheme = 'GB-PPON';");
        }
    }
}
