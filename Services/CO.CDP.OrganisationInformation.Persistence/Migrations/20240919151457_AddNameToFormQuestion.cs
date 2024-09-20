using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToFormQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "form_questions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_FinancialInformation01' WHERE title = 'What is the financial year end date for the information you uploaded?'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_FinancialInformation02' WHERE title = 'Upload your accounts'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_FinancialInformation03' WHERE title = 'Were your accounts audited?'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_FinancialInformation04' WHERE title = 'The financial information you will need.'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_ShareMyInformation01' WHERE title = 'Enter your postal address'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_ShareMyInformation02' WHERE title = 'Enter your email address'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_ShareMyInformation03' WHERE title = 'Enter your job title'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_ShareMyInformation04' WHERE title = 'Enter your name'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_ShareMyInformation05' WHERE title = 'Declaration'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_FinancialInformation05' WHERE section_id = (SELECT id FROM form_sections WHERE title = 'Financial Information') AND title = 'Check your answers'");
            migrationBuilder.Sql(
                "UPDATE form_questions SET name = '_ShareMyInformation06' WHERE section_id = (SELECT id FROM form_sections WHERE title = 'Share my information') AND title = 'Check your answers'");

            migrationBuilder.CreateIndex(
                name: "ix_form_questions_name",
                table: "form_questions",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_form_questions_name",
                table: "form_questions");

            migrationBuilder.DropColumn(
                name: "name",
                table: "form_questions");
        }
    }
}
