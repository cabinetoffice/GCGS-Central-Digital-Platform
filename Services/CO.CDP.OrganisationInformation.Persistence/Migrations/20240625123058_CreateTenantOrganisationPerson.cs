using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateTenantOrganisationPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    street_address = table.Column<string>(type: "text", nullable: false),
                    street_address2 = table.Column<string>(type: "text", nullable: true),
                    locality = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: true),
                    postal_code = table.Column<string>(type: "text", nullable: false),
                    country_name = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses", x => x.id);
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
                    user_urn = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.id);
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
                name: "organisations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    roles = table.Column<int[]>(type: "integer[]", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations", x => x.id);
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
                name: "contact_points",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    telephone = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    organisation_id = table.Column<int>(type: "integer", nullable: false)
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
                    identifier_id = table.Column<string>(type: "text", nullable: false),
                    scheme = table.Column<string>(type: "text", nullable: false),
                    legal_name = table.Column<string>(type: "text", nullable: false),
                    uri = table.Column<string>(type: "text", nullable: true),
                    primary = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    organisation_id = table.Column<int>(type: "integer", nullable: false)
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
                name: "organisation_address",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false)
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
                    completed_qualification = table.Column<bool>(type: "boolean", nullable: false),
                    completed_trade_assurance = table.Column<bool>(type: "boolean", nullable: false),
                    completed_operation_type = table.Column<bool>(type: "boolean", nullable: false),
                    completed_legal_form = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "qualifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    awarded_by_person_or_body_name = table.Column<string>(type: "text", nullable: false),
                    date_awarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    supplier_information_organisation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_qualifications_supplier_information_supplier_information_or",
                        column: x => x.supplier_information_organisation_id,
                        principalTable: "supplier_information",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trade_assurances",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    awarded_by_person_or_body_name = table.Column<string>(type: "text", nullable: false),
                    reference_number = table.Column<string>(type: "text", nullable: false),
                    date_awarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    supplier_information_organisation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trade_assurances", x => x.id);
                    table.ForeignKey(
                        name: "fk_trade_assurances_supplier_information_supplier_information_",
                        column: x => x.supplier_information_organisation_id,
                        principalTable: "supplier_information",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contact_points_organisation_id",
                table: "contact_points",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_organisation_id",
                table: "identifiers",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_address_address_id",
                table: "organisation_address",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_address_organisation_id",
                table: "organisation_address",
                column: "organisation_id");

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
                name: "ix_organisations_tenant_id",
                table: "organisations",
                column: "tenant_id");

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
                name: "ix_qualifications_guid",
                table: "qualifications",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_supplier_information_organisation_id",
                table: "qualifications",
                column: "supplier_information_organisation_id");

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
                name: "ix_trade_assurances_guid",
                table: "trade_assurances",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trade_assurances_supplier_information_organisation_id",
                table: "trade_assurances",
                column: "supplier_information_organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "buyer_information");

            migrationBuilder.DropTable(
                name: "contact_points");

            migrationBuilder.DropTable(
                name: "identifiers");

            migrationBuilder.DropTable(
                name: "legal_forms");

            migrationBuilder.DropTable(
                name: "organisation_address");

            migrationBuilder.DropTable(
                name: "organisation_person");

            migrationBuilder.DropTable(
                name: "qualifications");

            migrationBuilder.DropTable(
                name: "tenant_person");

            migrationBuilder.DropTable(
                name: "trade_assurances");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "supplier_information");

            migrationBuilder.DropTable(
                name: "organisations");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
