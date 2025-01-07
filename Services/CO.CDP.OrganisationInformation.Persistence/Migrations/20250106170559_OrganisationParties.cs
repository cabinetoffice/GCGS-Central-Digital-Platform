using System;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationParties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:connected_entity_individual_and_trust_category_type", "person_with_significant_control_for_indiv,director_or_indiv_with_the_same_responsibilities_for_indiv,any_other_indiv_with_significant_influence_or_control_for_indiv,person_with_significant_control_for_trust,director_or_indiv_with_the_same_responsibilities_for_trust,any_other_indiv_with_significant_influence_or_control_for_trust")
                .Annotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .Annotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:control_condition", "none,owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control")
                .Annotation("Npgsql:Enum:organisation_relationship", "consortium")
                .Annotation("Npgsql:Enum:organisation_type", "organisation,informal_consortium")
                .OldAnnotation("Npgsql:Enum:connected_entity_individual_and_trust_category_type", "person_with_significant_control_for_indiv,director_or_indiv_with_the_same_responsibilities_for_indiv,any_other_indiv_with_significant_influence_or_control_for_indiv,person_with_significant_control_for_trust,director_or_indiv_with_the_same_responsibilities_for_trust,any_other_indiv_with_significant_influence_or_control_for_trust")
                .OldAnnotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:control_condition", "none,owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:organisation_type", "organisation,informal_consortium");

            migrationBuilder.CreateTable(
                name: "organisation_parties",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    child_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_relationship = table.Column<OrganisationRelationship>(type: "organisation_relationship", nullable: false),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_parties", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_parties_organisations_child_organisation_id",
                        column: x => x.child_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_parties_organisations_parent_organisation_id",
                        column: x => x.parent_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_parties_shared_consents_shared_consent_id",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_organisation_parties_child_organisation_id",
                table: "organisation_parties",
                column: "child_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_parties_parent_organisation_id",
                table: "organisation_parties",
                column: "parent_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_parties_shared_consent_id",
                table: "organisation_parties",
                column: "shared_consent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organisation_parties");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:connected_entity_individual_and_trust_category_type", "person_with_significant_control_for_indiv,director_or_indiv_with_the_same_responsibilities_for_indiv,any_other_indiv_with_significant_influence_or_control_for_indiv,person_with_significant_control_for_trust,director_or_indiv_with_the_same_responsibilities_for_trust,any_other_indiv_with_significant_influence_or_control_for_trust")
                .Annotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .Annotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:control_condition", "none,owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control")
                .Annotation("Npgsql:Enum:organisation_type", "organisation,informal_consortium")
                .OldAnnotation("Npgsql:Enum:connected_entity_individual_and_trust_category_type", "person_with_significant_control_for_indiv,director_or_indiv_with_the_same_responsibilities_for_indiv,any_other_indiv_with_significant_influence_or_control_for_indiv,person_with_significant_control_for_trust,director_or_indiv_with_the_same_responsibilities_for_trust,any_other_indiv_with_significant_influence_or_control_for_trust")
                .OldAnnotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:control_condition", "none,owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:organisation_relationship", "consortium")
                .OldAnnotation("Npgsql:Enum:organisation_type", "organisation,informal_consortium");
        }
    }
}
