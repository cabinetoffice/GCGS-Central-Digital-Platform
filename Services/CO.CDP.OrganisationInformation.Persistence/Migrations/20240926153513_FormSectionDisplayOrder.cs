using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormSectionDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "form_sections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql($@"
                update form_sections set display_order = 1 where title = 'Qualifications';
                update form_sections set display_order = 2 where title = 'Trade assurances';
                update form_sections set display_order = 3 where title = 'Financial Information';
                update form_sections set display_order = 4 where title = 'Exclusions';
                update form_sections set display_order = 100 where title = 'Share my information';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_order",
                table: "form_sections");
        }
    }
}
