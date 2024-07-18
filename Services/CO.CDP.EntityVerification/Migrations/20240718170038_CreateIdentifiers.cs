using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.EntityVerification.Migrations
{
    /// <inheritdoc />
    public partial class CreateIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_on",
                schema: "entity_verification",
                table: "ppon",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "identifier_id",
                schema: "entity_verification",
                table: "ppon",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "entity_verification",
                table: "ppon",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_on",
                schema: "entity_verification",
                table: "ppon",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateTable(
                name: "identifier",
                schema: "entity_verification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    scheme = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identifier", x => x.id);
                    table.ForeignKey(
                        name: "fk_identifier_ppon_id",
                        column: x => x.id,
                        principalSchema: "entity_verification",
                        principalTable: "ppon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_identifier_id",
                schema: "entity_verification",
                table: "identifier",
                column: "id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identifier",
                schema: "entity_verification");

            migrationBuilder.DropColumn(
                name: "created_on",
                schema: "entity_verification",
                table: "ppon");

            migrationBuilder.DropColumn(
                name: "identifier_id",
                schema: "entity_verification",
                table: "ppon");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "entity_verification",
                table: "ppon");

            migrationBuilder.DropColumn(
                name: "updated_on",
                schema: "entity_verification",
                table: "ppon");
        }
    }
}
