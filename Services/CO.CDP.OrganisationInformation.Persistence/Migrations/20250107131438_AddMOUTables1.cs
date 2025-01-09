using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMOUTables1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_mou_signature_persons_person_id",
                table: "mou_signature");

            migrationBuilder.RenameColumn(
                name: "person_id",
                table: "mou_signature",
                newName: "created_by_id");

            migrationBuilder.RenameIndex(
                name: "ix_mou_signature_person_id",
                table: "mou_signature",
                newName: "ix_mou_signature_created_by_id");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "mou_signature",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "fk_mou_signature_persons_created_by_id",
                table: "mou_signature",
                column: "created_by_id",
                principalTable: "persons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_mou_signature_persons_created_by_id",
                table: "mou_signature");

            migrationBuilder.DropColumn(
                name: "name",
                table: "mou_signature");

            migrationBuilder.RenameColumn(
                name: "created_by_id",
                table: "mou_signature",
                newName: "person_id");

            migrationBuilder.RenameIndex(
                name: "ix_mou_signature_created_by_id",
                table: "mou_signature",
                newName: "ix_mou_signature_person_id");

            migrationBuilder.AddForeignKey(
                name: "fk_mou_signature_persons_person_id",
                table: "mou_signature",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
