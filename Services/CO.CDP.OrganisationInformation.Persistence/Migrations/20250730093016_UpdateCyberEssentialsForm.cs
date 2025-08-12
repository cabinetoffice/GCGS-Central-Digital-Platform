using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCyberEssentialsForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_02_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""CyberEssentials_02_CustomYesText"", ""customNoText"": ""CyberEssentials_02_CustomNoText"", ""beforeButtonContent"": ""CyberEssentials_02_CustomInsetText""}, ""grouping"": {""id"": ""a1b2c3d4-e5f6-4789-abcd-ef1234567890"", ""checkYourAnswers"": true, ""page"": false, ""summaryTitle"": ""CyberEssentials_Group1_SummaryTitle""}}'
                        WHERE title = 'CyberEssentials_02_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_03_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""CyberEssentials_03_CustomYesText"", ""customNoText"": ""CyberEssentials_03_CustomNoText""}, ""grouping"": {""id"": ""a1b2c3d4-e5f6-4789-abcd-ef1234567890"", ""checkYourAnswers"": true, ""page"": false}}'
                        WHERE title = 'CyberEssentials_03_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_04_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""CyberEssentials_04_CustomYesText"", ""customNoText"": ""CyberEssentials_04_CustomNoText""}, ""grouping"": {""id"": ""a1b2c3d4-e5f6-4789-abcd-ef1234567890"", ""checkYourAnswers"": true, ""page"": false}}'
                        WHERE title = 'CyberEssentials_04_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_05_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""CyberEssentials_05_CustomYesText"", ""customNoText"": ""CyberEssentials_05_CustomNoText""}, ""grouping"": {""id"": ""a1b2c3d4-e5f6-4789-abcd-ef1234567890"", ""checkYourAnswers"": true, ""page"": false}}'
                        WHERE title = 'CyberEssentials_05_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_06_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""CyberEssentials_06_CustomYesText"", ""customNoText"": ""CyberEssentials_06_CustomNoText"", ""beforeButtonContent"": ""CyberEssentials_06_CustomInsetText""}, ""grouping"": {""id"": ""b2c3d4e5-f6a7-4901-bcde-f23456789012"", ""checkYourAnswers"": true, ""page"": false, ""summaryTitle"": ""CyberEssentials_Group2_SummaryTitle""}}'
                        WHERE title = 'CyberEssentials_06_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_07_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""CyberEssentials_07_CustomYesText"", ""customNoText"": ""CyberEssentials_07_CustomNoText""}, ""grouping"": {""id"": ""b2c3d4e5-f6a7-4901-bcde-f23456789012"", ""checkYourAnswers"": true, ""page"": false}}'
                        WHERE title = 'CyberEssentials_07_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_08_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""CyberEssentials_08_CustomYesText"", ""customNoText"": ""CyberEssentials_08_CustomNoText""}, ""grouping"": {""id"": ""b2c3d4e5-f6a7-4901-bcde-f23456789012"", ""checkYourAnswers"": true, ""page"": false}}'
                        WHERE title = 'CyberEssentials_08_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'CyberEssentials_09_Title') THEN
                        UPDATE form_questions
                        SET options = '{""layout"": {""customYesText"": ""CyberEssentials_09_CustomYesText"", ""customNoText"": ""CyberEssentials_09_CustomNoText""}, ""grouping"": {""id"": ""b2c3d4e5-f6a7-4901-bcde-f23456789012"", ""checkYourAnswers"": true, ""page"": false}}'
                        WHERE title = 'CyberEssentials_09_Title';
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET options = '{}'
                    WHERE title IN (
                        'CyberEssentials_02_Title',
                        'CyberEssentials_03_Title',
                        'CyberEssentials_04_Title',
                        'CyberEssentials_05_Title',
                        'CyberEssentials_06_Title',
                        'CyberEssentials_07_Title',
                        'CyberEssentials_08_Title',
                        'CyberEssentials_09_Title'
                    );
                END $$;
            ");
        }
    }
}
