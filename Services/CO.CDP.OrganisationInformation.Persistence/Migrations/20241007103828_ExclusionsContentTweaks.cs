using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExclusionsContentTweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET description = '<div class=""govuk-hint""><p>Only select one exclusion. You can add another at the end if you need to.</p><p>If this exclusion happened outside the UK, select the equivalent offence in the UK for where it took place.</p></div>'
                    WHERE name = '_Exclusion08';

                    UPDATE form_questions
                    SET
                        description = '<div class=""govuk-hint"">Where the contracting authority can contact someone about the exclusion</div>',
                        caption = NULL
                    WHERE name = '_Exclusion06';

                    UPDATE form_questions
                    SET
                        description = '<div class=""govuk-hint"">Give us your explanation of the event. For example, any background information you can give about what happened or what caused the exclusion.</div>',
                        caption = NULL
                    WHERE name = '_Exclusion05';

                    UPDATE form_questions
                    SET
                        description = '<div class=""govuk-hint""><p class=""govuk-body"">You must tell us what you or the person who was subject to the event:</p><ul class=""govuk-list govuk-list--bullet""><li>have done to prove it was taken seriously - for example, paid a fine or compensation</li><li>have done to stop the circumstances that caused it from happening again - for example, taking steps like changing staff or management or putting procedures or training in place</li><li>are doing to monitor the steps that were taken - for example, regular meetings</li></ul></div>',
                        caption = NULL
                    WHERE name = '_Exclusion04';
                END $$;
             ");
        }
    }
}
