using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.EntityVerification.Migrations
{
    /// <inheritdoc />
    public partial class IdentifierRegistries1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_identifier_registries_country_code",
                schema: "entity_verification",
                table: "identifier_registries",
                column: "country_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_identifier_registries_country_code",
                schema: "entity_verification",
                table: "identifier_registries");
        }
    }
}
