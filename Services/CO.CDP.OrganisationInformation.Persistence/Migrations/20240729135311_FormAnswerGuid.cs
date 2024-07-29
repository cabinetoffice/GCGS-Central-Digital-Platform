using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormAnswerGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_form_answer_sets_form_sections_section_id",
                table: "form_answer_sets");

            migrationBuilder.AddColumn<Guid>(
                name: "guid",
                table: "form_answers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "fk_form_answer_sets_form_section_section_id",
                table: "form_answer_sets",
                column: "section_id",
                principalTable: "form_sections",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_form_answer_sets_form_section_section_id",
                table: "form_answer_sets");

            migrationBuilder.DropColumn(
                name: "guid",
                table: "form_answers");

            migrationBuilder.AddForeignKey(
                name: "fk_form_answer_sets_form_sections_section_id",
                table: "form_answer_sets",
                column: "section_id",
                principalTable: "form_sections",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
