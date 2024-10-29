using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnnotationsForTrustsTrustees : Migration
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
                .OldAnnotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:connected_person_category", "person_with_significant_control,director_or_individual_with_the_same_responsibilities,any_other_individual_with_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:control_condition", "none,owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .Annotation("Npgsql:Enum:connected_person_category", "person_with_significant_control,director_or_individual_with_the_same_responsibilities,any_other_individual_with_significant_influence_or_control")
                .Annotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:control_condition", "none,owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:connected_entity_individual_and_trust_category_type", "person_with_significant_control_for_indiv,director_or_indiv_with_the_same_responsibilities_for_indiv,any_other_indiv_with_significant_influence_or_control_for_indiv,person_with_significant_control_for_trust,director_or_indiv_with_the_same_responsibilities_for_trust,any_other_indiv_with_significant_influence_or_control_for_trust")
                .OldAnnotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:control_condition", "none,owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control");
        }
    }
}
