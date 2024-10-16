using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SupplierInformationLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_sections
                    SET title = 'ShareMyInformation_SectionTitle'
                    WHERE title = 'Share my information';

                    UPDATE form_sections
                    SET title = 'Qualifications_SectionTitle'
                    WHERE title = 'Qualifications';

                    UPDATE form_sections
                    SET title = 'TradeAssurances_SectionTitle'
                    WHERE title = 'Trade assurances';

                    UPDATE form_sections
                    SET title = 'Exclusions_SectionTitle'
                    WHERE title = 'Exclusions';

                    UPDATE form_sections
                    SET title = 'FinancialInformation_SectionTitle'
                    WHERE title = 'Financial Information';
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_sections                    
                    SET title = 'Share my information'
                    WHERE title = 'ShareMyInformation_SectionTitle';

                    UPDATE form_sections
                    SET title = 'Qualifications'
                    WHERE title = 'Qualifications_SectionTitle';

                    UPDATE form_sections
                    SET title = 'Trade assurances'
                    WHERE title = 'TradeAssurances_SectionTitle';

                    UPDATE form_sections
                    SET title = 'Exclusions'
                    WHERE title = 'Exclusions_SectionTitle';

                    UPDATE form_sections
                    SET title = 'Financial information'
                    WHERE title = 'FinancialInformation_SectionTitle';
                END $$;
             ");
        }
    }
}
