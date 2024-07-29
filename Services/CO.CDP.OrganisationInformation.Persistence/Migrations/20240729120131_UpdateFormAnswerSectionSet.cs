using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFormAnswerSectionSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_form_answer_sets_form_sections_section_id",
                table: "form_answer_sets");

            migrationBuilder.AddForeignKey(
                name: "fk_form_answer_sets_form_section_section_id",
                table: "form_answer_sets",
                column: "section_id",
                principalTable: "form_sections",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_form_answer_sets_form_section_section_id",
                table: "form_answer_sets");

            migrationBuilder.AddForeignKey(
                name: "fk_form_answer_sets_form_sections_section_id",
                table: "form_answer_sets",
                column: "section_id",
                principalTable: "form_sections",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
