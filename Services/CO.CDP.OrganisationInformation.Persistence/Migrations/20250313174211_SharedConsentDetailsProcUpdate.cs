using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SharedConsentDetailsProcUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
BEGIN
    SELECT id, organisation_id 
    INTO v_shared_consent_id, v_organisation_id
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
            ce.supplier_organisation_id = v_organisation_id;

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
            ce.supplier_organisation_id = v_organisation_id;

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
