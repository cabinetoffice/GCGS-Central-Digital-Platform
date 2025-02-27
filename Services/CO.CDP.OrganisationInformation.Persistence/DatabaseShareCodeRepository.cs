using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseShareCodeRepository(OrganisationInformationContext context) : IShareCodeRepository
{
    public async Task<bool> ShareCodeDocumentExistsAsync(string shareCode, string documentId)
    {
        return await context.Set<FormAnswer>()
            .Where(x => x.Question.Type == FormQuestionType.FileUpload
                && x.TextValue == documentId
                && x.FormAnswerSet!.Deleted == false
                && x.FormAnswerSet!.SharedConsent.ShareCode == shareCode)
            .AnyAsync();
    }

    public async Task<IEnumerable<SharedConsent>> GetShareCodesAsync(Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Where(x => x.SubmissionState == SubmissionState.Submitted && x.Organisation.Guid == organisationId)
            .OrderByDescending(y => y.SubmittedAt).ToListAsync();
    }

    public async Task<SharedConsent?> GetSharedConsentDraftAsync(Guid formId, Guid organisationId)
    {
        return await context.Set<SharedConsent>()
            .Where(x => x.SubmissionState == SubmissionState.Draft)
            .FirstOrDefaultAsync(s => s.Form.Guid == formId && s.Organisation.Guid == organisationId);
    }

    public async Task<SharedConsentNonEf?> GetByShareCode(string shareCode)
    {
        SharedConsentNonEf? sharedConsent = null;
        List<FormSectionNonEf> formSections = [];
        List<FormQuestionNonEf> formQuestions = [];

        NpgsqlConnection? conn = null;
        NpgsqlTransaction? tran = null;
        try
        {
            conn = (NpgsqlConnection)context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            tran = await conn.BeginTransactionAsync();

            using var command = new NpgsqlCommand("get_shared_consent_details", conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new NpgsqlParameter("p_share_code", shareCode));
            command.Parameters.Add(new NpgsqlParameter("consent_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "consent_cursor" });
            command.Parameters.Add(new NpgsqlParameter("identifier_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "identifier_cursor" });
            command.Parameters.Add(new NpgsqlParameter("address_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "address_cursor" });
            command.Parameters.Add(new NpgsqlParameter("contact_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "contact_cursor" });
            command.Parameters.Add(new NpgsqlParameter("connected_entities_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "connected_entities_cursor" });
            command.Parameters.Add(new NpgsqlParameter("connected_entities_address_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "connected_entities_address_cursor" });
            command.Parameters.Add(new NpgsqlParameter("form_answer_sets_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "form_answer_sets_cursor" });
            command.Parameters.Add(new NpgsqlParameter("form_question_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "form_question_cursor" });
            command.Parameters.Add(new NpgsqlParameter("form_answer_cursor", NpgsqlDbType.Refcursor) { Direction = ParameterDirection.InputOutput, Value = "form_answer_cursor" });

            await command.ExecuteNonQueryAsync(); // Declare the cursor

            using (var consentCmd = new NpgsqlCommand("FETCH ALL IN consent_cursor", conn))
            {
                using var consentReader = await consentCmd.ExecuteReaderAsync();

                while (await consentReader.ReadAsync())
                {
                    sharedConsent = new SharedConsentNonEf
                    {
                        Guid = consentReader.GetGuid("guid"),
                        SubmittedAt = consentReader.GetFieldValue<DateTimeOffset>("submitted_at"),
                        SubmissionState = (SubmissionState)consentReader.GetInt32("submission_state"),
                        ShareCode = consentReader.GetNullableString("share_code"),
                        Form = new FormNonEf
                        {
                            Name = consentReader.GetString("form_name"),
                            Version = consentReader.GetString("version"),
                            IsRequired = consentReader.GetBoolean("is_required"),
                        },
                        Organisation = new OrganisationNonEf
                        {
                            Guid = consentReader.GetGuid("organisation_guid"),
                            Name = consentReader.GetString("organisation_name"),
                            Type = (OrganisationType)consentReader.GetInt32("type"),
                            Roles = consentReader.GetFieldValue<int[]>("roles").Select(r => (PartyRole)r).ToList()
                        }
                    };

                    var supplierType = consentReader.GetFieldValue<int?>("supplier_type");
                    if (supplierType.HasValue)
                    {
                        sharedConsent.Organisation.SupplierInfo = new SupplierInformationNonEf
                        {
                            SupplierType = (SupplierType)supplierType,
                            OperationTypes = consentReader.GetFieldValue<int[]>("operation_types").Select(r => (OperationType)r).ToList(),
                            CompletedRegAddress = consentReader.GetBoolean("completed_reg_address"),
                            CompletedPostalAddress = consentReader.GetBoolean("completed_postal_address"),
                            CompletedVat = consentReader.GetBoolean("completed_vat"),
                            CompletedWebsiteAddress = consentReader.GetBoolean("completed_website_address"),
                            CompletedEmailAddress = consentReader.GetBoolean("completed_email_address"),
                            CompletedOperationType = consentReader.GetBoolean("completed_operation_type"),
                            CompletedLegalForm = consentReader.GetBoolean("completed_legal_form"),
                            CompletedConnectedPerson = consentReader.GetBoolean("completed_connected_person")
                        };

                        var registeredUnderAct2006 = consentReader.GetFieldValue<bool?>("registered_under_act2006");
                        if (registeredUnderAct2006.HasValue)
                        {
                            sharedConsent.Organisation.SupplierInfo.LegalForm = new LegalFormNonEf
                            {
                                RegisteredUnderAct2006 = registeredUnderAct2006.Value,
                                RegisteredLegalForm = consentReader.GetString("registered_legal_form"),
                                LawRegistered = consentReader.GetString("law_registered"),
                                RegistrationDate = consentReader.GetFieldValue<DateTimeOffset>("registration_date")
                            };
                        }
                    }
                }
            }

            if (sharedConsent == null) return null;

            using (var identifierCmd = new NpgsqlCommand("FETCH ALL IN identifier_cursor", conn))
            {
                using var identifierReader = await identifierCmd.ExecuteReaderAsync();

                while (await identifierReader.ReadAsync())
                {
                    sharedConsent.Organisation.Identifiers.Add(
                        new IdentifierNonEf
                        {
                            IdentifierId = identifierReader.GetNullableString("identifier_id"),
                            Scheme = identifierReader.GetString("scheme"),
                            LegalName = identifierReader.GetString("legal_name"),
                            Uri = identifierReader.GetNullableString("uri"),
                            Primary = identifierReader.GetBoolean("primary")
                        });
                }
            }

            using (var addressCmd = new NpgsqlCommand("FETCH ALL IN address_cursor", conn))
            {
                using var addressReader = await addressCmd.ExecuteReaderAsync();

                while (await addressReader.ReadAsync())
                {
                    sharedConsent.Organisation.Addresses.Add(
                        new AddressNonEf
                        {
                            StreetAddress = addressReader.GetString("street_address"),
                            Locality = addressReader.GetString("locality"),
                            Region = addressReader.GetNullableString("region"),
                            PostalCode = addressReader.GetNullableString("postal_code"),
                            CountryName = addressReader.GetString("country_name"),
                            Country = addressReader.GetString("country"),
                            Type = (AddressType)addressReader.GetInt32("type")
                        });
                }
            }

            using (var contactCmd = new NpgsqlCommand("FETCH ALL IN contact_cursor", conn))
            {
                using var contactReader = await contactCmd.ExecuteReaderAsync();

                while (await contactReader.ReadAsync())
                {
                    sharedConsent.Organisation.ContactPoints.Add(
                        new ContactPointNonEf
                        {
                            Name = contactReader.GetNullableString("name"),
                            Email = contactReader.GetNullableString("email"),
                            Telephone = contactReader.GetNullableString("telephone"),
                            Url = contactReader.GetNullableString("url")
                        });
                }
            }

            using (var connectedEntitiesCmd = new NpgsqlCommand("FETCH ALL IN connected_entities_cursor", conn))
            {
                using var connectedEntitiesReader = await connectedEntitiesCmd.ExecuteReaderAsync();

                while (await connectedEntitiesReader.ReadAsync())
                {
                    var connectedEntity = new ConnectedEntityNonEf
                    {
                        Id = connectedEntitiesReader.GetInt32("id"),
                        Guid = connectedEntitiesReader.GetGuid("guid"),
                        EntityType = (ConnectedEntityType)connectedEntitiesReader.GetInt32("entity_type"),
                        HasCompnayHouseNumber = connectedEntitiesReader.GetBoolean("has_compnay_house_number"),
                        CompanyHouseNumber = connectedEntitiesReader.GetNullableString("company_house_number"),
                        OverseasCompanyNumber = connectedEntitiesReader.GetNullableString("overseas_company_number"),
                        RegisteredDate = connectedEntitiesReader.GetFieldValue<DateTimeOffset?>("registered_date"),
                        RegisterName = connectedEntitiesReader.GetNullableString("register_name"),
                        StartDate = connectedEntitiesReader.GetFieldValue<DateTimeOffset?>("start_date"),
                        EndDate = connectedEntitiesReader.GetFieldValue<DateTimeOffset?>("end_date")
                    };

                    if (connectedEntity.EntityType == ConnectedEntityType.Organisation)
                    {
                        connectedEntity.Organisation = new ConnectedOrganisationNonEf
                        {
                            Category = (ConnectedOrganisationCategory)connectedEntitiesReader.GetInt32("organisation_category"),
                            Name = connectedEntitiesReader.GetString("organisation_name"),
                            InsolvencyDate = connectedEntitiesReader.GetFieldValue<DateTimeOffset?>("insolvency_date"),
                            RegisteredLegalForm = connectedEntitiesReader.GetNullableString("registered_legal_form"),
                            LawRegistered = connectedEntitiesReader.GetNullableString("law_registered"),
                            ControlCondition = connectedEntitiesReader.GetFieldValue<int[]>("organisation_control_condition").Select(r => (ControlCondition)r).ToList()
                        };
                    }
                    else
                    {
                        connectedEntity.IndividualOrTrust = new ConnectedIndividualTrustNonEf
                        {
                            Category = (ConnectedEntityIndividualAndTrustCategoryType)connectedEntitiesReader.GetInt32("individual_and_trust_category"),
                            FirstName = connectedEntitiesReader.GetString("first_name"),
                            LastName = connectedEntitiesReader.GetString("last_name"),
                            DateOfBirth = connectedEntitiesReader.GetFieldValue<DateTimeOffset?>("date_of_birth"),
                            Nationality = connectedEntitiesReader.GetNullableString("nationality"),
                            ControlCondition = connectedEntitiesReader.GetFieldValue<int[]>("individual_and_trust_control_condition").Select(r => (ControlCondition)r).ToList(),
                            ConnectedType = (ConnectedPersonType)connectedEntitiesReader.GetInt32("connected_person_type"),
                            ResidentCountry = connectedEntitiesReader.GetNullableString("resident_country")
                        };
                    }

                    sharedConsent.Organisation.ConnectedEntities.Add(connectedEntity);
                }
            }

            using (var connectedEntitiesAddressCmd = new NpgsqlCommand("FETCH ALL IN connected_entities_address_cursor", conn))
            {
                using var connectedEntitiesAddressReader = await connectedEntitiesAddressCmd.ExecuteReaderAsync();

                while (await connectedEntitiesAddressReader.ReadAsync())
                {
                    var connectedEntityId = connectedEntitiesAddressReader.GetInt32("id");

                    sharedConsent.Organisation.ConnectedEntities
                        .First(ce => ce.Id == connectedEntityId)
                        .Addresses.Add(new AddressNonEf
                        {
                            StreetAddress = connectedEntitiesAddressReader.GetString("street_address"),
                            Locality = connectedEntitiesAddressReader.GetString("locality"),
                            Region = connectedEntitiesAddressReader.GetNullableString("region"),
                            PostalCode = connectedEntitiesAddressReader.GetNullableString("postal_code"),
                            CountryName = connectedEntitiesAddressReader.GetString("country_name"),
                            Country = connectedEntitiesAddressReader.GetString("country"),
                            Type = (AddressType)connectedEntitiesAddressReader.GetInt32("type")
                        });
                }
            }

            using (var formAnswerSetCmd = new NpgsqlCommand("FETCH ALL IN form_answer_sets_cursor", conn))
            {
                using var formAnswerSetReader = await formAnswerSetCmd.ExecuteReaderAsync();

                while (await formAnswerSetReader.ReadAsync())
                {
                    var formSectionId = formAnswerSetReader.GetInt32("id");
                    var section = formSections.FirstOrDefault(fs => fs.Id == formSectionId);

                    if (section == null)
                    {
                        section = new FormSectionNonEf
                        {
                            Id = formSectionId,
                            Title = formAnswerSetReader.GetString("title"),
                            Type = (FormSectionType)formAnswerSetReader.GetInt32("type")
                        };
                        formSections.Add(section);
                    }

                    sharedConsent.AnswerSets.Add(
                        new FormAnswerSetNonEf
                        {
                            Id = formAnswerSetReader.GetInt32("form_answer_set_id"),
                            Guid = formAnswerSetReader.GetGuid("form_answer_set_guid"),
                            SectionId = formSectionId,
                            Section = section
                        });
                }
            }

            using (var formQuestionCmd = new NpgsqlCommand("FETCH ALL IN form_question_cursor", conn))
            {
                using var formQuestionReader = await formQuestionCmd.ExecuteReaderAsync();

                while (await formQuestionReader.ReadAsync())
                {
                    var section = formSections.First(fs => fs.Id == formQuestionReader.GetInt32("section_id"));
                    var question = new FormQuestionNonEf
                    {
                        Id = formQuestionReader.GetInt32("id"),
                        Guid = formQuestionReader.GetGuid("guid"),
                        SortOrder = formQuestionReader.GetInt32("sort_order"),
                        Type = (FormQuestionType)formQuestionReader.GetInt32("type"),
                        IsRequired = formQuestionReader.GetBoolean("is_required"),
                        Name = formQuestionReader.GetString("name"),
                        Title = formQuestionReader.GetString("title"),
                        SummaryTitle = formQuestionReader.GetNullableString("summary_title"),
                        Description = formQuestionReader.GetNullableString("description"),
                        Options = formQuestionReader.GetJsonObject<FormQuestionOptions>("options") ?? new(),
                        Section = section
                    };
                    section.Questions.Add(question);
                    formQuestions.Add(question);
                }
            }

            using (var formAnswerCmd = new NpgsqlCommand("FETCH ALL IN form_answer_cursor", conn))
            {
                using var formAnswerReader = await formAnswerCmd.ExecuteReaderAsync();

                while (await formAnswerReader.ReadAsync())
                {
                    var formAnswerSetId = formAnswerReader.GetInt32("form_answer_set_id");
                    var questionId = formAnswerReader.GetInt32("question_id");
                    var answerSet = sharedConsent.AnswerSets.First(a => a.Id == formAnswerSetId);

                    answerSet.Answers.Add(
                        new FormAnswerNonEf
                        {
                            FormAnswerSetId = formAnswerSetId,
                            FormAnswerSet = answerSet,
                            QuestionId = formAnswerReader.GetInt32("question_id"),
                            Question = formQuestions.First(fq => fq.Id == questionId),
                            BoolValue = formAnswerReader.GetFieldValue<bool?>("bool_value"),
                            NumericValue = formAnswerReader.GetFieldValue<double?>("numeric_value"),
                            DateValue = formAnswerReader.GetFieldValue<DateTime?>("date_value"),
                            StartValue = formAnswerReader.GetFieldValue<DateTime?>("start_value"),
                            EndValue = formAnswerReader.GetFieldValue<DateTime?>("end_value"),
                            TextValue = formAnswerReader.GetNullableString("text_value"),
                            OptionValue = formAnswerReader.GetNullableString("option_value"),
                            JsonValue = formAnswerReader.GetNullableString("json_value"),
                            AddressValue = formAnswerReader.GetJsonObject<AddressNonEf>("address_value"),
                        });
                }
            }


            formSections.ForEach(s => s.Questions = [.. s.Questions.OrderBy(q => q.SortOrder)]);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch
        {
            if (tran != null) await tran.RollbackAsync();
            throw;
        }
        finally
        {
            if (conn != null) await conn.DisposeAsync(); // This closes and disposes the connection safely
        }

        return sharedConsent;
    }

    public async Task<SharedConsentDetails?> GetShareCodeDetailsAsync(Guid organisationId, string shareCode)
    {
        var query = from s in context.SharedConsents
                    join fas in context.FormAnswerSets on s.Id equals fas.SharedConsentId
                    join fs in context.Set<FormSection>() on fas.SectionId equals fs.Id
                    join fa in context.Set<FormAnswer>() on fas.Id equals fa.FormAnswerSetId
                    join fq in context.Set<FormQuestion>() on fa.QuestionId equals fq.Id
                    join o in context.Organisations on s.OrganisationId equals o.Id
                    where
                        fs.Type == FormSectionType.Declaration
                        && fas.Deleted == false
                        && o.Guid == organisationId
                        && s.ShareCode == shareCode
                    select new
                    {
                        FormAnswerSetId = fas.Id,
                        FormAnswerSetUpdate = fas.UpdatedOn,
                        s.ShareCode,
                        s.SubmittedAt,
                        QuestionId = fq.Guid,
                        QuestionType = fq.Type,
                        fq.SummaryTitle,
                        fq.Title,
                        FormAnswer = fa
                    };

        var data = await query.ToListAsync();
        var sharedCodeResult = data.OrderByDescending(x => x.FormAnswerSetUpdate).GroupBy(g => new { g.ShareCode, g.FormAnswerSetId, g.SubmittedAt }).FirstOrDefault();
        if (sharedCodeResult == null) return null;

        return new SharedConsentDetails
        {
            ShareCode = sharedCodeResult.Key.ShareCode,
            SubmittedAt = sharedCodeResult.Key.SubmittedAt!.Value,
            QuestionAnswers = sharedCodeResult.Select(a =>
            new SharedConsentQuestionAnswer
            {
                QuestionId = a.QuestionId,
                QuestionType = a.QuestionType,
                Title = string.IsNullOrEmpty(a.SummaryTitle) ? a.Title : a.SummaryTitle,
                Answer = a.FormAnswer
            })
        };
    }

    public async Task<bool?> GetShareCodeVerifyAsync(string formVersionId, string shareCode)
    {
        // Get FormId and Organisation based on ShareCode and FormVersionId
        var query = from s in context.SharedConsents
                    where
                        s.FormVersionId == formVersionId
                        && s.ShareCode == shareCode
                    select s;

        if (query.Count() > 1) return false; // Scenario-1: 

        var data = await query.FirstOrDefaultAsync();
        if (data == null) return null; // Scenario-2: if Sharecode not found

        // Get the latest SharedConsent records of the Organistaion and FormVersionId and FormId
        var latestShareCode = await (from s in context.SharedConsents
                                     join fas in context.FormAnswerSets on s.Id equals fas.SharedConsentId
                                     where
                                         fas.Deleted == false
                                         && s.OrganisationId == data.OrganisationId
                                         && s.FormId == data.FormId
                                         && s.FormVersionId == data!.FormVersionId
                                     orderby s.UpdatedOn descending
                                     select s).Take(1).FirstOrDefaultAsync();

        if (latestShareCode!.SubmissionState != SubmissionState.Submitted) return false; // Scenario-3: Sharecode is not submitted

        if (data!.ShareCode == latestShareCode.ShareCode
            && data!.ShareCode == shareCode
                && data!.SubmissionState == SubmissionState.Submitted) return true; //Scenario-4: if requested sharecode is latest Sharecode and stae is submitted

        return false; //Scenario-4: if scenario-4 is not passed
    }

    public async Task<IEnumerable<string>> GetConsortiumOrganisationsShareCode(string shareCode)
    {
        return await context.SharedConsentConsortiums
                .Where(x => x.ParentSharedConsent!.ShareCode == shareCode)
                .Select(s => s.ChildSharedConsent!.ShareCode!)
                .ToListAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}