using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExclusionFormQuestionEightGroupChoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
              DO $$
              BEGIN                  
                  UPDATE form_questions
                  SET title = 'Exclusions_08_Title',
                  description = 'Exclusions_08_Description',
                  options = '{{""groups"": [{{""hint"": ""Exclusions_08_Options_Groups_01_Hint"", ""name"": ""Exclusions_08_Options_Groups_01_Name"", ""caption"": ""Exclusions_08_Options_Groups_01_Caption"", ""choices"": [{{""id"":""7e8669e6-bef3-4cf1-a8c4-a0003f47743a"", ""title"": ""Exclusions_08_Options_Groups_01_Choices_01_Title"", ""value"": ""adjustments_for_tax_arrangements""}}, {{""id"":""70b33154-689c-499c-be69-abad7e493c92"", ""title"": ""Exclusions_08_Options_Groups_01_Choices_02_Title"", ""value"": ""competition_law_infringements""}}, {{""id"":""99577e71-1819-4fcf-9e63-1d877dd09c12"",""title"": ""Exclusions_08_Options_Groups_01_Choices_03_Title"", ""value"": ""defeat_in_respect""}}, {{""id"":""cf35d4d8-993d-4bea-b14c-b34182df8ebc"", ""title"": ""Exclusions_08_Options_Groups_01_Choices_04_Title"", ""value"": ""failure_to_cooperate""}}, {{""id"":""629c2ce4-d77e-4fd5-ac78-1dd1c1747145"", ""title"": ""Exclusions_08_Options_Groups_01_Choices_05_Title"", ""value"": ""finding_by_HMRC""}}, {{""id"":""8dd0e803-3158-4fc6-8047-683a68c2ee8e"", ""title"": ""Exclusions_08_Options_Groups_01_Choices_06_Title"", ""value"": ""penalties_for_transactions""}}, {{""id"":""2847dc04-c6f5-4f87-abb3-0c6884844c59"",""title"": ""Exclusions_08_Options_Groups_01_Choices_07_Title"", ""value"": ""penalties_payable""}}]}}, {{""hint"": ""Exclusions_08_Options_Groups_02_Hint"", ""name"": ""Exclusions_08_Options_Groups_02_Name"", ""caption"": ""Exclusions_08_Options_Groups_02_Caption"", ""choices"": [{{""id"":""94f56050-a879-4de4-967d-1e9b3ee9ee70"", ""title"": ""Exclusions_08_Options_Groups_02_Choices_01_Title"", ""value"": ""ancillary_offences_aiding""}}, {{""id"":""635c1f29-1b77-4e5f-ab47-039fff1eb8d1"", ""title"": ""Exclusions_08_Options_Groups_02_Choices_02_Title"", ""value"": ""cartel_offences""}}, {{""id"":""46277494-f562-4c82-8b92-c52db58bbaee"", ""title"": ""Exclusions_08_Options_Groups_02_Choices_03_Title"", ""value"": ""corporate_manslaughter""}}, {{""id"":""d2cd6943-af69-4ed4-9ba3-1bf351bf6a3a"", ""title"": ""Exclusions_08_Options_Groups_02_Choices_04_Title"", ""value"": ""labour_market""}}, {{""id"":""88cf8c4b-309a-4764-92fc-2fcb03efc9c7"", ""title"": ""Exclusions_08_Options_Groups_02_Choices_05_Title"", ""value"": ""organised_crime""}}, {{""id"":""e6976075-5b3b-4682-b74e-f4f37f4e314e"", ""title"": ""Exclusions_08_Options_Groups_02_Choices_06_Title"", ""value"": ""tax_offences""}}, {{""id"":""c4c658fe-4cbe-45a8-bd2a-ea2c8e20d7b4"", ""title"": ""Exclusions_08_Options_Groups_02_Choices_07_Title"", ""value"": ""terrorism_and_offences""}}, {{""id"":""fb0f171b-4fb4-4ee6-ab7d-c2cca4be948c"", ""title"": ""Exclusions_08_Options_Groups_02_Choices_08_Title"", ""value"": ""theft_fraud""}}]}}, {{""hint"": ""Exclusions_08_Options_Groups_03_Hint"", ""name"": ""Exclusions_08_Options_Groups_03_Name"", ""caption"": ""Exclusions_08_Options_Groups_03_Caption"", ""choices"": [{{""id"": ""bac47f5c-cb99-4681-af19-a01a7bbee86a"",""title"": ""Exclusions_08_Options_Groups_03_Choices_01_Title"", ""value"": ""acting_improperly""}}, {{""id"": ""4cd39237-88d2-4b4d-a037-1d2cc76ce869"",""title"": ""Exclusions_08_Options_Groups_03_Choices_02_Title"", ""value"": ""breach_of_contract""}}, {{""id"": ""cc8bbedb-c698-42e6-a217-f37cf65f128e"",""title"": ""Exclusions_08_Options_Groups_03_Choices_03_Title"", ""value"": ""environmental_misconduct""}}, {{""id"":""0e3c0e11-d908-46fe-ab40-faf32833a825"",""title"": ""Exclusions_08_Options_Groups_03_Choices_04_Title"", ""value"": ""infringement_of_competition""}}, {{""id"":""df0f6ccc-8917-4ccb-a3b5-ff4d5da9b69d"", ""title"": ""Exclusions_08_Options_Groups_03_Choices_05_Title"", ""value"": ""insolvency_bankruptcy""}}, {{""id"":""60fd754f-a4d7-494b-8717-068924e7017b"", ""title"": ""Exclusions_08_Options_Groups_03_Choices_06_Title"", ""value"": ""labour_market_misconduct""}}, {{""id"":""8c668f5f-21f2-4004-b6ba-f62fab845d06"", ""title"": ""Exclusions_08_Options_Groups_03_Choices_07_Title"", ""value"": ""competition_law_infringements""}}, {{""id"":""a76aee38-2306-4b75-a082-2513ac142b4c"", ""title"": ""Exclusions_08_Options_Groups_03_Choices_08_Title"", ""value"": ""professional_misconduct""}}, {{""id"":""a9843a24-ea46-4c96-9a4f-0c829d251db4"", ""title"": ""Exclusions_08_Options_Groups_03_Choices_09_Title"", ""value"": ""substantial_part_business""}}]}}], ""choices"": null, ""choiceProviderStrategy"": null}}',                              
                  summary_title = 'Exclusions_08_SummaryTitle'
                  WHERE name = '_Exclusion08';        
              END $$;
           ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
