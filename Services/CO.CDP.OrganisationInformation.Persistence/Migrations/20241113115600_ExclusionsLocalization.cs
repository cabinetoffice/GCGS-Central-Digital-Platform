using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExclusionsLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET title = 'Exclusions_07_Title',
                    summary_title = 'Exclusions_07_SummaryTitle'
                    WHERE name = '_Exclusion07'

                    UPDATE form_questions
                    SET title = 'Exclusions_06_Title',
                    description = 'Exclusions_06_Description',
                    summary_title = 'Exclusions_06_SummaryTitle'
                    WHERE name = '_Exclusion06'

                    UPDATE form_questions
                    SET title = 'Exclusions_05_Title',
                    description = 'Exclusions_05_Title',
                    summary_title = 'Exclusions_05_SummaryTitle'
                    WHERE name = '_Exclusion05'

                    UPDATE form_questions
                    SET title = 'Exclusions_04_Title',
                    description = 'Exclusions_04_Description',
                    summary_title = 'Exclusions_04_SummaryTitle'
                    WHERE name = '_Exclusion04'

                    UPDATE form_questions
                    SET title = 'Exclusions_03_Title',
                    description = 'Exclusions_03_Description',
                    caption = 'Exclusions_03_Caption',
                    summary_title = 'Exclusions_03_SummaryTitle'
                    WHERE name = '_Exclusion03'

                    UPDATE form_questions
                    SET title = 'Exclusions_02_Title',
                    description = 'Exclusions_02_Description',
                    caption = 'Exclusions_02_Caption',
                    summary_title = 'Exclusions_02_SummaryTitle'
                    WHERE name = '_Exclusion02'

                    UPDATE form_questions
                    SET title = 'Global_CheckYourAnswers',
                    WHERE name = '_Exclusion01'

                    UPDATE form_questions
                    SET title = 'Exclusions_09_Title',
                    description = 'Exclusions_09_Description',
                    summary_title = 'Exclusions_09_SummaryTitle'
                    WHERE name = '_Exclusion09'

                    UPDATE form_questions
                    SET title = 'Exclusions_10_Title',
                    description = 'Exclusions_10_Description',
                    caption = 'Exclusions_10_Caption',
                    summary_title = 'Exclusions_10_SummaryTitle'
                    WHERE name = '_Exclusion10'

                    UPDATE form_questions
                    SET title = 'Exclusions_08_Title',
                    description = 'Exclusions_08_Description',
                    options = '{{""groups"": [{{""hint"": ""Exclusions_08_Options_Groups_01_Hint"", ""name"": ""Exclusions_08_Options_Groups_01_Name"", ""caption"": ""Exclusions_08_Options_Groups_01_Caption"", ""choices"": [{{""title"": ""Exclusions_08_Options_Groups_01_Choices_01_Title"", ""value"": ""adjustments_for_tax_arrangements""}}, {{""title"": ""Exclusions_08_Options_Groups_01_Choices_02_Title"", ""value"": ""competition_law_infringements""}}, {{""title"": ""Exclusions_08_Options_Groups_01_Choices_03_Title"", ""value"": ""defeat_in_respect""}}, {{""title"": ""Exclusions_08_Options_Groups_01_Choices_04_Title"", ""value"": ""failure_to_cooperate""}}, {{""title"": ""Exclusions_08_Options_Groups_01_Choices_05_Title"", ""value"": ""finding_by_HMRC""}}, {{""title"": ""Exclusions_08_Options_Groups_01_Choices_06_Title"", ""value"": ""penalties_for_transactions""}}, {{""title"": ""Exclusions_08_Options_Groups_01_Choices_07_Title"", ""value"": ""penalties_payable""}}]}}, {{""hint"": ""Exclusions_08_Options_Groups_02_Hint"", ""name"": ""Exclusions_08_Options_Groups_02_Name"", ""caption"": ""Exclusions_08_Options_Groups_02_Caption"", ""choices"": [{{""title"": ""Exclusions_08_Options_Groups_02_Choices_01_Title"", ""value"": ""ancillary_offences_aiding""}}, {{""title"": ""Exclusions_08_Options_Groups_02_Choices_02_Title"", ""value"": ""cartel_offences""}}, {{""title"": ""Exclusions_08_Options_Groups_02_Choices_03_Title"", ""value"": ""corporate_manslaughter""}}, {{""title"": ""Exclusions_08_Options_Groups_02_Choices_04_Title"", ""value"": ""labour_market""}}, {{""title"": ""Exclusions_08_Options_Groups_02_Choices_05_Title"", ""value"": ""organised_crime""}}, {{""title"": ""Exclusions_08_Options_Groups_02_Choices_06_Title"", ""value"": ""tax_offences""}}, {{""title"": ""Exclusions_08_Options_Groups_02_Choices_07_Title"", ""value"": ""terrorism_and_offences""}}, {{""title"": ""Exclusions_08_Options_Groups_02_Choices_08_Title, fraud and bribery"", ""value"": ""theft_fraud""}}]}}, {{""hint"": ""Exclusions_08_Options_Groups_03_Hint"", ""name"": ""Exclusions_08_Options_Groups_03_Name"", ""caption"": ""Exclusions_08_Options_Groups_03_Caption"", ""choices"": [{{""title"": ""Exclusions_08_Options_Groups_03_Choices_01_Title"", ""value"": ""acting_improperly""}}, {{""title"": ""Exclusions_08_Options_Groups_03_Choices_02_Title"", ""value"": ""breach_of_contract""}}, {{""title"": ""Exclusions_08_Options_Groups_03_Choices_03_Title"", ""value"": ""environmental_misconduct""}}, {{""title"": ""Exclusions_08_Options_Groups_03_Choices_04_Title"", ""value"": ""infringement_of_competition""}}, {{""title"": ""Exclusions_08_Options_Groups_03_Choices_05_Title"", ""value"": ""insolvency_bankruptcy""}}, {{""title"": ""Exclusions_08_Options_Groups_03_Choices_06_Title"", ""value"": ""labour_market_misconduct""}}, {{""title"": ""Exclusions_08_Options_Groups_03_Choices_07_Title"", ""value"": ""competition_law_infringements""}}, {{""title"": ""Exclusions_08_Options_Groups_03_Choices_08_Title"", ""value"": ""professional_misconduct""}}, {{""title"": ""Exclusions_08_Options_Groups_03_Choices_09_Title"", ""value"": ""substantial_part_business""}}]}}], ""choices"": null, ""choiceProviderStrategy"": null}}',
                    summary_title = 'Exclusions_08_SummaryTitle'
                    WHERE name = '_Exclusion08'

                    UPDATE form_sections
                    SET configuration = '{{""AddAnotherAnswerLabel"": ""Exclusions_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""Exclusions_Configuration_SingularSummaryHeading"", ""RemoveConfirmationCaption"": ""Exclusions_SectionTitle"", ""RemoveConfirmationHeading"": ""Exclusions_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""Exclusions_Configuration_PluralSummaryHeadingFormat"", ""FurtherQuestionsExemptedHint"": ""Exclusions_Configuration_FurtherQuestionsExemptedHint"", ""FurtherQuestionsExemptedHeading"": ""Exclusions_Configuration_FurtherQuestionsExemptedHeading""}}'
                    WHERE title = 'Exclusions_SectionTitle';
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
