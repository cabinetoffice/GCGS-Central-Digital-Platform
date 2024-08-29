using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UniqueNullableIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "identifier_id",
                table: "identifiers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.Sql("UPDATE identifiers SET identifier_id=null WHERE identifier_id='';");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_identifier_id_scheme",
                table: "identifiers",
                columns: new[] { "identifier_id", "scheme" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_identifiers_identifier_id_scheme",
                table: "identifiers");

            migrationBuilder.Sql("UPDATE identifiers SET identifier_id='' WHERE identifier_id IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "identifier_id",
                table: "identifiers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}