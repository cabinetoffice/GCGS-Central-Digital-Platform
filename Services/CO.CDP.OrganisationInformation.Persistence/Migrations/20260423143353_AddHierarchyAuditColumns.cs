using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHierarchyAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "organisation_hierarchies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "superseded_by",
                table: "organisation_hierarchies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "organisation_hierarchies");

            migrationBuilder.DropColumn(
                name: "superseded_by",
                table: "organisation_hierarchies");
        }
    }
}
