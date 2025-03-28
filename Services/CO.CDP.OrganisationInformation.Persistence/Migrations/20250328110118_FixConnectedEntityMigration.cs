using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixConnectedEntityMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE
                    connected_individual_trust as cit
                SET
                    category = 4
                FROM
                    connected_entities ce
                WHERE
                    cit.connected_individual_trust_id = ce.id
                    AND ce.entity_type = 3
                    AND cit.category = 1
            ");

            migrationBuilder.Sql($@"
                UPDATE
                    connected_individual_trust_snapshot as cits
                SET
                    category = 4
                FROM
                    connected_entities_snapshot ces
                WHERE
                    cits.id = ces.id
                    AND ces.entity_type = 3
                    AND cits.category = 1
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
