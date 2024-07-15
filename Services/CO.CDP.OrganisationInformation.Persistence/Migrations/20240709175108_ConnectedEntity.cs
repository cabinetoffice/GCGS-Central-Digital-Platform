using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConnectedEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .Annotation("Npgsql:Enum:connected_person_category", "person_with_significant_control,director_or_individual_with_the_same_responsibilities,any_other_individual_with_significant_influence_or_control")
                .Annotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .Annotation("Npgsql:Enum:control_condition", "owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control");

            migrationBuilder.AddColumn<bool>(
                name: "completed_connected_person",
                table: "supplier_information",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "connected_entities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<int>(type: "integer", nullable: false),
                    has_compnay_house_number = table.Column<bool>(type: "boolean", nullable: false),
                    company_house_number = table.Column<string>(type: "text", nullable: true),
                    overseas_company_number = table.Column<string>(type: "text", nullable: true),
                    registered_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    register_name = table.Column<string>(type: "text", nullable: true),
                    supplier_organisation_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                name: "ix_connected_entity_address_address_id",
                table: "connected_entity_address",
                column: "address_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "connected_entity_address");

            migrationBuilder.DropTable(
                name: "connected_individual_trust");

            migrationBuilder.DropTable(
                name: "connected_organisation");

            migrationBuilder.DropTable(
                name: "connected_entities");

            migrationBuilder.DropColumn(
                name: "completed_connected_person",
                table: "supplier_information");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:connected_entity_type", "organisation,individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:connected_organisation_category", "registered_company,director_or_the_same_responsibilities,parent_or_subsidiary_company,a_company_your_organisation_has_taken_over,any_other_organisation_with_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:connected_person_category", "person_with_significant_control,director_or_individual_with_the_same_responsibilities,any_other_individual_with_significant_influence_or_control")
                .OldAnnotation("Npgsql:Enum:connected_person_type", "individual,trust_or_trustee")
                .OldAnnotation("Npgsql:Enum:control_condition", "owns_shares,has_voting_rights,can_appoint_or_remove_directors,has_other_significant_influence_or_control");
        }
    }
}