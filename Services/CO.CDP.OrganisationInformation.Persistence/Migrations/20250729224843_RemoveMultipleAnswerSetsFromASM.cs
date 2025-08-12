using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMultipleAnswerSetsFromASM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    UPDATE form_sections
                    SET ""configuration"" = '{}',
                        ""allows_multiple_answer_sets"" = false,
                        ""check_further_questions_exempted"" = false
                    WHERE ""title"" IN (
                        'Steel_SectionTitle',
                        'HealthAndSafety_SectionTitle',
                        'ModernSlavery_SectionTitle',
                        'CyberEssentials_SectionTitle',
                        'DataProtection_SectionTitle',
                        'CarbonNetZero_SectionTitle',
                        'Payments_SectionTitle'
                    );
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    UPDATE form_sections
                    SET ""configuration"" = '{""AddAnotherAnswerLabel"": ""CarbonNetZero_Configuration_AddAnotherAnswerLabel"", ""SingularSummaryHeading"": ""CarbonNetZero_Configuration_SingularSummaryHeading"", ""SummaryRenderFormatter"": {""KeyParams"": [""_CarbonNetZeroQuestion01"", ""_CarbonNetZeroQuestion02""], ""ValueParams"": [""_CarbonNetZeroQuestion01"", ""_CarbonNetZeroQuestion02""], ""KeyExpression"": ""{0}"", ""ValueExpression"": ""{1}"", ""KeyExpressionOperation"": ""StringFormat"", ""ValueExpressionOperation"": ""StringFormat""}, ""RemoveConfirmationCaption"": ""CarbonNetZero_SectionTitle"", ""RemoveConfirmationHeading"": ""CarbonNetZero_Configuration_RemoveConfirmationHeading"", ""PluralSummaryHeadingFormat"": ""CarbonNetZero_Configuration_PluralSummaryHeadingFormat"", ""SingularSummaryHeadingHint"": ""CarbonNetZero_Configuration_SingularSummaryHeadingHint"", ""FurtherQuestionsExemptedHint"": ""CarbonNetZero_Configuration_FurtherQuestionsExemptedHint"", ""PluralSummaryHeadingHintFormat"": ""CarbonNetZero_Configuration_PluralSummaryHeadingHintFormat"", ""FurtherQuestionsExemptedHeading"": ""CarbonNetZero_Configuration_FurtherQuestionsExemptedHeading""}'
                    WHERE ""title"" = 'CarbonNetZero_SectionTitle';
                END $$;
            ");
        }
    }
}
