using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonToPersonInvite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "person_id",
                table: "person_invites",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_invites_person_id",
                table: "person_invites",
                column: "person_id");

            migrationBuilder.AddForeignKey(
                name: "fk_person_invites_persons_person_id",
                table: "person_invites",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_person_invites_persons_person_id",
                table: "person_invites");

            migrationBuilder.DropIndex(
                name: "ix_person_invites_person_id",
                table: "person_invites");

            migrationBuilder.DropColumn(
                name: "person_id",
                table: "person_invites");
        }
    }
}
