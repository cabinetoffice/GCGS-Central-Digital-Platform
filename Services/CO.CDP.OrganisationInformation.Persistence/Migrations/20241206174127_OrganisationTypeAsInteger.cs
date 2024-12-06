using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationTypeAsInteger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "organisations");

            migrationBuilder.AddColumn<OrganisationType>(
                name: "type",
                table: "organisations",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "organisations");

            migrationBuilder.AddColumn<OrganisationType>(
                name: "type",
                table: "organisations",
                type: "organisation_type",
                nullable: false,
                defaultValue: OrganisationType.Organisation);
        }
    }
}
