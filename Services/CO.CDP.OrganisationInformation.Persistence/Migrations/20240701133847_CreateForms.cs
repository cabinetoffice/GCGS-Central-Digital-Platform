using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    scope = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "form_sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    form_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_sections", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_sections_forms_form_id",
                        column: x => x.form_id,
                        principalTable: "forms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shared_consents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    form_id = table.Column<int>(type: "integer", nullable: false),
                    submission_state = table.Column<int>(type: "integer", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    form_version_id = table.Column<string>(type: "text", nullable: false),
                    booking_reference = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_consents", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_consents_forms_form_id",
                        column: x => x.form_id,
                        principalTable: "forms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_consents_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    next_question_id = table.Column<int>(type: "integer", nullable: true),
                    next_question_alternative_id = table.Column<int>(type: "integer", nullable: true),
                    section_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    options = table.Column<string>(type: "jsonb", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_questions_form_questions_next_question_alternative_id",
                        column: x => x.next_question_alternative_id,
                        principalTable: "form_questions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_form_questions_form_questions_next_question_id",
                        column: x => x.next_question_id,
                        principalTable: "form_questions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_form_questions_form_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "form_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_answer_sets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_answer_sets", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_answer_sets_form_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "form_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_answer_sets_shared_consents_shared_consent_id",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "form_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    form_answer_set_id = table.Column<int>(type: "integer", nullable: false),
                    bool_value = table.Column<bool>(type: "boolean", nullable: true),
                    numeric_value = table.Column<double>(type: "double precision", nullable: true),
                    date_value = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_value = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_value = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    text_value = table.Column<string>(type: "text", nullable: true),
                    option_value = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_answers_form_answer_sets_form_answer_set_id",
                        column: x => x.form_answer_set_id,
                        principalTable: "form_answer_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_answers_form_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "form_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_form_answer_sets_section_id",
                table: "form_answer_sets",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_answer_sets_shared_consent_id",
                table: "form_answer_sets",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_answers_form_answer_set_id",
                table: "form_answers",
                column: "form_answer_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_answers_question_id",
                table: "form_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_questions_next_question_alternative_id",
                table: "form_questions",
                column: "next_question_alternative_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_questions_next_question_id",
                table: "form_questions",
                column: "next_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_questions_section_id",
                table: "form_questions",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_sections_form_id",
                table: "form_sections",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_consents_form_id",
                table: "shared_consents",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_consents_organisation_id",
                table: "shared_consents",
                column: "organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "form_answers");

            migrationBuilder.DropTable(
                name: "form_answer_sets");

            migrationBuilder.DropTable(
                name: "form_questions");

            migrationBuilder.DropTable(
                name: "shared_consents");

            migrationBuilder.DropTable(
                name: "form_sections");

            migrationBuilder.DropTable(
                name: "forms");
        }
    }
}