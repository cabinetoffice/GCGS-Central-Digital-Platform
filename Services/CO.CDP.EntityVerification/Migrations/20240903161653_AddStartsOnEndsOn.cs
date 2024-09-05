using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.EntityVerification.Migrations
{
    /// <inheritdoc />
    public partial class AddStartsOnEndsOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ends_on",
                schema: "entity_verification",
                table: "ppons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "starts_on",
                schema: "entity_verification",
                table: "ppons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ends_on",
                schema: "entity_verification",
                table: "identifiers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "starts_on",
                schema: "entity_verification",
                table: "identifiers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ends_on",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropColumn(
                name: "starts_on",
                schema: "entity_verification",
                table: "ppons");

            migrationBuilder.DropColumn(
                name: "ends_on",
                schema: "entity_verification",
                table: "identifiers");

            migrationBuilder.DropColumn(
                name: "starts_on",
                schema: "entity_verification",
                table: "identifiers");
        }
    }
}
