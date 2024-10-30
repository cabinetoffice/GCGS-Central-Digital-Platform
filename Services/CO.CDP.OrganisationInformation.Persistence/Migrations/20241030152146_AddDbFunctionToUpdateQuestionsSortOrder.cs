using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDbFunctionToUpdateQuestionsSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                CREATE OR REPLACE FUNCTION update_form_questions_sort_order()
                RETURNS TRIGGER AS $$
                BEGIN
                
                  IF pg_trigger_depth() = 1 THEN
                    WITH RECURSIVE SectionQuestions AS (
                      SELECT q.id, q.next_question_id, q.section_id, 1 AS sort_order
                      FROM form_questions q
                      WHERE NOT EXISTS (
                          SELECT 1
                          FROM form_questions
                          WHERE next_question_id = q.id
                          AND section_id = q.section_id
                        )
                      UNION ALL
                        SELECT q.id, q.next_question_id, q.section_id, sq.sort_order + 1
                        FROM form_questions q
                        JOIN SectionQuestions sq
                        ON q.id = sq.next_question_id
                        AND q.section_id = sq.section_id
                    )
                    
                    UPDATE form_questions q
                    SET sort_order = sq.sort_order
                    FROM SectionQuestions sq
                    WHERE q.id = sq.id;
                  END IF;
                  
                  RETURN NEW;
                  
                END;
                $$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DROP FUNCTION IF EXISTS update_form_questions_sort_order();
            ");
        }
    }
}
