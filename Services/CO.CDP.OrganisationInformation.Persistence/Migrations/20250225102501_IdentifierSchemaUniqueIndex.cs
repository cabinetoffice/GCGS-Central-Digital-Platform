using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdentifierSchemaUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_identifiers_identifier_id_scheme",
                table: "identifiers");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_identifier_id_scheme",
                table: "identifiers",
                columns: new[] { "identifier_id", "scheme" },
                unique: true,
                filter: "scheme <> 'VAT'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_identifiers_identifier_id_scheme",
                table: "identifiers");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_identifier_id_scheme",
                table: "identifiers",
                columns: new[] { "identifier_id", "scheme" },
                unique: true);
        }
    }
}
