using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrg2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_form_answer_sets_organisations_organisation_id",
                table: "form_answer_sets");

            migrationBuilder.DropIndex(
                name: "ix_form_answer_sets_organisation_id",
                table: "form_answer_sets");

            migrationBuilder.DropColumn(
                name: "organisation_id",
                table: "form_answer_sets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
