using System;
using System.Collections.Generic;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQualityManagementSectionForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'QualityManagementQuestion_01_Title') THEN
                        UPDATE form_questions
                        SET ""options"" = '{""layout"": {""button"": {""style"": ""Start"", ""text"": ""Global_Start""}}}'::jsonb
                        WHERE title = 'QualityManagementQuestion_01_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'QualityManagementQuestion_03_Title') THEN
                        UPDATE form_questions
                        SET is_required  = true
                        WHERE title = 'QualityManagementQuestion_03_Title';
                    END IF;

                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'QualityManagementQuestion_01_Title') THEN
                        UPDATE form_questions
                        SET ""options"" = '{}'::jsonb
                        WHERE title = 'QualityManagementQuestion_01_Title';
                    END IF;

                    IF EXISTS (SELECT 1 FROM form_questions WHERE title = 'QualityManagementQuestion_03_Title') THEN
                        UPDATE form_questions
                        SET is_required  = false
                        WHERE title = 'QualityManagementQuestion_03_Title';
                    END IF;

                END $$;
            ");
        }
    }
}
