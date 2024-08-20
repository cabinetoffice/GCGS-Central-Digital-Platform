using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveOrganisationToSharedConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE form_answer_sets CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE shared_consents CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE form_questions CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE form_sections CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE forms CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_form_answer_sets_organisations_organisation_id",
                table: "form_answer_sets");

            migrationBuilder.DropForeignKey(
                name: "fk_form_answer_sets_shared_consents_shared_consent_id",
                table: "form_answer_sets");

            migrationBuilder.DropIndex(
                name: "ix_form_answer_sets_organisation_id",
                table: "form_answer_sets");

            migrationBuilder.DropColumn(
                name: "organisation_id",
                table: "form_answer_sets");

            migrationBuilder.AlterColumn<int>(
                name: "shared_consent_id",
                table: "form_answer_sets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_form_answer_sets_shared_consents_shared_consent_id",
                table: "form_answer_sets",
                column: "shared_consent_id",
                principalTable: "shared_consents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE form_answer_sets CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE shared_consents CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE form_questions CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE form_sections CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE forms CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_form_answer_sets_shared_consents_shared_consent_id",
                table: "form_answer_sets");

            migrationBuilder.AlterColumn<int>(
                name: "shared_consent_id",
                table: "form_answer_sets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "organisation_id",
                table: "form_answer_sets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_form_answer_sets_organisation_id",
                table: "form_answer_sets",
                column: "organisation_id");

            migrationBuilder.AddForeignKey(
                name: "fk_form_answer_sets_organisations_organisation_id",
                table: "form_answer_sets",
                column: "organisation_id",
                principalTable: "organisations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_form_answer_sets_shared_consents_shared_consent_id",
                table: "form_answer_sets",
                column: "shared_consent_id",
                principalTable: "shared_consents",
                principalColumn: "id");
        }
    }
}
