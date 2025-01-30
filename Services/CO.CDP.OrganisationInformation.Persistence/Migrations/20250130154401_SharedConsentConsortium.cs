using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SharedConsentConsortium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shared_consent_consortiums",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    child_shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_consent_consortiums", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_consent_consortiums_shared_consents_child_shared_con",
                        column: x => x.child_shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_consent_consortiums_shared_consents_parent_shared_co",
                        column: x => x.parent_shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shared_consent_consortiums_child_shared_consent_id",
                table: "shared_consent_consortiums",
                column: "child_shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_consent_consortiums_parent_shared_consent_id",
                table: "shared_consent_consortiums",
                column: "parent_shared_consent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_consent_consortiums");
        }
    }
}
