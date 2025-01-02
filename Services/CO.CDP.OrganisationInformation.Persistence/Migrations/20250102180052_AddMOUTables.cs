using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMOUTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mou",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mou", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mou_signatures",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    persion_id = table.Column<int>(type: "integer", nullable: false),
                    job_title = table.Column<string>(type: "text", nullable: false),
                    mou_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mou_signatures", x => x.id);
                    table.ForeignKey(
                        name: "fk_mou_signatures_mou_mou_id",
                        column: x => x.mou_id,
                        principalTable: "mou",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mou_signatures_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mou_signatures_persons_persion_id",
                        column: x => x.persion_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mou_guid",
                table: "mou",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mou_signatures_guid",
                table: "mou_signatures",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mou_signatures_mou_id",
                table: "mou_signatures",
                column: "mou_id");

            migrationBuilder.CreateIndex(
                name: "ix_mou_signatures_organisation_id",
                table: "mou_signatures",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_mou_signatures_persion_id",
                table: "mou_signatures",
                column: "persion_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mou_signatures");

            migrationBuilder.DropTable(
                name: "mou");
        }
    }
}
