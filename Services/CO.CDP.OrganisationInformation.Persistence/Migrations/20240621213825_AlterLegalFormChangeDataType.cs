using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlterLegalFormChangeDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"
                ALTER TABLE legal_forms 
                ALTER COLUMN registered_under_act2006 
                TYPE boolean 
                USING (CASE 
                          WHEN LOWER(registered_under_act2006) = 'true' THEN true 
                          WHEN LOWER(registered_under_act2006) = 'false' THEN false 
                          ELSE null 
                      END);
                "
           );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                ALTER TABLE legal_forms 
                ALTER COLUMN registered_under_act2006 
                TYPE varchar 
                USING (CASE 
                          WHEN registered_under_act2006 = true THEN 'true' 
                          WHEN registered_under_act2006 = false THEN 'false' 
                          ELSE null 
                      END);
                "
            );
        }
    }
}
