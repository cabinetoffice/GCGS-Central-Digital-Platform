using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExclusionFormSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
	                form_id int;
                BEGIN
                    SELECT id INTO form_id FROM forms WHERE guid = '0618b13e-eaf2-46e3-a7d2-6f2c44be7022';

	                INSERT INTO form_sections (guid, title, form_id, allows_multiple_answer_sets, type, configuration)
                    VALUES ('8a75cb04-fe29-45ae-90f9-168832dbea48', 'Exclusions', form_id, TRUE, 2, '{{""AddAnotherAnswerLabel"": ""Add another exclusion?"", ""SingularSummaryHeading"": ""You have added 1 exclusions"", ""RemoveConfirmationCaption"": ""Exclusions"", ""RemoveConfirmationHeading"": ""Are you sure you want to remove this exclusion?"", ""PluralSummaryHeadingFormat"": ""You have added {{0}} exclusions""}}');
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM form_sections WHERE guid = '8a75cb04-fe29-45ae-90f9-168832dbea48';");
        }
    }
}
