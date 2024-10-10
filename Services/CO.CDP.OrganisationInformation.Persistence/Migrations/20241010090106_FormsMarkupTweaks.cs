using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormsMarkupTweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN

                    -- Removing invalid <br> tags from inside a <ul> and replacing with spacing classes
                    UPDATE form_questions
                    SET description = '<ul class=""govuk-list govuk-list--bullet""><li class=""govuk-!-margin-bottom-4"">I am authorised to make this declaration on behalf of the supplier and declare that to the best of my knowledge the answers submitted and information contained is correct and accurate at the time of declaration.</li> <li class=""govuk-!-margin-bottom-4"">I declare that, upon request from the Contracting Authority and without delay I will provide the certificates or documentary evidence referred to in this information.</li><li class=""govuk-!-margin-bottom-4"">I understand that the information is required as per the regulations of the Procurement Act 2023 and may be used in the selection process to assess my suitability to participate further in this procurement.</li><li class=""govuk-!-margin-bottom-4"">I understand that a contracting authority with whom this information is shared may request further clarity or detail on information provided in this submission.</li></ul>'
                    WHERE name = '_ShareMyInformation05';

                    -- Removing label markup from conditional text input captions
                    UPDATE form_questions
                    SET caption = 'Website address'
                    WHERE name = '_Exclusion10';

                    -- Removing label markup from conditional text input captions
                    UPDATE form_questions
                    SET caption = 'Trade assurance reference number'
                    WHERE name = '_TradeAssurance02';
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
