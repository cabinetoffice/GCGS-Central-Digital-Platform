using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organisations_AdditionalIdentifiers");

            migrationBuilder.DropColumn(
                name: "Identifier_Id",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Identifier_LegalName",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Identifier_Number",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Identifier_Scheme",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Identifier_Uri",
                table: "Organisations");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Persons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Persons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Organisations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Organisations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "OrganisationIdentifier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Primary = table.Column<bool>(type: "boolean", nullable: false),
                    IdentifierId = table.Column<string>(type: "text", nullable: true),
                    LegalName = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<string>(type: "text", nullable: false),
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    Scheme = table.Column<string>(type: "text", nullable: false),
                    Uri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationIdentifier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationIdentifier_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationIdentifier_OrganisationId",
                table: "OrganisationIdentifier",
                column: "OrganisationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganisationIdentifier");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Persons",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Persons",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "Organisations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "Organisations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Identifier_Id",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identifier_LegalName",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identifier_Number",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Identifier_Scheme",
                table: "Organisations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Identifier_Uri",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organisations_AdditionalIdentifiers",
                columns: table => new
                {
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<string>(type: "text", nullable: false),
                    LegalName = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<string>(type: "text", nullable: false),
                    Scheme = table.Column<string>(type: "text", nullable: false),
                    Uri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisations_AdditionalIdentifiers", x => new { x.OrganisationId, x.Id });
                    table.ForeignKey(
                        name: "FK_Organisations_AdditionalIdentifiers_Organisations_Organisat~",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
