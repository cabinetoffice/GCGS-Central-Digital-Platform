using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExclusionFormDataGroupedSingleChoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    sectionId INT;
                    questionId INT;
                BEGIN
                    SELECT id INTO sectionId FROM form_sections WHERE guid = '8a75cb04-fe29-45ae-90f9-168832dbea48';
 
                    SELECT id INTO questionId FROM form_questions WHERE section_id = sectionId AND name = '_Exclusion06';

                    INSERT INTO form_questions (guid, section_id, next_question_id, type, is_required, title, description, options, caption, summary_title, name)
                    VALUES ('{Guid.NewGuid()}',
                            sectionId,
                            questionId,
                            {(int)FormQuestionType.GroupedSingleChoice},
                            TRUE,
                            'Select which exclusion applies',
                            '<div id=""only-one-exclusion-hint"" class=""govuk-hint"">Only select one exclusion. You can add another at the end if you need to.</div>',
                            '{{ 
                            ""choices"": null,
                            ""choiceProviderStrategy"": null,
                            ""groups"": [
                                {{
                                    ""name"": ""Penalties and other events"",
                                    ""hint"": ""Defined in <a target=\""_blank\"" href=\""https://www.legislation.gov.uk/ukpga/2023/54/schedule/6\"">schedule 6 of the Procurement Act 2023 (opens in new tab).</a>"",
                                    ""caption"": ""Mandatory exclusions"",
                                    ""choices"": [
                                        {{""title"": ""Adjustments for tax arrangements that are abusive"", ""value"": ""adjustments_for_tax_arrangements""}},
                                        {{""title"": ""Competition law infringements"", ""value"": ""competition_law_infringements""}},
                                        {{""title"": ""Defeat in respect of notifiable tax arrangements"", ""value"": ""defeat_in_respect""}},
                                        {{""title"": ""Failure to cooperate with an investigation"", ""value"": ""failure_to_cooperate""}},
                                        {{""title"": ""Finding by HMRC, in exercise of its powers in respect of VAT, of abusive practice"", ""value"": ""finding_by_HMRC""}},
                                        {{""title"": ""Penalties for transactions connected with VAT fraud and evasion of tax or duty"", ""value"": ""penalties_for_transactions""}},
                                        {{""title"": ""Penalties payable for errors in tax documentation and failure to notify, and certain VAT and excise"", ""value"": ""penalties_payable""}}
                                    ]
                                }},
                                {{
                                    ""name"": ""Convictions"",
                                    ""hint"": ""Defined in <a target=\""_blank\"" href=\""https://www.legislation.gov.uk/ukpga/2023/54/schedule/6\"">schedule 6 of the Procurement Act 2023 (opens in new tab).</a>"",
                                    ""caption"": ""Mandatory exclusions"",
                                    ""choices"": [
                                        {{""title"": ""Ancillary offences - aiding, abetting, encouraging or assisting crime"", ""value"": ""ancillary_offences_aiding""}},
                                        {{""title"": ""Cartel offences"", ""value"": ""cartel_offences""}},
                                        {{""title"": ""Corporate manslaughter or homicide"", ""value"": ""corporate_manslaughter""}},
                                        {{""title"": ""Labour market, slavery and human trafficking offences"", ""value"": ""labour_market""}},
                                        {{""title"": ""Organised crime"", ""value"": ""organised_crime""}},
                                        {{""title"": ""Tax offences"", ""value"": ""tax_offences""}},
                                        {{""title"": ""Terrorism and offences having a terrorist connection"", ""value"": ""terrorism_and_offences""}},
                                        {{""title"": ""Theft, fraud and bribery"", ""value"": ""theft_fraud""}}
                                    ]
                                }},
                                {{
                                    ""name"": ""Discretionary exclusions"",
                                    ""hint"": ""Defined in <a target=\""_blank\"" href=\""https://www.legislation.gov.uk/ukpga/2023/54/schedule/7\"">schedule 7 of the Procurement Act 2023 (opens in new tab).</a>"",
                                    ""caption"": ""Discretionary exclusions"",
                                    ""choices"": [
                                        {{""title"": ""Acting improperly in procurement"", ""value"": ""acting_improperly""}},
                                        {{""title"": ""Breach of contract and poor performance"", ""value"": ""breach_of_contract""}},
                                        {{""title"": ""Environmental misconduct"", ""value"": ""environmental_misconduct""}},
                                        {{""title"": ""Infringement of Competition Act 1998, under Chapter II prohibition"", ""value"": ""infringement_of_competition""}},
                                        {{""title"": ""Insolvency or bankruptcy"", ""value"": ""insolvency_bankruptcy""}},
                                        {{""title"": ""Labour market misconduct"", ""value"": ""labour_market_misconduct""}},
                                        {{""title"": ""Potential competition and competition law infringements"", ""value"": ""competition_law_infringements""}},
                                        {{""title"": ""Professional misconduct"", ""value"": ""professional_misconduct""}},
                                        {{""title"": ""Suspension or ceasing to carry on all or a substantial part of a business"", ""value"": ""substantial_part_business""}}
                                    ]
                                }}
                            ]
                        }}', NULL, 'Exclusion applies', '_Exclusion08')

                    RETURNING id INTO questionId;
 
                    UPDATE form_questions SET next_question_id = questionId WHERE sectionId = id AND name = '_Exclusion07';
                END $$;
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
                    sectionId INT;
                    questionId INT;
                BEGIN
                    SELECT id INTO sectionId FROM form_sections WHERE guid = '8a75cb04-fe29-45ae-90f9-168832dbea48';

                    SELECT id INTO questionId FROM form_questions WHERE section_id = sectionId AND name = '_Exclusion06';

                    UPDATE form_questions SET next_question_id = questionId WHERE section_id = sectionId AND name = '_Exclusion07';

                    DELETE FROM form_questions WHERE section_id = sectionId AND name = '_Exclusion08';
                END $$;
            ");
        }
    }
}
