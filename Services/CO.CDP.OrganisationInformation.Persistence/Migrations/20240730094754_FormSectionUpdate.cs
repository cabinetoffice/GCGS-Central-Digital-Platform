using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormSectionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "allows_multiple_answer_sets",
                table: "form_sections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql($@"
                UPDATE form_sections
                SET allows_multiple_answer_sets = 1
                WHERE guid = '13511cb1-9ed4-4d72-ba9e-05b4a0be880c';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allows_multiple_answer_sets",
                table: "form_sections");
        }
    }
}
