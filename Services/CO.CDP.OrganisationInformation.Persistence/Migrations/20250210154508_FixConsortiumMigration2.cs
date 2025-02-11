using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixConsortiumMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql($@"
            DO $$
            DECLARE
                sectionId INT;
            BEGIN
                
                SELECT id INTO sectionId FROM form_sections WHERE guid = '463998ef-faac-400c-b5f2-e7b24997d1a3';

                UPDATE form_questions SET section_id = sectionId, sort_order = 5 WHERE name = '_ShareMyInformationConsortium01';
                UPDATE form_questions SET section_id = sectionId, sort_order = 4 WHERE name = '_ShareMyInformationConsortium02';
                UPDATE form_questions SET section_id = sectionId, sort_order = 3 WHERE name = '_ShareMyInformationConsortium03';
                UPDATE form_questions SET section_id = sectionId, sort_order = 2 WHERE name = '_ShareMyInformationConsortium04';
                UPDATE form_questions SET section_id = sectionId, sort_order = 1 WHERE name = '_ShareMyInformationConsortium05';
                UPDATE form_questions SET section_id = sectionId, sort_order = 6 WHERE name = '_ShareMyInformationConsortium06';
            END $$;
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
