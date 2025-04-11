using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFormQuestionDescTitleCaption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN                                      

                    UPDATE  form_questions
                    SET     description = 'ShareMyInformation_01_Description',
                            caption = 'Global_Your_Declaration_Details',
                            summary_title = 'ShareMyInformation_01_SummaryTitle'
                    WHERE   name = '_ShareMyInformation01';

                    UPDATE  form_questions
                    SET     description = 'ShareMyInformation_02_Description',
                            caption = 'Global_Your_Declaration_Details',
                            summary_title = 'ShareMyInformation_02_SummaryTitle'
                    WHERE   name = '_ShareMyInformation02';

                    UPDATE  form_questions
                    SET     caption = 'Global_Your_Declaration_Details',
                            summary_title = 'ShareMyInformation_03_SummaryTitle'
                    WHERE   name = '_ShareMyInformation03';

                    UPDATE  form_questions
                    SET     description = 'ShareMyInformation_04_Description',
                            caption = 'Global_Your_Declaration_Details',
                            summary_title = 'ShareMyInformation_04_SummaryTitle'
                    WHERE   name = '_ShareMyInformation04';

                    UPDATE  form_questions
                    SET     description = 'ShareMyInformation_05_Description',
                            title = 'Global_Declaration'
                    WHERE   name = '_ShareMyInformation05';

                    UPDATE  form_questions
                    SET     caption = 'Global_Your_Declaration_Details',
                            summary_title = 'ShareMyInformationConsortium_01_SummaryTitle'
                    WHERE   name = '_ShareMyInformationConsortium01';

                    UPDATE  form_questions
                    SET     caption = 'Global_Your_Declaration_Details',
                            summary_title = 'ShareMyInformationConsortium_02_SummaryTitle'
                    WHERE   name = '_ShareMyInformationConsortium02';

                    UPDATE  form_questions
                    SET     caption = 'Global_Your_Declaration_Details',
                            summary_title = 'ShareMyInformationConsortium_03_SummaryTitle'
                    WHERE   name = '_ShareMyInformationConsortium03';

                    UPDATE  form_questions
                    SET     description = 'ShareMyInformationConsortium_04_Description',
                            caption = 'Global_Your_Declaration_Details',
                            summary_title = 'ShareMyInformationConsortium_04_SummaryTitle'
                    WHERE   name = '_ShareMyInformationConsortium04';

                    UPDATE  form_questions
                    SET     description = 'ShareMyInformationConsortium_05_Description',
                            title = 'Global_Declaration'
                    WHERE   name = '_ShareMyInformationConsortium05';
                    
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
