using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeApprovedToReviewed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organisations_persons_approved_by_id",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "approved_comment",
                table: "organisations");

            migrationBuilder.RenameColumn(
                name: "approved_by_id",
                table: "organisations",
                newName: "reviewed_by_id");

            migrationBuilder.RenameIndex(
                name: "ix_organisations_approved_by_id",
                table: "organisations",
                newName: "ix_organisations_reviewed_by_id");

            migrationBuilder.AddColumn<string>(
                name: "review_comment",
                table: "organisations",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_organisations_persons_reviewed_by_id",
                table: "organisations",
                column: "reviewed_by_id",
                principalTable: "persons",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organisations_persons_reviewed_by_id",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "review_comment",
                table: "organisations");

            migrationBuilder.RenameColumn(
                name: "reviewed_by_id",
                table: "organisations",
                newName: "approved_by_id");

            migrationBuilder.RenameIndex(
                name: "ix_organisations_reviewed_by_id",
                table: "organisations",
                newName: "ix_organisations_approved_by_id");

            migrationBuilder.AddColumn<string>(
                name: "approved_comment",
                table: "organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_organisations_persons_approved_by_id",
                table: "organisations",
                column: "approved_by_id",
                principalTable: "persons",
                principalColumn: "id");
        }
    }
}
