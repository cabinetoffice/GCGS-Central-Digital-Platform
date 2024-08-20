using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormsDataSeedRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                DECLARE
	                financial_info_form_id int;
	                share_code_form_id int;
                BEGIN
                    SELECT id INTO financial_info_form_id FROM forms WHERE guid = '0618b13e-eaf2-46e3-a7d2-6f2c44be7022';
	                SELECT id INTO share_code_form_id FROM forms WHERE guid = '049df96e-f6ea-423c-9361-ecb20c0250ea';      

	                UPDATE shared_consents SET form_id = financial_info_form_id WHERE form_id = share_code_form_id;
	                UPDATE form_sections SET form_id = financial_info_form_id WHERE form_id = share_code_form_id;
	                DELETE FROM forms WHERE id = share_code_form_id;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                INSERT INTO forms (guid, name, version, is_required, type, scope)
                VALUES ('049df96e-f6ea-423c-9361-ecb20c0250ea', 'Declaration Information Form', '1.0', TRUE, 0, 0);
            ");

            migrationBuilder.Sql($@"
                DO $$
                DECLARE
	                share_code_form_id int;
                begin
	                SELECT currval(pg_get_serial_sequence('forms', 'id')) INTO share_code_form_id;
	                UPDATE form_sections SET form_id = share_code_form_id WHERE title = 'Declaration Information';
                END $$;
            ");
        }
    }
}
