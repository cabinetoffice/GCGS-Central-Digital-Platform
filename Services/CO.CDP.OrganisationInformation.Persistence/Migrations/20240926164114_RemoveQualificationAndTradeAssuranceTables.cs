using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQualificationAndTradeAssuranceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qualifications");

            migrationBuilder.DropTable(
                name: "trade_assurances");

            migrationBuilder.DropColumn(
                name: "completed_qualification",
                table: "supplier_information");

            migrationBuilder.DropColumn(
                name: "completed_trade_assurance",
                table: "supplier_information");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "completed_qualification",
                table: "supplier_information",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "completed_trade_assurance",
                table: "supplier_information",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "qualifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    awarded_by_person_or_body_name = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    date_awarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    supplier_information_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_qualifications_supplier_information_supplier_information_or",
                        column: x => x.supplier_information_organisation_id,
                        principalTable: "supplier_information",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trade_assurances",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    awarded_by_person_or_body_name = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    date_awarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_number = table.Column<string>(type: "text", nullable: false),
                    supplier_information_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trade_assurances", x => x.id);
                    table.ForeignKey(
                        name: "fk_trade_assurances_supplier_information_supplier_information_",
                        column: x => x.supplier_information_organisation_id,
                        principalTable: "supplier_information",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_guid",
                table: "qualifications",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_supplier_information_organisation_id",
                table: "qualifications",
                column: "supplier_information_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_trade_assurances_guid",
                table: "trade_assurances",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trade_assurances_supplier_information_organisation_id",
                table: "trade_assurances",
                column: "supplier_information_organisation_id");
        }
    }
}
