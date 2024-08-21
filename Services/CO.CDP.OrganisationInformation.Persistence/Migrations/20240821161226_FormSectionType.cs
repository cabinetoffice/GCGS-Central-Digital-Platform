using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormSectionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "forms");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "form_sections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE form_sections SET type = 1 WHERE guid = '936096b3-c3bb-4475-ad7d-73b44ff61e76';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "form_sections");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "forms",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
