using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExclusionsAppliesToAnswerFieldNameConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET options= '{{""choices"": [], ""choiceProviderStrategy"": ""ExclusionAppliesToChoiceProviderStrategy"", ""answerFieldName"": ""JsonValue""}}'
                    WHERE name = '_Exclusion09';
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
