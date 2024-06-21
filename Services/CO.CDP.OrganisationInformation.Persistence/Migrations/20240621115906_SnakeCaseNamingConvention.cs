using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseNamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationPerson_Organisations_OrganisationId",
                table: "OrganisationPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationPerson_Persons_PersonId",
                table: "OrganisationPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_Tenants_TenantId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantPerson_Persons_PersonId",
                table: "TenantPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantPerson_Tenants_TenantId",
                table: "TenantPerson");

            migrationBuilder.DropTable(
                name: "BuyerInformation");

            migrationBuilder.DropTable(
                name: "ContactPoint");

            migrationBuilder.DropTable(
                name: "Identifier");

            migrationBuilder.DropTable(
                name: "LegalForm");

            migrationBuilder.DropTable(
                name: "OrganisationAddress");

            migrationBuilder.DropTable(
                name: "Qualification");

            migrationBuilder.DropTable(
                name: "TradeAssurance");

            migrationBuilder.DropTable(
                name: "SupplierInformation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Persons",
                table: "Persons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organisations",
                table: "Organisations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantPerson",
                table: "TenantPerson");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganisationPerson",
                table: "OrganisationPerson");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Address",
                table: "Address");

            migrationBuilder.RenameTable(
                name: "Tenants",
                newName: "tenants");

            migrationBuilder.RenameTable(
                name: "Persons",
                newName: "persons");

            migrationBuilder.RenameTable(
                name: "Organisations",
                newName: "organisations");

            migrationBuilder.RenameTable(
                name: "TenantPerson",
                newName: "tenant_person");

            migrationBuilder.RenameTable(
                name: "OrganisationPerson",
                newName: "organisation_person");

            migrationBuilder.RenameTable(
                name: "Address",
                newName: "addresses");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "tenants",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "tenants",
                newName: "guid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tenants",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "tenants",
                newName: "updated_on");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "tenants",
                newName: "created_on");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_Name",
                table: "tenants",
                newName: "ix_tenants_name");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_Guid",
                table: "tenants",
                newName: "ix_tenants_guid");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "persons",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "persons",
                newName: "guid");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "persons",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "persons",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserUrn",
                table: "persons",
                newName: "user_urn");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "persons",
                newName: "updated_on");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "persons",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "persons",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "persons",
                newName: "created_on");

            migrationBuilder.RenameIndex(
                name: "IX_Persons_Guid",
                table: "persons",
                newName: "ix_persons_guid");

            migrationBuilder.RenameIndex(
                name: "IX_Persons_Email",
                table: "persons",
                newName: "ix_persons_email");

            migrationBuilder.RenameColumn(
                name: "Roles",
                table: "organisations",
                newName: "roles");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "organisations",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "organisations",
                newName: "guid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "organisations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "organisations",
                newName: "updated_on");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "organisations",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "organisations",
                newName: "created_on");

            migrationBuilder.RenameIndex(
                name: "IX_Organisations_Name",
                table: "organisations",
                newName: "ix_organisations_name");

            migrationBuilder.RenameIndex(
                name: "IX_Organisations_Guid",
                table: "organisations",
                newName: "ix_organisations_guid");

            migrationBuilder.RenameIndex(
                name: "IX_Organisations_TenantId",
                table: "organisations",
                newName: "ix_organisations_tenant_id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "tenant_person",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "tenant_person",
                newName: "person_id");

            migrationBuilder.RenameIndex(
                name: "IX_TenantPerson_TenantId",
                table: "tenant_person",
                newName: "ix_tenant_person_tenant_id");

            migrationBuilder.RenameColumn(
                name: "Scopes",
                table: "organisation_person",
                newName: "scopes");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "organisation_person",
                newName: "updated_on");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "organisation_person",
                newName: "created_on");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "organisation_person",
                newName: "person_id");

            migrationBuilder.RenameColumn(
                name: "OrganisationId",
                table: "organisation_person",
                newName: "organisation_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrganisationPerson_PersonId",
                table: "organisation_person",
                newName: "ix_organisation_person_person_id");

            migrationBuilder.RenameColumn(
                name: "Region",
                table: "addresses",
                newName: "region");

            migrationBuilder.RenameColumn(
                name: "Locality",
                table: "addresses",
                newName: "locality");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "addresses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "addresses",
                newName: "updated_on");

            migrationBuilder.RenameColumn(
                name: "StreetAddress2",
                table: "addresses",
                newName: "street_address2");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "addresses",
                newName: "street_address");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "addresses",
                newName: "postal_code");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "addresses",
                newName: "created_on");

            migrationBuilder.RenameColumn(
                name: "CountryName",
                table: "addresses",
                newName: "country_name");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tenants",
                table: "tenants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_persons",
                table: "persons",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_organisations",
                table: "organisations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tenant_person",
                table: "tenant_person",
                columns: new[] { "person_id", "tenant_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_organisation_person",
                table: "organisation_person",
                columns: new[] { "organisation_id", "person_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_addresses",
                table: "addresses",
                column: "id");

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
                    registered_under_act2006 = table.Column<string>(type: "text", nullable: false),
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
                name: "ix_qualifications_guid",
                table: "qualifications",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_qualifications_supplier_information_organisation_id",
                table: "qualifications",
                column: "supplier_information_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_trade_assurances_guid",
                table: "trade_assurances",
                column: "guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trade_assurances_supplier_information_organisation_id",
                table: "trade_assurances",
                column: "supplier_information_organisation_id");

            migrationBuilder.AddForeignKey(
                name: "fk_organisation_person_organisations_organisation_id",
                table: "organisation_person",
                column: "organisation_id",
                principalTable: "organisations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_organisation_person_persons_person_id",
                table: "organisation_person",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_organisations_tenants_tenant_id",
                table: "organisations",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tenant_person_persons_person_id",
                table: "tenant_person",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tenant_person_tenants_tenant_id",
                table: "tenant_person",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organisation_person_organisations_organisation_id",
                table: "organisation_person");

            migrationBuilder.DropForeignKey(
                name: "fk_organisation_person_persons_person_id",
                table: "organisation_person");

            migrationBuilder.DropForeignKey(
                name: "fk_organisations_tenants_tenant_id",
                table: "organisations");

            migrationBuilder.DropForeignKey(
                name: "fk_tenant_person_persons_person_id",
                table: "tenant_person");

            migrationBuilder.DropForeignKey(
                name: "fk_tenant_person_tenants_tenant_id",
                table: "tenant_person");

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
                name: "qualifications");

            migrationBuilder.DropTable(
                name: "trade_assurances");

            migrationBuilder.DropTable(
                name: "supplier_information");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tenants",
                table: "tenants");

            migrationBuilder.DropPrimaryKey(
                name: "pk_persons",
                table: "persons");

            migrationBuilder.DropPrimaryKey(
                name: "pk_organisations",
                table: "organisations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tenant_person",
                table: "tenant_person");

            migrationBuilder.DropPrimaryKey(
                name: "pk_organisation_person",
                table: "organisation_person");

            migrationBuilder.DropPrimaryKey(
                name: "pk_addresses",
                table: "addresses");

            migrationBuilder.RenameTable(
                name: "tenants",
                newName: "Tenants");

            migrationBuilder.RenameTable(
                name: "persons",
                newName: "Persons");

            migrationBuilder.RenameTable(
                name: "organisations",
                newName: "Organisations");

            migrationBuilder.RenameTable(
                name: "tenant_person",
                newName: "TenantPerson");

            migrationBuilder.RenameTable(
                name: "organisation_person",
                newName: "OrganisationPerson");

            migrationBuilder.RenameTable(
                name: "addresses",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Tenants",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "guid",
                table: "Tenants",
                newName: "Guid");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Tenants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_on",
                table: "Tenants",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "Tenants",
                newName: "CreatedOn");

            migrationBuilder.RenameIndex(
                name: "ix_tenants_name",
                table: "Tenants",
                newName: "IX_Tenants_Name");

            migrationBuilder.RenameIndex(
                name: "ix_tenants_guid",
                table: "Tenants",
                newName: "IX_Tenants_Guid");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Persons",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "guid",
                table: "Persons",
                newName: "Guid");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Persons",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Persons",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_urn",
                table: "Persons",
                newName: "UserUrn");

            migrationBuilder.RenameColumn(
                name: "updated_on",
                table: "Persons",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Persons",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Persons",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "Persons",
                newName: "CreatedOn");

            migrationBuilder.RenameIndex(
                name: "ix_persons_guid",
                table: "Persons",
                newName: "IX_Persons_Guid");

            migrationBuilder.RenameIndex(
                name: "ix_persons_email",
                table: "Persons",
                newName: "IX_Persons_Email");

            migrationBuilder.RenameColumn(
                name: "roles",
                table: "Organisations",
                newName: "Roles");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Organisations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "guid",
                table: "Organisations",
                newName: "Guid");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Organisations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_on",
                table: "Organisations",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Organisations",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "Organisations",
                newName: "CreatedOn");

            migrationBuilder.RenameIndex(
                name: "ix_organisations_name",
                table: "Organisations",
                newName: "IX_Organisations_Name");

            migrationBuilder.RenameIndex(
                name: "ix_organisations_guid",
                table: "Organisations",
                newName: "IX_Organisations_Guid");

            migrationBuilder.RenameIndex(
                name: "ix_organisations_tenant_id",
                table: "Organisations",
                newName: "IX_Organisations_TenantId");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "TenantPerson",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "person_id",
                table: "TenantPerson",
                newName: "PersonId");

            migrationBuilder.RenameIndex(
                name: "ix_tenant_person_tenant_id",
                table: "TenantPerson",
                newName: "IX_TenantPerson_TenantId");

            migrationBuilder.RenameColumn(
                name: "scopes",
                table: "OrganisationPerson",
                newName: "Scopes");

            migrationBuilder.RenameColumn(
                name: "updated_on",
                table: "OrganisationPerson",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "OrganisationPerson",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "person_id",
                table: "OrganisationPerson",
                newName: "PersonId");

            migrationBuilder.RenameColumn(
                name: "organisation_id",
                table: "OrganisationPerson",
                newName: "OrganisationId");

            migrationBuilder.RenameIndex(
                name: "ix_organisation_person_person_id",
                table: "OrganisationPerson",
                newName: "IX_OrganisationPerson_PersonId");

            migrationBuilder.RenameColumn(
                name: "region",
                table: "Address",
                newName: "Region");

            migrationBuilder.RenameColumn(
                name: "locality",
                table: "Address",
                newName: "Locality");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Address",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_on",
                table: "Address",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "street_address2",
                table: "Address",
                newName: "StreetAddress2");

            migrationBuilder.RenameColumn(
                name: "street_address",
                table: "Address",
                newName: "StreetAddress");

            migrationBuilder.RenameColumn(
                name: "postal_code",
                table: "Address",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "Address",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "country_name",
                table: "Address",
                newName: "CountryName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Persons",
                table: "Persons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organisations",
                table: "Organisations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantPerson",
                table: "TenantPerson",
                columns: new[] { "PersonId", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganisationPerson",
                table: "OrganisationPerson",
                columns: new[] { "OrganisationId", "PersonId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Address",
                table: "Address",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BuyerInformation",
                columns: table => new
                {
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    BuyerType = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DevolvedRegulations = table.Column<int[]>(type: "integer[]", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerInformation", x => x.OrganisationId);
                    table.ForeignKey(
                        name: "FK_BuyerInformation_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    Telephone = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactPoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactPoint_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Identifier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IdentifierId = table.Column<string>(type: "text", nullable: false),
                    LegalName = table.Column<string>(type: "text", nullable: false),
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    Primary = table.Column<bool>(type: "boolean", nullable: false),
                    Scheme = table.Column<string>(type: "text", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Uri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identifier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Identifier_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationAddress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressId = table.Column<int>(type: "integer", nullable: false),
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationAddress_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganisationAddress_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierInformation",
                columns: table => new
                {
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    CompletedEmailAddress = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedLegalForm = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedOperationType = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedPostalAddress = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedQualification = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedRegAddress = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedTradeAssurance = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedVat = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedWebsiteAddress = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    OperationTypes = table.Column<int[]>(type: "integer[]", nullable: false),
                    SupplierType = table.Column<int>(type: "integer", nullable: true),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierInformation", x => x.OrganisationId);
                    table.ForeignKey(
                        name: "FK_SupplierInformation_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegalForm",
                columns: table => new
                {
                    SupplierInformationOrganisationId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LawRegistered = table.Column<string>(type: "text", nullable: false),
                    RegisteredLegalForm = table.Column<string>(type: "text", nullable: false),
                    RegisteredUnderAct2006 = table.Column<string>(type: "text", nullable: false),
                    RegistrationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalForm", x => x.SupplierInformationOrganisationId);
                    table.ForeignKey(
                        name: "FK_LegalForm_SupplierInformation_SupplierInformationOrganisati~",
                        column: x => x.SupplierInformationOrganisationId,
                        principalTable: "SupplierInformation",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Qualification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AwardedByPersonOrBodyName = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DateAwarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SupplierInformationOrganisationId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Qualification_SupplierInformation_SupplierInformationOrgani~",
                        column: x => x.SupplierInformationOrganisationId,
                        principalTable: "SupplierInformation",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeAssurance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AwardedByPersonOrBodyName = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DateAwarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: false),
                    SupplierInformationOrganisationId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeAssurance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeAssurance_SupplierInformation_SupplierInformationOrgan~",
                        column: x => x.SupplierInformationOrganisationId,
                        principalTable: "SupplierInformation",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactPoint_OrganisationId",
                table: "ContactPoint",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Identifier_OrganisationId",
                table: "Identifier",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAddress_AddressId",
                table: "OrganisationAddress",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAddress_OrganisationId",
                table: "OrganisationAddress",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualification_Guid",
                table: "Qualification",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Qualification_SupplierInformationOrganisationId",
                table: "Qualification",
                column: "SupplierInformationOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeAssurance_Guid",
                table: "TradeAssurance",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeAssurance_SupplierInformationOrganisationId",
                table: "TradeAssurance",
                column: "SupplierInformationOrganisationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationPerson_Organisations_OrganisationId",
                table: "OrganisationPerson",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationPerson_Persons_PersonId",
                table: "OrganisationPerson",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_Tenants_TenantId",
                table: "Organisations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantPerson_Persons_PersonId",
                table: "TenantPerson",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantPerson_Tenants_TenantId",
                table: "TenantPerson",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
