using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QuestionSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "form_questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    UPDATE form_questions SET sort_order = 1 WHERE name='_FinancialInformation04';
                    UPDATE form_questions SET sort_order = 2 WHERE name='_FinancialInformation03';
                    UPDATE form_questions SET sort_order = 3 WHERE name='_FinancialInformation02';
                    UPDATE form_questions SET sort_order = 4 WHERE name='_FinancialInformation01';
                    UPDATE form_questions SET sort_order = 5 WHERE name='_FinancialInformation05';

                    UPDATE form_questions SET sort_order = 1 WHERE name='_Exclusion07';
                    UPDATE form_questions SET sort_order = 2 WHERE name='_Exclusion06';
                    UPDATE form_questions SET sort_order = 3 WHERE name='_Exclusion05';
                    UPDATE form_questions SET sort_order = 4 WHERE name='_Exclusion04';
                    UPDATE form_questions SET sort_order = 5 WHERE name='_Exclusion03';
                    UPDATE form_questions SET sort_order = 6 WHERE name='_Exclusion02';
                    UPDATE form_questions SET sort_order = 7 WHERE name='_Exclusion01';

                    UPDATE form_questions SET sort_order = 1 WHERE name='_Qualifications01';
                    UPDATE form_questions SET sort_order = 2 WHERE name='_Qualifications02';
                    UPDATE form_questions SET sort_order = 3 WHERE name='_Qualifications03';
                    UPDATE form_questions SET sort_order = 4 WHERE name='_Qualifications04';

                    UPDATE form_questions SET sort_order = 1 WHERE name='_TradeAssurance01';
                    UPDATE form_questions SET sort_order = 2 WHERE name='_TradeAssurance02';
                    UPDATE form_questions SET sort_order = 3 WHERE name='_TradeAssurance03';
                    UPDATE form_questions SET sort_order = 4 WHERE name='_TradeAssurance04';

                    UPDATE form_questions SET sort_order = 1 WHERE name='_ShareMyInformation05';
                    UPDATE form_questions SET sort_order = 2 WHERE name='_ShareMyInformation04';
                    UPDATE form_questions SET sort_order = 3 WHERE name='_ShareMyInformation03';
                    UPDATE form_questions SET sort_order = 4 WHERE name='_ShareMyInformation02';
                    UPDATE form_questions SET sort_order = 5 WHERE name='_ShareMyInformation01';
                    UPDATE form_questions SET sort_order = 6 WHERE name='_ShareMyInformation06';

                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "form_questions");
        }
    }
}
