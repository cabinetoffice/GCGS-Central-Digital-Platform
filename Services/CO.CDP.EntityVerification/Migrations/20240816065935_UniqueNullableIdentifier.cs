using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.EntityVerification.Migrations
{
    /// <inheritdoc />
    public partial class UniqueNullableIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "identifier_id",
                schema: "entity_verification",
                table: "identifiers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.Sql("UPDATE entity_verification.identifiers SET identifier_id=null WHERE identifier_id='';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "identifier_id",
                schema: "entity_verification",
                table: "identifiers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE entity_verification.identifiers SET identifier_id='' WHERE identifier_id IS NULL;");
        }
    }
}