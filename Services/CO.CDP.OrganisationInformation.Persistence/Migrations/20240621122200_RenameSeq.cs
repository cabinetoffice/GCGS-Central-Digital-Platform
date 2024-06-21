using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameSeq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameSequence(name: "Address_Id_seq", newName: "addresses_id_seq");
            migrationBuilder.RenameSequence(name: "Organisations_Id_seq", newName: "organisations_id_seq");
            migrationBuilder.RenameSequence(name: "Persons_Id_seq", newName: "persons_id_seq");
            migrationBuilder.RenameSequence(name: "Tenants_Id_seq", newName: "tenants_id_seq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameSequence(name: "addresses_id_seq", newName: "Address_Id_seq");
            migrationBuilder.RenameSequence(name: "organisations_id_seq", newName: "Organisations_Id_seq");
            migrationBuilder.RenameSequence(name: "persons_id_seq", newName: "Persons_Id_seq");
            migrationBuilder.RenameSequence(name: "tenants_id_seq", newName: "Tenants_Id_seq");
        }
    }
}
