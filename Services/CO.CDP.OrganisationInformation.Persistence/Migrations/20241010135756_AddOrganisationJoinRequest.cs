using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationJoinRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organisation_join_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reviewed_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reviewed_by_id = table.Column<int>(type: "integer", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_join_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_join_requests_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_join_requests_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_join_requests_persons_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "persons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_organisation_join_requests_guid",
                table: "organisation_join_requests",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisation_join_requests_organisation_id",
                table: "organisation_join_requests",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_join_requests_person_id",
                table: "organisation_join_requests",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_join_requests_reviewed_by_id",
                table: "organisation_join_requests",
                column: "reviewed_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organisation_join_requests");
        }
    }
}
