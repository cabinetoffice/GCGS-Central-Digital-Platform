using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_Connected_Entities_Deleted_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "deleted",
                table: "connected_entities",
                type: "boolean",
                defaultValue: false,
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "deleted",
                table: "connected_entities_snapshot",
                type: "boolean",
                defaultValue: false,
                nullable: false);

            migrationBuilder.Sql($@"
CREATE OR REPLACE PROCEDURE public.create_shared_consent_snapshot(
	IN p_share_code text)
LANGUAGE 'plpgsql'
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
			registered_date, register_name, start_date, end_date, created_on, updated_on, mapping_id, deleted)
	    SELECT 
	        v_shared_consent_id, ""guid"", entity_type, has_compnay_house_number, company_house_number, overseas_company_number, 
			registered_date, register_name, start_date, end_date, created_on, updated_on, id, deleted
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
CREATE OR REPLACE PROCEDURE public.get_shared_consent_details(
	IN p_share_code text,
	INOUT consent_cursor refcursor,
	INOUT identifier_cursor refcursor,
	INOUT address_cursor refcursor,
	INOUT contact_cursor refcursor,
	INOUT connected_entities_cursor refcursor,
	INOUT connected_entities_address_cursor refcursor,
	INOUT form_answer_sets_cursor refcursor,
	INOUT form_question_cursor refcursor,
	INOUT form_answer_cursor refcursor)
LANGUAGE 'plpgsql'
AS $$

DECLARE v_shared_consent_id INT;
BEGIN
    SELECT id, submitted_at 
    INTO v_shared_consent_id
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
            ce.shared_consent_id = v_shared_consent_id and ce.deleted = false;

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
            ce.shared_consent_id = v_shared_consent_id and ce.deleted = false;

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted",
                table: "connected_entities");

            migrationBuilder.DropColumn(
                name: "deleted",
                table: "connected_entities_snapshot");

            migrationBuilder.Sql($@"
CREATE OR REPLACE PROCEDURE public.create_shared_consent_snapshot(
	IN p_share_code text)
LANGUAGE 'plpgsql'
AS $BODY$

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
CREATE OR REPLACE PROCEDURE public.get_shared_consent_details(
	IN p_share_code text,
	INOUT consent_cursor refcursor,
	INOUT identifier_cursor refcursor,
	INOUT address_cursor refcursor,
	INOUT contact_cursor refcursor,
	INOUT connected_entities_cursor refcursor,
	INOUT connected_entities_address_cursor refcursor,
	INOUT form_answer_sets_cursor refcursor,
	INOUT form_question_cursor refcursor,
	INOUT form_answer_cursor refcursor)
LANGUAGE 'plpgsql'
AS $BODY$

DECLARE v_shared_consent_id INT;
BEGIN
    SELECT id, submitted_at 
    INTO v_shared_consent_id
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
            ce.shared_consent_id = v_shared_consent_id;

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
            ce.shared_consent_id = v_shared_consent_id;

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
