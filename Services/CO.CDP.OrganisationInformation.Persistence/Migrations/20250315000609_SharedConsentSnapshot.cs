using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SharedConsentSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "connected_entities_snapshot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shared_consent_id = table.Column<int>(type: "integer", nullable: false),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<int>(type: "integer", nullable: false),
                    has_compnay_house_number = table.Column<bool>(type: "boolean", nullable: false),
                    company_house_number = table.Column<string>(type: "text", nullable: true),
                    overseas_company_number = table.Column<string>(type: "text", nullable: true),
                    registered_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    register_name = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                name: "ix_connected_entities_snapshot_shared_consent_id",
                table: "connected_entities_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_entity_address_snapshot_address_id",
                table: "connected_entity_address_snapshot",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_entity_address_snapshot_connected_entity_snapshot",
                table: "connected_entity_address_snapshot",
                column: "connected_entity_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_points_snapshot_shared_consent_id",
                table: "contact_points_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_identifiers_snapshot_shared_consent_id",
                table: "identifiers_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_address_snapshot_address_id",
                table: "organisation_address_snapshot",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_address_snapshot_shared_consent_id",
                table: "organisation_address_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_snapshot_shared_consent_id",
                table: "organisations_snapshot",
                column: "shared_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_information_snapshot_shared_consent_id",
                table: "supplier_information_snapshot",
                column: "shared_consent_id");

            migrationBuilder.Sql($@"
CREATE OR REPLACE PROCEDURE create_shared_consent_snapshot(
    IN p_share_code TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE v_shared_consent_id INT;
DECLARE v_organisation_id INT;
DECLARE v_inserted_id INT;
BEGIN
    SELECT id, organisation_id 
    INTO v_shared_consent_id, v_organisation_id
    FROM shared_consents
    WHERE share_code = p_share_code;

    -- Organisation
    INSERT INTO organisations_snapshot
		(shared_consent_id, ""name"")
    SELECT 
    	v_shared_consent_id, ""name""
   	FROM 
   		organisations 
  	WHERE 
  		id = v_organisation_id;

    -- Contact Points
    INSERT INTO contact_points_snapshot
		(shared_consent_id, ""name"", email, telephone, url, created_on, updated_on)
    SELECT 
    	v_shared_consent_id, ""name"", email, telephone, url, created_on, updated_on 
   	FROM 
   		contact_points 
  	WHERE 
  		organisation_id = v_organisation_id;

    -- Identifiers
  	INSERT INTO identifiers_snapshot
		(shared_consent_id, identifier_id, scheme, legal_name, uri, ""primary"", created_on, updated_on)
	SELECT 
		v_shared_consent_id, identifier_id, scheme, legal_name, uri, ""primary"", created_on, updated_on
	FROM 
		identifiers 
	WHERE 
		organisation_id = v_organisation_id;
	
    -- Supplier information
	INSERT INTO supplier_information_snapshot
		(shared_consent_id, supplier_type, operation_types, completed_reg_address, completed_postal_address, 
		completed_vat, completed_website_address, completed_email_address, completed_operation_type, 
		completed_legal_form, completed_connected_person, created_on, updated_on)
    SELECT 
        v_shared_consent_id, supplier_type, operation_types, completed_reg_address, completed_postal_address, 
        completed_vat, completed_website_address, completed_email_address, completed_operation_type, 
        completed_legal_form, completed_connected_person, created_on, updated_on
    FROM 
        supplier_information
    WHERE 
        id = v_organisation_id
   	RETURNING id INTO v_inserted_id;
	
    -- Legal forms
	INSERT INTO legal_forms_snapshot
		(id, registered_under_act2006, registered_legal_form, law_registered, registration_date, created_on, updated_on)
    SELECT 
        v_inserted_id, registered_under_act2006, registered_legal_form, law_registered, registration_date, created_on, updated_on
    FROM 
        legal_forms
    WHERE 
        id = v_organisation_id;

	-- Organisation addessess
 	CREATE TEMP TABLE temp_inserted_address_data (id INT);
	WITH inserted_address_data AS (
		INSERT INTO addresses_snapshot
			(street_address, locality, region, postal_code, country_name, 
			country, created_on, updated_on, mapping_id)
		SELECT 
            a.street_address, a.locality, a.region, a.postal_code, a.country_name, 
            a.country, a.created_on, a.updated_on, oa.id
        FROM 
            organisation_address AS oa 
            INNER JOIN addresses AS a ON a.id = oa.address_id 
        WHERE 
            oa.organisation_id = v_organisation_id
		RETURNING id
	)
	INSERT INTO temp_inserted_address_data (id)
	SELECT id FROM inserted_address_data;

	INSERT INTO organisation_address_snapshot
		(shared_consent_id, ""type"", address_id)
	SELECT 
        v_shared_consent_id, oa.""type"", a.id
    FROM 
        organisation_address AS oa 
        INNER JOIN addresses_snapshot AS a on a.mapping_id = oa.id
        INNER JOIN temp_inserted_address_data AS iad ON iad.id = a.id;
	
    -- Connected entities
	CREATE TEMP TABLE temp_inserted_connected_entities_data (id INT);
	WITH inserted_connected_entities_data AS (
		INSERT INTO connected_entities_snapshot
			(shared_consent_id, ""guid"", entity_type, has_compnay_house_number, company_house_number, overseas_company_number, 
			registered_date, register_name, start_date, end_date, created_on, updated_on, mapping_id)
	    SELECT 
	        v_shared_consent_id, ""guid"", entity_type, has_compnay_house_number, company_house_number, overseas_company_number, 
			registered_date, register_name, start_date, end_date, created_on, updated_on, id
	    FROM 
	        connected_entities
	    WHERE 
	        supplier_organisation_id = v_organisation_id
        RETURNING id
    )
	INSERT INTO temp_inserted_connected_entities_data (id)
	SELECT id FROM inserted_connected_entities_data;
   
    -- Connected entities individual trust
	INSERT INTO connected_individual_trust_snapshot
		(id, category, first_name, last_name, date_of_birth, nationality, ""control_condition"", 
		connected_type, person_id, resident_country, created_on, updated_on)
	SELECT
    	ces.id, cit.category, cit.first_name, cit.last_name, cit.date_of_birth, cit.nationality, cit.""control_condition"", 
    	cit.connected_type, cit.person_id, cit.resident_country, cit.created_on, cit.updated_on
	FROM 
        connected_entities AS ce
        INNER JOIN connected_individual_trust cit ON cit.connected_individual_trust_id = ce.id
        INNER JOIN connected_entities_snapshot ces ON ces.mapping_id = ce.id
        INNER JOIN temp_inserted_connected_entities_data vid ON vid.id = ces.id;
   
    -- Connected entities organisation
	INSERT INTO connected_organisation_snapshot
		(id, category, ""name"", insolvency_date, registered_legal_form, 
		law_registered, ""control_condition"", organisation_id, created_on, updated_on)
	SELECT
    	ces.id, co.category, co.""name"", co.insolvency_date, co.registered_legal_form, 
		co.law_registered, co.""control_condition"", co.organisation_id, co.created_on, co.updated_on
	FROM 
        connected_entities AS ce
        INNER JOIN connected_organisation AS co ON co.connected_organisation_id = ce.id
        INNER JOIN connected_entities_snapshot ces ON ces.mapping_id = ce.id
        INNER JOIN temp_inserted_connected_entities_data vid ON vid.id = ces.id 
    WHERE 
        ce.supplier_organisation_id = v_organisation_id;
   
	CREATE TEMP TABLE temp_connected_entities_address_map (
		id SERIAL PRIMARY key,
		connected_entity_id INT,
		connected_entity_address_type INT,
		connected_entity_address_address_id INT,
		connected_entity_snapshot_id INT);
	
	INSERT INTO temp_connected_entities_address_map 
		(connected_entity_id, connected_entity_address_type, connected_entity_address_address_id, connected_entity_snapshot_id)
	SELECT 
	    ce.id, cea.""type"", cea.address_id, tice.id 
	FROM 
	    connected_entities AS ce
	    INNER JOIN connected_entity_address cea ON cea.connected_entity_id = ce.id
	    INNER JOIN addresses AS a ON a.id = cea.address_id 
	    INNER JOIN connected_entities_snapshot AS ces ON ces.mapping_id = ce.id 
	    INNER JOIN temp_inserted_connected_entities_data tice on tice.id = ces.id
	WHERE 
	    ce.supplier_organisation_id = v_organisation_id;
 
	-- Connected entity addessesss
	CREATE TEMP TABLE temp_inserted_connected_entities_address_data (id INT);
	WITH inserted_connected_entities_address_data AS (
		INSERT INTO addresses_snapshot
			(street_address, locality, region, postal_code, country_name, 
			country, created_on, updated_on, mapping_id)
		SELECT 
	        a.street_address, a.locality, a.region, a.postal_code, a.country_name, 
	        a.country, a.created_on, a.updated_on, tcea.id
	    FROM 
	        temp_connected_entities_address_map tcea
	        INNER JOIN addresses AS a ON a.id = tcea.connected_entity_address_address_id
        RETURNING id
    )
	INSERT INTO temp_inserted_connected_entities_address_data (id)
	SELECT id FROM inserted_connected_entities_address_data;
	
	INSERT INTO connected_entity_address_snapshot
		(connected_entity_snapshot_id, ""type"", address_id)
	SELECT 
        tcea.connected_entity_snapshot_id, tcea.connected_entity_address_type, a.id
    FROM 
        temp_connected_entities_address_map AS tcea
        INNER JOIN addresses_snapshot AS a on a.mapping_id = tcea.id
	    INNER JOIN temp_inserted_connected_entities_address_data ticea on ticea.id = a.id;
       
   	DROP TABLE IF EXISTS temp_inserted_address_data;
	DROP TABLE IF EXISTS temp_inserted_connected_entities_data;
	DROP TABLE IF EXISTS temp_connected_entities_address_map;
	DROP TABLE IF EXISTS temp_inserted_connected_entities_address_data;
END;
$$;
             ");

            migrationBuilder.Sql($@"
CREATE OR REPLACE PROCEDURE get_shared_consent_details(
    IN p_share_code TEXT, 
    INOUT consent_cursor REFCURSOR, 
    INOUT identifier_cursor REFCURSOR, 
    INOUT address_cursor REFCURSOR, 
    INOUT contact_cursor REFCURSOR, 
    INOUT connected_entities_cursor REFCURSOR, 
    INOUT connected_entities_address_cursor REFCURSOR, 
    INOUT form_answer_sets_cursor REFCURSOR, 
    INOUT form_question_cursor REFCURSOR, 
    INOUT form_answer_cursor REFCURSOR
)
LANGUAGE plpgsql
AS $$
DECLARE v_shared_consent_id INT;
DECLARE v_submitted_at TIMESTAMPTZ;
BEGIN
    SELECT id, submitted_at 
    INTO v_shared_consent_id, v_submitted_at
    FROM shared_consents
    WHERE share_code = p_share_code;

    -- Cursor for Shared Consent and Organisation Details
    OPEN consent_cursor FOR 
        SELECT 
            s.""guid"", s.submitted_at, s.submission_state, s.share_code, 
            f.""name"" AS form_name, f.""version"", f.is_required, 
            o.""guid"" AS organisation_guid, os.""name"" AS organisation_name, o.""type"", o.roles,
            s0.supplier_type, s0.operation_types, s0.completed_reg_address, s0.completed_postal_address, 
            s0.completed_vat, s0.completed_website_address, s0.completed_email_address, s0.completed_operation_type, 
            s0.completed_legal_form, s0.completed_connected_person,
			l.registered_under_act2006, l.registered_legal_form, l.law_registered, l.registration_date
        FROM 
            shared_consents AS s
            INNER JOIN forms AS f ON s.form_id = f.id
            INNER JOIN organisations AS o ON s.organisation_id = o.id
            INNER JOIN organisations_snapshot AS os ON os.shared_consent_id = s.id
            LEFT JOIN supplier_information_snapshot AS s0 ON s0.shared_consent_id = s.id
            LEFT JOIN legal_forms_snapshot AS l ON s0.id = l.id
        WHERE 
            s.share_code = p_share_code;

    -- Cursor for Identifiers
    OPEN identifier_cursor FOR 
        SELECT identifier_id, scheme, legal_name, uri, ""primary"" 
       	FROM identifiers_snapshot 
		WHERE shared_consent_id = v_shared_consent_id;

    -- Cursor for Organisation Addresses
    OPEN address_cursor FOR 
        SELECT 
            oa.""type"", a.street_address, a.locality, 
            a.region, a.postal_code, a.country_name, a.country
        FROM 
            organisation_address_snapshot AS oa 
            LEFT JOIN addresses_snapshot AS a ON a.id = oa.address_id 
        WHERE 
            oa.shared_consent_id = v_shared_consent_id;

    -- Cursor for Contact Points
    OPEN contact_cursor FOR 
        SELECT email, name, telephone, url 
       	FROM contact_points_snapshot 
       	WHERE shared_consent_id = v_shared_consent_id;

    -- Cursor for Connected Entities
    OPEN connected_entities_cursor FOR 
        SELECT
        	ce.id, ce.""guid"", ce.entity_type, ce.has_compnay_house_number, ce.company_house_number, 
        	ce.overseas_company_number, ce.registered_date, ce.register_name, ce.""start_date"", ce.end_date,
        	cit.category AS individual_and_trust_category, cit.first_name, cit.last_name, cit.date_of_birth, 
        	cit.nationality, cit.control_condition AS individual_and_trust_control_condition, 
        	cit.connected_type AS connected_person_type, cit.resident_country,
        	co.category AS organisation_category, co.name AS organisation_name, co.insolvency_date,
        	co.registered_legal_form, co.law_registered, co.control_condition AS organisation_control_condition
    	FROM 
            connected_entities_snapshot AS ce
            LEFT JOIN connected_individual_trust_snapshot cit ON cit.id = ce.id
            LEFT JOIN connected_organisation_snapshot AS co ON co.id = ce.id 
        WHERE 
            ce.shared_consent_id = v_shared_consent_id
            AND ce.created_on < v_submitted_at
            AND (ce.end_date IS NULL OR ce.end_date > v_submitted_at); 

    -- Cursor for Connected Entities Address
    OPEN connected_entities_address_cursor FOR 
        SELECT
        	ce.id, cea.""type"", a.street_address, a.locality, 
            a.region, a.postal_code, a.country_name, a.country
    	FROM 
            connected_entities_snapshot AS ce
            INNER JOIN connected_entity_address_snapshot cea ON cea.connected_entity_snapshot_id = ce.id
            INNER JOIN addresses_snapshot AS a ON a.id = cea.address_id
        WHERE 
            ce.shared_consent_id = v_shared_consent_id
            AND ce.created_on < v_submitted_at
            AND (ce.end_date IS NULL OR ce.end_date > v_submitted_at); 

    -- Cursor for Form Answer Sets
    OPEN form_answer_sets_cursor FOR 
        SELECT 
       		fas.id AS form_answer_set_id, fas.""guid"" AS form_answer_set_guid, fs.id, fs.title, fs.""type""
       	FROM 
       		form_answer_sets AS fas
            INNER JOIN form_sections AS fs ON fs.id = fas.section_id
       	WHERE 
       		fas.shared_consent_id = v_shared_consent_id and NOT fas.deleted AND fs.""type"" <> 1;

    -- Cursor for Form Question
    OPEN form_question_cursor FOR 
        SELECT DISTINCT ON (fq.id)
			fq.id, fq.""guid"", fq.sort_order, fq.""type"", fq.is_required, fq.""name"", 
			fq.title, fq.summary_title, fq.""description"", fq.""options"", fq.section_id
        FROM 
            form_answer_sets AS fas
            INNER JOIN form_sections AS fs ON fs.id = fas.section_id
            INNER JOIN form_questions AS fq ON fq.section_id = fs.id
        WHERE  
            fas.shared_consent_id = v_shared_consent_id and NOT fas.deleted AND fs.""type"" <> 1;

    -- Cursor for Form Answer
    OPEN form_answer_cursor FOR 
        SELECT 
			fa.question_id, fa.form_answer_set_id, fa.bool_value, fa.numeric_value, fa.date_value, 
			fa.start_value, fa.end_value, fa.text_value, fa.option_value, fa.""json_value"", fa.address_value
        FROM 
            form_answer_sets AS fas
            INNER JOIN form_sections AS fs ON fs.id = fas.section_id
            INNER JOIN form_answers AS fa ON fa.form_answer_set_id = fas.id
        WHERE  
            fas.shared_consent_id = v_shared_consent_id and NOT fas.deleted AND fs.""type"" <> 1;
END;
$$;
             ");

            migrationBuilder.Sql($@"
DO $$ 
DECLARE 
    record_value TEXT;
BEGIN
    FOR record_value IN 
        SELECT share_code FROM shared_consents WHERE share_code IS NOT NULL
    LOOP
        CALL create_shared_consent_snapshot(record_value);
    END LOOP;
END $$;
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "connected_entity_address_snapshot");

            migrationBuilder.DropTable(
                name: "connected_individual_trust_snapshot");

            migrationBuilder.DropTable(
                name: "connected_organisation_snapshot");

            migrationBuilder.DropTable(
                name: "contact_points_snapshot");

            migrationBuilder.DropTable(
                name: "identifiers_snapshot");

            migrationBuilder.DropTable(
                name: "legal_forms_snapshot");

            migrationBuilder.DropTable(
                name: "organisation_address_snapshot");

            migrationBuilder.DropTable(
                name: "organisations_snapshot");

            migrationBuilder.DropTable(
                name: "connected_entities_snapshot");

            migrationBuilder.DropTable(
                name: "supplier_information_snapshot");

            migrationBuilder.DropTable(
                name: "addresses_snapshot");

            migrationBuilder.Sql("DROP PROCEDURE create_shared_consent_snapshot");

            migrationBuilder.Sql($@"
CREATE OR REPLACE PROCEDURE get_shared_consent_details(
    IN p_share_code TEXT, 
    INOUT consent_cursor REFCURSOR, 
    INOUT identifier_cursor REFCURSOR, 
    INOUT address_cursor REFCURSOR, 
    INOUT contact_cursor REFCURSOR, 
    INOUT connected_entities_cursor REFCURSOR, 
    INOUT connected_entities_address_cursor REFCURSOR, 
    INOUT form_answer_sets_cursor REFCURSOR, 
    INOUT form_question_cursor REFCURSOR, 
    INOUT form_answer_cursor REFCURSOR
)
LANGUAGE plpgsql
AS $$
DECLARE v_shared_consent_id INT;
DECLARE v_organisation_id INT;
DECLARE v_submitted_at TIMESTAMPTZ;
BEGIN
    SELECT id, organisation_id, submitted_at 
    INTO v_shared_consent_id, v_organisation_id, v_submitted_at
    FROM shared_consents
    WHERE share_code = p_share_code;

    -- Cursor for Shared Consent and Organisation Details
    OPEN consent_cursor FOR 
        SELECT 
            s.""guid"", s.submitted_at, s.submission_state, s.share_code, 
            f.""name"" AS form_name, f.""version"", f.is_required, 
            o.""guid"" AS organisation_guid, o.""name"" AS organisation_name, o.""type"", o.roles,
            s0.supplier_type, s0.operation_types, s0.completed_reg_address, s0.completed_postal_address, 
            s0.completed_vat, s0.completed_website_address, s0.completed_email_address, s0.completed_operation_type, 
            s0.completed_legal_form, s0.completed_connected_person,
			l.registered_under_act2006, l.registered_legal_form, l.law_registered, l.registration_date
        FROM 
            shared_consents AS s
            INNER JOIN forms AS f ON s.form_id = f.id
            INNER JOIN organisations AS o ON s.organisation_id = o.id
            LEFT JOIN supplier_information AS s0 ON o.id = s0.id
            LEFT JOIN legal_forms AS l ON s0.id = l.id
        WHERE 
            s.share_code = p_share_code;

    -- Cursor for Identifiers
    OPEN identifier_cursor FOR 
        SELECT identifier_id, scheme, legal_name, uri, ""primary"" FROM identifiers WHERE organisation_id = v_organisation_id;

    -- Cursor for Organisation Addresses
    OPEN address_cursor FOR 
        SELECT 
            oa.""type"", a.street_address, a.locality, 
            a.region, a.postal_code, a.country_name, a.country
        FROM 
            organisation_address AS oa 
            LEFT JOIN addresses AS a ON a.id = oa.address_id 
        WHERE 
            oa.organisation_id = v_organisation_id;

    -- Cursor for Contact Points
    OPEN contact_cursor FOR 
        SELECT email, name, telephone, url FROM contact_points WHERE organisation_id = v_organisation_id;

    -- Cursor for Connected Entities
    OPEN connected_entities_cursor FOR 
        SELECT
        	ce.id, ce.""guid"", ce.entity_type, ce.has_compnay_house_number, ce.company_house_number, 
        	ce.overseas_company_number, ce.registered_date, ce.register_name, ce.""start_date"", ce.end_date,
        	cit.category AS individual_and_trust_category, cit.first_name, cit.last_name, cit.date_of_birth, 
        	cit.nationality, cit.control_condition AS individual_and_trust_control_condition, 
        	cit.connected_type AS connected_person_type, cit.resident_country,
        	co.category AS organisation_category, co.name AS organisation_name, co.insolvency_date,
        	co.registered_legal_form, co.law_registered, co.control_condition AS organisation_control_condition
    	FROM 
            connected_entities AS ce
            LEFT JOIN connected_individual_trust cit ON cit.connected_individual_trust_id = ce.id
            LEFT JOIN connected_organisation AS co ON co.connected_organisation_id = ce.id 
        WHERE 
            ce.supplier_organisation_id = v_organisation_id
            AND ce.created_on < v_submitted_at
            AND (ce.end_date IS NULL OR ce.end_date > v_submitted_at); 

    -- Cursor for Connected Entities Address
    OPEN connected_entities_address_cursor FOR 
        SELECT
        	ce.id, cea.""type"", a.street_address, a.locality, 
            a.region, a.postal_code, a.country_name, a.country
    	FROM 
            connected_entities AS ce
            INNER JOIN connected_entity_address cea ON cea.connected_entity_id = ce.id
            INNER JOIN addresses AS a ON a.id = cea.address_id
        WHERE 
            ce.supplier_organisation_id = v_organisation_id
            AND ce.created_on < v_submitted_at
            AND (ce.end_date IS NULL OR ce.end_date > v_submitted_at); 

    -- Cursor for Form Answer Sets
    OPEN form_answer_sets_cursor FOR 
        SELECT 
       		fas.id AS form_answer_set_id, fas.""guid"" AS form_answer_set_guid, fs.id, fs.title, fs.""type""
       	FROM 
       		form_answer_sets AS fas
            INNER JOIN form_sections AS fs ON fs.id = fas.section_id
       	WHERE 
       		fas.shared_consent_id = v_shared_consent_id and NOT fas.deleted AND fs.""type"" <> 1;

    -- Cursor for Form Question
    OPEN form_question_cursor FOR 
        SELECT DISTINCT ON (fq.id)
			fq.id, fq.""guid"", fq.sort_order, fq.""type"", fq.is_required, fq.""name"", 
			fq.title, fq.summary_title, fq.""description"", fq.""options"", fq.section_id
        FROM 
            form_answer_sets AS fas
            INNER JOIN form_sections AS fs ON fs.id = fas.section_id
            INNER JOIN form_questions AS fq ON fq.section_id = fs.id
        WHERE  
            fas.shared_consent_id = v_shared_consent_id and NOT fas.deleted AND fs.""type"" <> 1;

    -- Cursor for Form Answer
    OPEN form_answer_cursor FOR 
        SELECT 
			fa.question_id, fa.form_answer_set_id, fa.bool_value, fa.numeric_value, fa.date_value, 
			fa.start_value, fa.end_value, fa.text_value, fa.option_value, fa.""json_value"", fa.address_value
        FROM 
            form_answer_sets AS fas
            INNER JOIN form_sections AS fs ON fs.id = fas.section_id
            INNER JOIN form_answers AS fa ON fa.form_answer_set_id = fas.id
        WHERE  
            fas.shared_consent_id = v_shared_consent_id and NOT fas.deleted AND fs.""type"" <> 1;
END;
$$;
             ");
        }
    }
}
