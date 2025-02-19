using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrganisationNameToCitext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS citext;");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "organisations",
                type: "citext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "organisations",
                type: "text",
                nullable: false);

            migrationBuilder.Sql("DROP EXTENSION IF EXISTS citext;");
        }
    }
}
