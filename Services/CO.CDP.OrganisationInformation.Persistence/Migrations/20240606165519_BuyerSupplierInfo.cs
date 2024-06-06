using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BuyerSupplierInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "OrganisationIdentifier",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedOn",
                table: "OrganisationIdentifier",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateTable(
                name: "BuyerInformation",
                columns: table => new
                {
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    BuyerType = table.Column<string>(type: "text", nullable: false),
                    DevolvedRegulations = table.Column<List<int>>(type: "integer[]", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerInformation", x => x.OrganisationId);
                    table.ForeignKey(
                        name: "FK_BuyerInformation_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierInformation",
                columns: table => new
                {
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    SupplierType = table.Column<int>(type: "integer", nullable: false),
                    OperationTypes = table.Column<int[]>(type: "integer[]", nullable: false),
                    CompletedRegAddress = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedPostalAddress = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedVat = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedWebsiteAddress = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedEmailAddress = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedQualification = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedTradeAssurance = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedOrganisationType = table.Column<bool>(type: "boolean", nullable: true),
                    CompletedLegalForm = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierInformation", x => x.OrganisationId);
                    table.ForeignKey(
                        name: "FK_SupplierInformation_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegalForm",
                columns: table => new
                {
                    SupplierInformationOrganisationId = table.Column<int>(type: "integer", nullable: false),
                    RegisteredUnderAct2006 = table.Column<string>(type: "text", nullable: false),
                    RegisteredLegalForm = table.Column<string>(type: "text", nullable: false),
                    LawRegistered = table.Column<string>(type: "text", nullable: false),
                    RegistrationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalForm", x => x.SupplierInformationOrganisationId);
                    table.ForeignKey(
                        name: "FK_LegalForm_SupplierInformation_SupplierInformationOrganisati~",
                        column: x => x.SupplierInformationOrganisationId,
                        principalTable: "SupplierInformation",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Qualification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AwardedByPersonOrBodyName = table.Column<string>(type: "text", nullable: true),
                    DateAwarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SupplierInformationOrganisationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Qualification_SupplierInformation_SupplierInformationOrgani~",
                        column: x => x.SupplierInformationOrganisationId,
                        principalTable: "SupplierInformation",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeAssurance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AwardedByPersonOrBodyName = table.Column<string>(type: "text", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    DateAwarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SupplierInformationOrganisationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeAssurance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeAssurance_SupplierInformation_SupplierInformationOrgan~",
                        column: x => x.SupplierInformationOrganisationId,
                        principalTable: "SupplierInformation",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Qualification_SupplierInformationOrganisationId",
                table: "Qualification",
                column: "SupplierInformationOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeAssurance_SupplierInformationOrganisationId",
                table: "TradeAssurance",
                column: "SupplierInformationOrganisationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuyerInformation");

            migrationBuilder.DropTable(
                name: "LegalForm");

            migrationBuilder.DropTable(
                name: "Qualification");

            migrationBuilder.DropTable(
                name: "TradeAssurance");

            migrationBuilder.DropTable(
                name: "SupplierInformation");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "OrganisationIdentifier");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "OrganisationIdentifier");
        }
    }
}
