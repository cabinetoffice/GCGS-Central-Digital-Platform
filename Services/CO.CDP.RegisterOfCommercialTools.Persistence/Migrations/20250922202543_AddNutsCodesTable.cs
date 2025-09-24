using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.RegisterOfCommercialTools.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNutsCodesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "commercial_tools_nuts_codes",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description_en =
                        table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description_cy =
                        table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    parent_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_selectable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commercial_tools_nuts_codes", x => x.code);
                    table.ForeignKey(
                        name: "fk_commercial_tools_nuts_codes_commercial_tools_nuts_codes_par",
                        column: x => x.parent_code,
                        principalTable: "commercial_tools_nuts_codes",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_commercial_tools_nuts_codes_code_is_active",
                table: "commercial_tools_nuts_codes",
                columns: new[] { "code", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_commercial_tools_nuts_codes_is_active_is_selectable",
                table: "commercial_tools_nuts_codes",
                columns: new[] { "is_active", "is_selectable" });

            migrationBuilder.CreateIndex(
                name: "ix_commercial_tools_nuts_codes_parent_code_is_active",
                table: "commercial_tools_nuts_codes",
                columns: new[] { "parent_code", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commercial_tools_nuts_codes");
        }
    }
}
