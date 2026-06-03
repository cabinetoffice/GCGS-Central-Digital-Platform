using System;
using System.Collections.Generic;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenUrnAndSalt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:connected_entity_individual_and_trust_category_type", "person_with_significant_control_for_indiv,director_or_indiv_with_the_same_responsibilities_for_indiv,any_other_indiv_with_significant_influence_or_control_for_indiv,person_with_significant_control_for_trust,director_or_indiv_with_the_same_responsibilities_for_trust,any_other_indiv_with_significant_influence_or_control_for_trust")
                .Annotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .Annotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:control_condition", "none,owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control")
                .Annotation("Npgsql:Enum:organisation_relationship", "consortium")
                .Annotation("Npgsql:Enum:organisation_type", "organisation,informal_consortium");

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    street_address = table.Column<string>(type: "text", nullable: false),
                    locality = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: true),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    country_name = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "addresses_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    street_address = table.Column<string>(type: "text", nullable: false),
                    locality = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: true),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    country_name = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    mapping_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses_snapshot", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "announcements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    url_regex = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_announcements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    client_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled_by_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    property_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    old_value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    new_value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    user_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

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
                    scope = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mou",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mou", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    published = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    queue_url = table.Column<string>(type: "text", nullable: false),
                    message_group_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: true),
                    user_urn = table.Column<string>(type: "text", nullable: false),
                    previous_urns = table.Column<List<string>>(type: "text[]", nullable: false),
                    scopes = table.Column<List<string>>(type: "text[]", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked = table.Column<bool>(type: "boolean", nullable: true),
                    user_urn = table.Column<string>(type: "text", nullable: true),
                    salt = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "application_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_permissions_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_roles_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    form_id = table.Column<int>(type: "integer", nullable: false),
                    allows_multiple_answer_sets = table.Column<bool>(type: "boolean", nullable: false),
                    check_further_questions_exempted = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    configuration = table.Column<string>(type: "jsonb", nullable: false)
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
                name: "organisations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    roles = table.Column<int[]>(type: "integer[]", nullable: false),
                    pending_roles = table.Column<int[]>(type: "integer[]", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    approved_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reviewed_by_id = table.Column<int>(type: "integer", nullable: true),
                    review_comment = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisations_persons_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "persons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_organisations_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_person",
                columns: table => new
                {
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_person", x => new { x.person_id, x.tenant_id });
                    table.ForeignKey(
                        name: "fk_tenant_person_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tenant_person_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    application_permission_id = table.Column<int>(type: "integer", nullable: false),
                    application_role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.application_permission_id, x.application_role_id });
                    table.ForeignKey(
                        name: "fk_role_permissions_application_permissions_application_permis",
                        column: x => x.application_permission_id,
                        principalTable: "application_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_application_roles_application_role_id",
                        column: x => x.application_role_id,
                        principalTable: "application_roles",
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
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    section_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    caption = table.Column<string>(type: "text", nullable: true),
                    options = table.Column<string>(type: "jsonb", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    summary_title = table.Column<string>(type: "text", nullable: true)
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
                name: "authentication_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: true),
                    revoked = table.Column<bool>(type: "boolean", nullable: false),
                    scopes = table.Column<string>(type: "jsonb", nullable: false),
                    revoked_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authentication_keys", x => x.id);
                    table.ForeignKey(
                        name: "fk_authentication_keys_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "buyer_information",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    buyer_type = table.Column<string>(type: "text", nullable: true),
                    devolved_regulations = table.Column<int[]>(type: "integer[]", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_buyer_information", x => x.id);
                    table.ForeignKey(
                        name: "fk_buyer_information_organisations_id",
                        column: x => x.id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_entities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<int>(type: "integer", nullable: false),
                    has_company_house_number = table.Column<bool>(type: "boolean", nullable: false),
                    company_house_number = table.Column<string>(type: "text", nullable: true),
                    overseas_company_number = table.Column<string>(type: "text", nullable: true),
                    registered_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    register_name = table.Column<string>(type: "text", nullable: true),
                    supplier_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_entities", x => x.id);
                    table.ForeignKey(
                        name: "fk_connected_entities_organisations_supplier_organisation_id",
                        column: x => x.supplier_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contact_points",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    telephone = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_points", x => x.id);
                    table.ForeignKey(
                        name: "fk_contact_points_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identifiers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    identifier_id = table.Column<string>(type: "text", nullable: true),
                    scheme = table.Column<string>(type: "text", nullable: false),
                    legal_name = table.Column<string>(type: "text", nullable: false),
                    uri = table.Column<string>(type: "text", nullable: true),
                    primary = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identifiers", x => x.id);
                    table.ForeignKey(
                        name: "fk_identifiers_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mou_email_reminders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    reminder_sent_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mou_email_reminders", x => x.id);
                    table.ForeignKey(
                        name: "fk_mou_email_reminders_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mou_signature",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    signature_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    job_title = table.Column<string>(type: "text", nullable: false),
                    mou_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mou_signature", x => x.id);
                    table.ForeignKey(
                        name: "fk_mou_signature_mou_mou_id",
                        column: x => x.mou_id,
                        principalTable: "mou",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mou_signature_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mou_signature_persons_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organisation_address",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_address", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_address_address_address_id",
                        column: x => x.address_id,
                        principalTable: "addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_address_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organisation_applications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    enabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    enabled_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_applications_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_applications_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organisation_hierarchies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    child_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    superseded_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_hierarchies", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_hierarchies_organisations_child_organisation_id",
                        column: x => x.child_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_hierarchies_organisations_parent_organisation_",
                        column: x => x.parent_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organisation_join_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reviewed_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reviewed_by_id = table.Column<int>(type: "integer", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_join_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_join_requests_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_join_requests_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_join_requests_persons_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "persons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "organisation_person",
                columns: table => new
                {
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    scopes = table.Column<string>(type: "jsonb", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_person", x => new { x.organisation_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_organisation_person_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_person_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_invites",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: true),
                    scopes = table.Column<List<string>>(type: "text[]", nullable: false),
                    invite_sent_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expires_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_invites", x => x.id);
                    table.ForeignKey(
                        name: "fk_person_invites_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_person_invites_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id");
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
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    form_version_id = table.Column<string>(type: "text", nullable: false),
                    share_code = table.Column<string>(type: "text", nullable: true),
                    created_from = table.Column<Guid>(type: "uuid", nullable: true),
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
                name: "supplier_information",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    supplier_type = table.Column<int>(type: "integer", nullable: true),
                    operation_types = table.Column<int[]>(type: "integer[]", nullable: false),
                    completed_reg_address = table.Column<bool>(type: "boolean", nullable: false),
                    completed_postal_address = table.Column<bool>(type: "boolean", nullable: false),
                    completed_vat = table.Column<bool>(type: "boolean", nullable: false),
                    completed_website_address = table.Column<bool>(type: "boolean", nullable: false),
                    completed_email_address = table.Column<bool>(type: "boolean", nullable: false),
                    completed_operation_type = table.Column<bool>(type: "boolean", nullable: false),
                    completed_legal_form = table.Column<bool>(type: "boolean", nullable: false),
                    completed_connected_person = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supplier_information", x => x.id);
                    table.ForeignKey(
                        name: "fk_supplier_information_organisations_id",
                        column: x => x.id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_entity_address",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    connected_entity_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_entity_address", x => new { x.connected_entity_id, x.id });
                    table.ForeignKey(
                        name: "fk_connected_entity_address_address_address_id",
                        column: x => x.address_id,
                        principalTable: "addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_connected_entity_address_connected_entities_connected_entit",
                        column: x => x.connected_entity_id,
                        principalTable: "connected_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_individual_trust",
                columns: table => new
                {
                    connected_individual_trust_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "text", nullable: true),
                    control_condition = table.Column<int[]>(type: "integer[]", nullable: false),
                    connected_type = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resident_country = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_individual_trust", x => x.connected_individual_trust_id);
                    table.ForeignKey(
                        name: "fk_connected_individual_trust_connected_entities_connected_ind",
                        column: x => x.connected_individual_trust_id,
                        principalTable: "connected_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_organisation",
                columns: table => new
                {
                    connected_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    insolvency_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    registered_legal_form = table.Column<string>(type: "text", nullable: true),
                    law_registered = table.Column<string>(type: "text", nullable: true),
                    control_condition = table.Column<int[]>(type: "integer[]", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_organisation", x => x.connected_organisation_id);
                    table.ForeignKey(
                        name: "fk_connected_organisation_connected_entities_connected_organis",
                        column: x => x.connected_organisation_id,
                        principalTable: "connected_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_application_assignments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_application_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_application_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_application_assignments_organisation_applications_orga",
                        column: x => x.organisation_application_id,
                        principalTable: "organisation_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_application_assignments_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_entities_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<int>(type: "integer", nullable: false),
                    has_company_house_number = table.Column<bool>(type: "boolean", nullable: false),
                    company_house_number = table.Column<string>(type: "text", nullable: true),
                    overseas_company_number = table.Column<string>(type: "text", nullable: true),
                    registered_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    register_name = table.Column<string>(type: "text", nullable: true),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    mapping_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_entities_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_connected_entities_snapshot_shared_consents_shared_consent_",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contact_points_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    telephone = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_points_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_contact_points_snapshot_shared_consents_shared_consent_id",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
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
                    shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    section_id = table.Column<int>(type: "integer", nullable: false),
                    further_questions_exempted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_from = table.Column<Guid>(type: "uuid", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_answer_sets", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_answer_sets_form_section_section_id",
                        column: x => x.section_id,
                        principalTable: "form_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_answer_sets_shared_consents_shared_consent_id",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identifiers_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    identifier_id = table.Column<string>(type: "text", nullable: true),
                    scheme = table.Column<string>(type: "text", nullable: false),
                    legal_name = table.Column<string>(type: "text", nullable: false),
                    uri = table.Column<string>(type: "text", nullable: true),
                    primary = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identifiers_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_identifiers_snapshot_shared_consents_shared_consent_id",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organisation_address_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_address_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_address_snapshot_address_snapshot_address_id",
                        column: x => x.address_id,
                        principalTable: "addresses_snapshot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_address_snapshot_shared_consents_shared_consen",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organisation_parties",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    child_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_relationship = table.Column<OrganisationRelationship>(type: "organisation_relationship", nullable: false),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_parties", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_parties_organisations_child_organisation_id",
                        column: x => x.child_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_parties_organisations_parent_organisation_id",
                        column: x => x.parent_organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organisation_parties_shared_consents_shared_consent_id",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "organisations_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisations_snapshot_shared_consents_shared_consent_id",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shared_consent_consortiums",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    child_shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_consent_consortiums", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_consent_consortiums_shared_consents_child_shared_con",
                        column: x => x.child_shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_consent_consortiums_shared_consents_parent_shared_co",
                        column: x => x.parent_shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_information_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_type = table.Column<int>(type: "integer", nullable: true),
                    operation_types = table.Column<int[]>(type: "integer[]", nullable: false),
                    completed_reg_address = table.Column<bool>(type: "boolean", nullable: false),
                    completed_postal_address = table.Column<bool>(type: "boolean", nullable: false),
                    completed_vat = table.Column<bool>(type: "boolean", nullable: false),
                    completed_website_address = table.Column<bool>(type: "boolean", nullable: false),
                    completed_email_address = table.Column<bool>(type: "boolean", nullable: false),
                    completed_operation_type = table.Column<bool>(type: "boolean", nullable: false),
                    completed_legal_form = table.Column<bool>(type: "boolean", nullable: false),
                    completed_connected_person = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supplier_information_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_supplier_information_snapshot_shared_consents_shared_consen",
                        column: x => x.shared_consent_id,
                        principalTable: "shared_consents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "legal_forms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    registered_under_act2006 = table.Column<bool>(type: "boolean", nullable: false),
                    registered_legal_form = table.Column<string>(type: "text", nullable: false),
                    law_registered = table.Column<string>(type: "text", nullable: false),
                    registration_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_legal_forms", x => x.id);
                    table.ForeignKey(
                        name: "fk_legal_forms_supplier_information_id",
                        column: x => x.id,
                        principalTable: "supplier_information",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_application_assignment_roles",
                columns: table => new
                {
                    application_role_id = table.Column<int>(type: "integer", nullable: false),
                    user_application_assignment_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_application_assignment_roles", x => new { x.application_role_id, x.user_application_assignment_id });
                    table.ForeignKey(
                        name: "fk_user_application_assignment_roles_application_roles_applica",
                        column: x => x.application_role_id,
                        principalTable: "application_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_application_assignment_roles_user_application_assignme",
                        column: x => x.user_application_assignment_id,
                        principalTable: "user_application_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_entity_address_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<int>(type: "integer", nullable: false),
                    connected_entity_snapshot_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_entity_address_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_connected_entity_address_snapshot_address_snapshot_address_",
                        column: x => x.address_id,
                        principalTable: "addresses_snapshot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_connected_entity_address_snapshot_connected_entity_snapshot",
                        column: x => x.connected_entity_snapshot_id,
                        principalTable: "connected_entities_snapshot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_individual_trust_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "text", nullable: true),
                    control_condition = table.Column<int[]>(type: "integer[]", nullable: false),
                    connected_type = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resident_country = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_individual_trust_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_connected_individual_trust_snapshot_connected_entity_snapsh",
                        column: x => x.id,
                        principalTable: "connected_entities_snapshot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_organisation_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    insolvency_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    registered_legal_form = table.Column<string>(type: "text", nullable: true),
                    law_registered = table.Column<string>(type: "text", nullable: true),
                    control_condition = table.Column<int[]>(type: "integer[]", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_organisation_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_connected_organisation_snapshot_connected_entity_snapshot_id",
                        column: x => x.id,
                        principalTable: "connected_entities_snapshot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    form_answer_set_id = table.Column<int>(type: "integer", nullable: false),
                    bool_value = table.Column<bool>(type: "boolean", nullable: true),
                    numeric_value = table.Column<double>(type: "double precision", nullable: true),
                    date_value = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_value = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_value = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    text_value = table.Column<string>(type: "text", nullable: true),
                    option_value = table.Column<string>(type: "text", nullable: true),
                    json_value = table.Column<string>(type: "jsonb", nullable: true),
                    address_value = table.Column<string>(type: "jsonb", nullable: true),
                    created_from = table.Column<Guid>(type: "uuid", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "legal_forms_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    registered_under_act2006 = table.Column<bool>(type: "boolean", nullable: false),
                    registered_legal_form = table.Column<string>(type: "text", nullable: false),
                    law_registered = table.Column<string>(type: "text", nullable: false),
                    registration_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_legal_forms_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_legal_forms_snapshot_supplier_information_snapshot_id",
                        column: x => x.id,
                        principalTable: "supplier_information_snapshot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_announcements_guid",
                table: "announcements",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_application_permissions_application_id_name",
                table: "application_permissions",
                columns: new[] { "application_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_application_roles_application_id_name",
                table: "application_roles",
                columns: new[] { "application_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_application_roles_guid",
                table: "application_roles",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_applications_client_id",
                table: "applications",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_applications_guid",
                table: "applications",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                table: "audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_id",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_authentication_keys_name_organisation_id",
                table: "authentication_keys",
                columns: new[] { "name", "organisation_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_authentication_keys_organisation_id",
                table: "authentication_keys",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_entities_guid",
                table: "connected_entities",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_connected_entities_supplier_organisation_id",
                table: "connected_entities",
                column: "supplier_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_entities_snapshot_shared_consent_id",
                table: "connected_entities_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_entity_address_address_id",
                table: "connected_entity_address",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_entity_address_snapshot_address_id",
                table: "connected_entity_address_snapshot",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_entity_address_snapshot_connected_entity_snapshot",
                table: "connected_entity_address_snapshot",
                column: "connected_entity_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_points_organisation_id",
                table: "contact_points",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_points_snapshot_shared_consent_id",
                table: "contact_points_snapshot",
                column: "shared_consent_id");

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
                name: "ix_form_questions_name",
                table: "form_questions",
                column: "name",
                unique: true);

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
                name: "ix_identifiers_organisation_id",
                table: "identifiers",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_snapshot_shared_consent_id",
                table: "identifiers_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_mou_guid",
                table: "mou",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mou_email_reminders_organisation_id",
                table: "mou_email_reminders",
                column: "organisation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mou_signature_created_by_id",
                table: "mou_signature",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_mou_signature_mou_id",
                table: "mou_signature",
                column: "mou_id");

            migrationBuilder.CreateIndex(
                name: "ix_mou_signature_organisation_id",
                table: "mou_signature",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_mou_signature_signature_guid",
                table: "mou_signature",
                column: "signature_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisation_address_address_id",
                table: "organisation_address",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_address_organisation_id",
                table: "organisation_address",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_address_snapshot_address_id",
                table: "organisation_address_snapshot",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_address_snapshot_shared_consent_id",
                table: "organisation_address_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_applications_application_id",
                table: "organisation_applications",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_applications_organisation_id_application_id",
                table: "organisation_applications",
                columns: new[] { "organisation_id", "application_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisation_hierarchies_child_organisation_id",
                table: "organisation_hierarchies",
                column: "child_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_hierarchies_parent_organisation_id",
                table: "organisation_hierarchies",
                column: "parent_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_hierarchies_parent_organisation_id_child_organ",
                table: "organisation_hierarchies",
                columns: new[] { "parent_organisation_id", "child_organisation_id", "superseded_on" },
                filter: "superseded_on IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_hierarchies_relationship_id",
                table: "organisation_hierarchies",
                column: "relationship_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisation_join_requests_guid",
                table: "organisation_join_requests",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisation_join_requests_organisation_id",
                table: "organisation_join_requests",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_join_requests_person_id",
                table: "organisation_join_requests",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_join_requests_reviewed_by_id",
                table: "organisation_join_requests",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_parties_child_organisation_id",
                table: "organisation_parties",
                column: "child_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_parties_parent_organisation_id",
                table: "organisation_parties",
                column: "parent_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_parties_shared_consent_id",
                table: "organisation_parties",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_person_person_id",
                table: "organisation_person",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_guid",
                table: "organisations",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisations_name",
                table: "organisations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisations_reviewed_by_id",
                table: "organisations",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_tenant_id",
                table: "organisations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_snapshot_shared_consent_id",
                table: "organisations_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_on",
                table: "outbox_messages",
                column: "created_on");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_published",
                table: "outbox_messages",
                column: "published");

            migrationBuilder.CreateIndex(
                name: "ix_person_invites_guid",
                table: "person_invites",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_invites_organisation_id",
                table: "person_invites",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_person_invites_person_id",
                table: "person_invites",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_persons_email",
                table: "persons",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persons_guid",
                table: "persons",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persons_user_urn",
                table: "persons",
                column: "user_urn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_application_role_id",
                table: "role_permissions",
                column: "application_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_consent_consortiums_child_shared_consent_id",
                table: "shared_consent_consortiums",
                column: "child_shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_consent_consortiums_parent_shared_consent_id_child_s",
                table: "shared_consent_consortiums",
                columns: new[] { "parent_shared_consent_id", "child_shared_consent_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shared_consents_form_id",
                table: "shared_consents",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_consents_organisation_id",
                table: "shared_consents",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_information_snapshot_shared_consent_id",
                table: "supplier_information_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_person_tenant_id",
                table: "tenant_person",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_guid",
                table: "tenants",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_name",
                table: "tenants",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_application_assignment_roles_user_application_assignme",
                table: "user_application_assignment_roles",
                column: "user_application_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_application_assignments_organisation_application_id",
                table: "user_application_assignments",
                column: "organisation_application_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_application_assignments_person_id_organisation_applica",
                table: "user_application_assignments",
                columns: new[] { "person_id", "organisation_application_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "announcements");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "authentication_keys");

            migrationBuilder.DropTable(
                name: "buyer_information");

            migrationBuilder.DropTable(
                name: "connected_entity_address");

            migrationBuilder.DropTable(
                name: "connected_entity_address_snapshot");

            migrationBuilder.DropTable(
                name: "connected_individual_trust");

            migrationBuilder.DropTable(
                name: "connected_individual_trust_snapshot");

            migrationBuilder.DropTable(
                name: "connected_organisation");

            migrationBuilder.DropTable(
                name: "connected_organisation_snapshot");

            migrationBuilder.DropTable(
                name: "contact_points");

            migrationBuilder.DropTable(
                name: "contact_points_snapshot");

            migrationBuilder.DropTable(
                name: "form_answers");

            migrationBuilder.DropTable(
                name: "identifiers");

            migrationBuilder.DropTable(
                name: "identifiers_snapshot");

            migrationBuilder.DropTable(
                name: "legal_forms");

            migrationBuilder.DropTable(
                name: "legal_forms_snapshot");

            migrationBuilder.DropTable(
                name: "mou_email_reminders");

            migrationBuilder.DropTable(
                name: "mou_signature");

            migrationBuilder.DropTable(
                name: "organisation_address");

            migrationBuilder.DropTable(
                name: "organisation_address_snapshot");

            migrationBuilder.DropTable(
                name: "organisation_hierarchies");

            migrationBuilder.DropTable(
                name: "organisation_join_requests");

            migrationBuilder.DropTable(
                name: "organisation_parties");

            migrationBuilder.DropTable(
                name: "organisation_person");

            migrationBuilder.DropTable(
                name: "organisations_snapshot");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "person_invites");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "shared_consent_consortiums");

            migrationBuilder.DropTable(
                name: "tenant_person");

            migrationBuilder.DropTable(
                name: "user_application_assignment_roles");

            migrationBuilder.DropTable(
                name: "connected_entities");

            migrationBuilder.DropTable(
                name: "connected_entities_snapshot");

            migrationBuilder.DropTable(
                name: "form_answer_sets");

            migrationBuilder.DropTable(
                name: "form_questions");

            migrationBuilder.DropTable(
                name: "supplier_information");

            migrationBuilder.DropTable(
                name: "supplier_information_snapshot");

            migrationBuilder.DropTable(
                name: "mou");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "addresses_snapshot");

            migrationBuilder.DropTable(
                name: "application_permissions");

            migrationBuilder.DropTable(
                name: "application_roles");

            migrationBuilder.DropTable(
                name: "user_application_assignments");

            migrationBuilder.DropTable(
                name: "form_sections");

            migrationBuilder.DropTable(
                name: "shared_consents");

            migrationBuilder.DropTable(
                name: "organisation_applications");

            migrationBuilder.DropTable(
                name: "forms");

            migrationBuilder.DropTable(
                name: "applications");

            migrationBuilder.DropTable(
                name: "organisations");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
