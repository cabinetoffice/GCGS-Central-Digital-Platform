using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.Localization;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Text.Json;
using Address = CO.CDP.OrganisationInformation.Address;
using ConnectedEntityIndividualAndTrustCategoryType =
    CO.CDP.OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.AutoMapper;

public class DataSharingProfile : Profile
{
    public DataSharingProfile()
    {
        CreateMap<Persistence.SharedConsent, ShareReceipt>()
            .ForMember(m => m.FormId, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.FormVersionId, o => o.MapFrom(m => m.FormVersionId));

        CreateMap<Persistence.SharedConsent, SharedConsent>()
            .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt));

        CreateMap<Persistence.SharedConsentQuestionAnswer, SharedConsentQuestionAnswer>()
            .ForMember(m => m.QuestionId, o => o.MapFrom(m => m.QuestionId))
            .ForMember(m => m.Title, o => o.MapFrom<LocalizedPropertyResolver<Persistence.SharedConsentQuestionAnswer, SharedConsentQuestionAnswer>, string>(m => m.Title))
            .ForMember(m => m.Answer, o => o.MapFrom<CustomResolver>());

        CreateMap<Persistence.SharedConsentDetails, SharedConsentDetails>()
            .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt))
            .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.ShareCode));

        CreateMap<SharedConsentNonEf, SupplierInformation>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Organisation.Guid))
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Organisation.Name))
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Organisation.Type))
            .ForMember(m => m.Identifier, o => o.MapFrom(m => m.Organisation.Identifiers.FirstOrDefault(x => x.Primary)))
            .ForMember(m => m.AdditionalIdentifiers, o => o.MapFrom(m => m.Organisation.Identifiers.Where(x => !x.Primary).ToList()))
            .ForMember(m => m.Address, o => o.MapFrom(m => m.Organisation.Addresses.FirstOrDefault(x => x.Type == AddressType.Registered)))
            .ForMember(m => m.AdditionalAddresses, o => o.MapFrom(m => m.Organisation.Addresses.Where(x => x.Type == AddressType.Postal)))
            .ForMember(m => m.ContactPoint, o => o.MapFrom(m => m.Organisation.ContactPoints.FirstOrDefault()))
            .ForMember(m => m.Roles, o => o.MapFrom(m => m.Organisation.Roles))
            .ForMember(m => m.Details, o => o.MapFrom<DetailsValueResolver>())
            .ForMember(m => m.SupplierInformationData, o => o.MapFrom(m => m))
            .ForMember(m => m.AssociatedPersons, o => o.MapFrom(m => m.Organisation.AssociatedPersons))
            .ForMember(m => m.AdditionalEntities, o => o.MapFrom(m => m.Organisation.AdditionalEntities))
            .ForMember(m => m.AdditionalParties, o => o.Ignore());

        CreateMap<ConnectedEntityNonEf, AssociatedPerson>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Addresses, o => o.MapFrom(m => m.Addresses))
            .ForMember(m => m.RegistrationAuthority, o => o.MapFrom(m => m.RegisterName))
            .ForMember(m => m.RegisteredDate, o => o.MapFrom(m => m.RegisteredDate))
            .ForMember(m => m.HasCompanyHouseNumber, o => o.MapFrom(m => m.HasCompanyHouseNumber))
            .ForMember(m => m.CompanyHouseNumber, o => o.MapFrom(m => m.CompanyHouseNumber))
            .ForMember(m => m.OverseasCompanyNumber, o => o.MapFrom(m => m.OverseasCompanyNumber))
            .ForMember(m => m.Period, o => o.MapFrom(m => new AssociatedPeriod { EndDate = m.EndDate }))
            .ForMember(m => m.EntityType, o => o.MapFrom(m => m.IndividualOrTrust!.ConnectedType))
            .ForMember(m => m.Relationship, o => o.MapFrom(m => ToAssociatedRelationship(m.IndividualOrTrust!.Category)))
            .ForMember(m => m.FirstName, o => o.MapFrom(m => m.IndividualOrTrust!.FirstName))
            .ForMember(m => m.LastName, o => o.MapFrom(m => m.IndividualOrTrust!.LastName))
            .ForMember(m => m.DateOfBirth, o => o.MapFrom(m => m.IndividualOrTrust!.DateOfBirth))
            .ForMember(m => m.Nationality, o => o.MapFrom(m => m.IndividualOrTrust!.Nationality))
            .ForMember(m => m.ResidentCountry, o => o.MapFrom(m => m.IndividualOrTrust!.ResidentCountry))
            .ForMember(m => m.ControlCondition, o => o.MapFrom(m => m.IndividualOrTrust!.ControlCondition));

        CreateMap<ConnectedEntityNonEf, AssociatedEntity>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Addresses, o => o.MapFrom(m => m.Addresses))
            .ForMember(m => m.RegistrationAuthority, o => o.MapFrom(m => m.RegisterName))
            .ForMember(m => m.RegisteredDate, o => o.MapFrom(m => m.RegisteredDate))
            .ForMember(m => m.HasCompanyHouseNumber, o => o.MapFrom(m => m.HasCompanyHouseNumber))
            .ForMember(m => m.CompanyHouseNumber, o => o.MapFrom(m => m.CompanyHouseNumber))
            .ForMember(m => m.OverseasCompanyNumber, o => o.MapFrom(m => m.OverseasCompanyNumber))
            .ForMember(m => m.Period, o => o.MapFrom(m => new AssociatedPeriod { EndDate = m.EndDate }))
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Organisation!.Name))
            .ForMember(m => m.Category, o => o.MapFrom(m => m.Organisation!.Category))
            .ForMember(m => m.InsolvencyDate, o => o.MapFrom(m => m.Organisation!.InsolvencyDate))
            .ForMember(m => m.RegisteredLegalForm, o => o.MapFrom(m => m.Organisation!.RegisteredLegalForm))
            .ForMember(m => m.LawRegistered, o => o.MapFrom(m => m.Organisation!.LawRegistered))
            .ForMember(m => m.ControlCondition, o => o.MapFrom(m => m.Organisation!.ControlCondition));

        Uri? tempResult;
        CreateMap<IdentifierNonEf, OrganisationInformation.Identifier>()
            .ForMember(m => m.Scheme, o => o.MapFrom(m => m.Scheme))
            .ForMember(m => m.Id, o => o.MapFrom(m => m.IdentifierId))
            .ForMember(m => m.LegalName, o => o.MapFrom(m => m.LegalName))
            .ForMember(m => m.Uri, o => o.MapFrom(m => Uri.TryCreate(m.Uri, UriKind.Absolute, out tempResult) ? tempResult : null));

        CreateMap<AddressNonEf, Address>()
            .ForMember(m => m.StreetAddress, o => o.MapFrom(m => m.StreetAddress))
            .ForMember(m => m.Locality, o => o.MapFrom(m => m.Locality))
            .ForMember(m => m.Region, o => o.MapFrom(m => m.Region))
            .ForMember(m => m.PostalCode, o => o.MapFrom(m => m.PostalCode))
            .ForMember(m => m.CountryName, o => o.MapFrom(m => m.CountryName))
            .ForMember(m => m.Country, o => o.MapFrom(m => m.Country))
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type));

        CreateMap<ContactPointNonEf, ContactPoint>()
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Name))
            .ForMember(m => m.Email, o => o.MapFrom(m => m.Email))
            .ForMember(m => m.Telephone, o => o.MapFrom(m => m.Telephone))
            .ForMember(m => m.Url, o => o.MapFrom(m => !string.IsNullOrWhiteSpace(m.Url) ? new Uri(m.Url) : default));

        CreateMap<SharedConsentNonEf, SupplierInformationData>()
            .ForMember(m => m.Form, o => o.MapFrom(m => m))
            .ForMember(m => m.AnswerSets, o => o.MapFrom(m => m.AnswerSets))
            .ForMember(m => m.Questions,
                o => o.MapFrom(m => m.AnswerSets.SelectMany(a => a.Section.Questions.Where(x => x.Type != Persistence.FormQuestionType.NoInput && x.Type != Persistence.FormQuestionType.CheckYourAnswers)).DistinctBy(q => q.Id)));

        CreateMap<SharedConsentNonEf, Form>()
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Form.Name))
            .ForMember(m => m.SubmissionState, o => o.MapFrom(m => m.SubmissionState.ToString()))
            .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt))
            .ForMember(m => m.OrganisationId, o => o.MapFrom(m => m.Organisation.Guid))
            .ForMember(m => m.FormId, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.FormVersionId, o => o.MapFrom(m => m.Form.Version))
            .ForMember(m => m.IsRequired, o => o.MapFrom(m => m.Form.IsRequired))
            .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.ShareCode));

        CreateMap<FormAnswerSetNonEf, FormAnswerSet>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.SectionName, o => o.MapFrom<LocalizedPropertyResolver<FormAnswerSetNonEf, FormAnswerSet>, string>(m => m.Section.Title))
            .ForMember(m => m.Answers, o => o.MapFrom(m => m.Answers.Where(x => x.Question.Type != Persistence.FormQuestionType.NoInput && x.Question.Type != Persistence.FormQuestionType.CheckYourAnswers)))
            .ForMember(m => m.OrganisationId, o => o.MapFrom((_, _, _, ctx) => ((SharedConsentNonEf)ctx.Items["RootSource"]).Organisation.Guid));

        CreateMap<FormAnswerNonEf, FormAnswer>()
            .ForMember(m => m.QuestionName, o => o.MapFrom(m => m.Question.Name))
            .ForMember(m => m.BoolValue, o => o.MapFrom(m => m.BoolValue))
            .ForMember(m => m.NumericValue, o => o.MapFrom(m => m.NumericValue))
            .ForMember(m => m.StartValue, o => o.MapFrom(m => m.StartValue))
            .ForMember(m => m.EndValue, o => o.MapFrom(m => m.EndValue))
            .ForMember(m => m.DateValue, o => o.MapFrom(m => ToDateOnly(m.DateValue)))
            .ForMember(m => m.TextValue, o => o.MapFrom(m => m.Question.Type == Persistence.FormQuestionType.FileUpload ? null : m.TextValue))
            .ForMember(m => m.OptionValue, o => o.MapFrom(m => m.OptionValue != null ? new string[] { m.OptionValue } : null))
            .ForMember(m => m.JsonValue, o => o.MapFrom<JsonValueResolver>())
            .ForMember(m => m.DocumentUri, o => o.MapFrom<DocumentUriValueResolver>());

        CreateMap<FormQuestionNonEf, FormQuestion>()
            .ForMember(m => m.Type, o => o.MapFrom<CustomFormQuestionTypeResolver>())
            .ForMember(m => m.Title, o => o.MapFrom<LocalizedPropertyResolver<FormQuestionNonEf, FormQuestion>, string>(m => m.Title))
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Name))
            .ForMember(m => m.Text, o => o.MapFrom<LocalizedPropertyResolver<FormQuestionNonEf, FormQuestion>, string>(m => m.Description ?? string.Empty))
            .ForMember(m => m.IsRequired, o => o.MapFrom(m => m.IsRequired))
            .ForMember(m => m.SectionName, o => o.MapFrom<LocalizedPropertyResolver<FormQuestionNonEf, FormQuestion>, string>(m => m.Section.Title))
            .ForMember(m => m.Options, o => o.MapFrom<FormQuestionOptionsResolver>())
            .ForMember(m => m.SortOrder, o => o.MapFrom(m => m.SortOrder))
            .ForMember(m => m.OrganisationId, o => o.MapFrom((_, _, _, ctx) => ((SharedConsentNonEf)ctx.Items["RootSource"]).Organisation.Guid));

        CreateMap<Persistence.FormQuestionChoice, FormQuestionOption>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Id))
            .ForMember(m => m.Value, o => o.MapFrom<LocalizedPropertyResolver<Persistence.FormQuestionChoice, FormQuestionOption>, string>(m => m.Title));

        CreateMap<LegalFormNonEf, LegalForm>()
            .ForMember(m => m.LawRegistered, o => o.MapFrom(m => m.LawRegistered))
            .ForMember(m => m.RegistrationDate, o => o.MapFrom(m => m.RegistrationDate.ToString("yyyy-MM-dd")))
            .ForMember(m => m.RegisteredLegalForm, o => o.MapFrom(m => m.RegisteredLegalForm))
            .ForMember(m => m.RegisteredUnderAct2006, o => o.MapFrom(m => m.RegisteredUnderAct2006));
    }

    private static AssociatedRelationship ToAssociatedRelationship(ConnectedEntityIndividualAndTrustCategoryType relationship)
    {
        return relationship switch
        {
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv
                => AssociatedRelationship.PersonWithSignificantControlForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForIndiv
                => AssociatedRelationship.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForIndiv
                => AssociatedRelationship.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust
                => AssociatedRelationship.PersonWithSignificantControlForTrust,
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForTrust
                => AssociatedRelationship.DirectorOrIndividualWithTheSameResponsibilitiesForTrust,
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForTrust
                => AssociatedRelationship.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust,
            _
                => throw new Exception($"{nameof(ConnectedEntityIndividualAndTrustCategoryType)} enum to {nameof(AssociatedRelationship)} enum does not exists."),
        };
    }

    private DateOnly? ToDateOnly(DateTime? dateTime) => dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
}
public class DetailsValueResolver : IValueResolver<SharedConsentNonEf, SupplierInformation, Details>
{
    public Details Resolve(SharedConsentNonEf source, SupplierInformation destination,
        Details destMember, ResolutionContext context)
    {
        var legalForm = source.Organisation.SupplierInfo?.LegalForm;
        var operationTypes = source.Organisation.SupplierInfo?.OperationTypes;

        return new Details
        {
            LegalForm = legalForm != null ? context.Mapper.Map<LegalForm>(legalForm) : null,
            Scale = (operationTypes != null && operationTypes.Contains(OperationType.SmallOrMediumSized)) ? "small"
                : ((operationTypes == null || operationTypes.Count == 0) ? null : "large"),
            Vcse = (operationTypes != null && operationTypes.Count > 0) ? operationTypes.Contains(OperationType.NonGovernmental) : (bool?)null,
            ShelteredWorkshop = (operationTypes != null && operationTypes.Count > 0) ? operationTypes.Contains(OperationType.SupportedEmploymentProvider) : (bool?)null,
            PublicServiceMissionOrganization = (operationTypes != null && operationTypes.Count > 0) ? operationTypes.Contains(OperationType.PublicService) : (bool?)null
        };
    }
}

public class CustomFormQuestionTypeResolver : IValueResolver<FormQuestionNonEf, FormQuestion, FormQuestionType>
{
    public FormQuestionType Resolve(FormQuestionNonEf source,
        FormQuestion destination, FormQuestionType destMemb, ResolutionContext context)
    {
        switch (source.Type)
        {
            case Persistence.FormQuestionType.YesOrNo:
            case Persistence.FormQuestionType.CheckBox:
                return FormQuestionType.Boolean;

            case Persistence.FormQuestionType.Text:
            case Persistence.FormQuestionType.Address:
            case Persistence.FormQuestionType.MultiLine:
                return FormQuestionType.Text;

            case Persistence.FormQuestionType.SingleChoice:
            case Persistence.FormQuestionType.GroupedSingleChoice:
            case Persistence.FormQuestionType.MultipleChoice:
                if (source.Options.AnswerFieldName == "JsonValue")
                {
                    return FormQuestionType.OptionJson;
                }
                else
                {
                    return FormQuestionType.Option;
                }

            case Persistence.FormQuestionType.Date:
                return FormQuestionType.Date;

            case Persistence.FormQuestionType.Url:
                return FormQuestionType.Url;

            case Persistence.FormQuestionType.FileUpload:
                return FormQuestionType.FileUpload;

            case Persistence.FormQuestionType.NoInput:
            case Persistence.FormQuestionType.CheckYourAnswers:
                return FormQuestionType.None;

            default:
                throw new InvalidOperationException($"Unhandled FormQuestionType: {source.Type}");
        }
    }
}

public class CustomResolver : IValueResolver<Persistence.SharedConsentQuestionAnswer, SharedConsentQuestionAnswer, string?>
{
    public string? Resolve(Persistence.SharedConsentQuestionAnswer source,
        SharedConsentQuestionAnswer destination, string? destMemb, ResolutionContext context)
    {
        switch (source.QuestionType)
        {
            case Persistence.FormQuestionType.Text:
            case Persistence.FormQuestionType.FileUpload:
                return source.Answer.TextValue;
            case Persistence.FormQuestionType.YesOrNo:
            case Persistence.FormQuestionType.CheckBox:
                return source.Answer.BoolValue.ToString();
            case Persistence.FormQuestionType.Date:
                return source.Answer.DateValue.ToString();
            case Persistence.FormQuestionType.Address:
                return source.Answer.AddressValue != null ? ToHtmlString(source.Answer.AddressValue) : "";
            default:
                return "";
        }
    }

    private string ToHtmlString(Persistence.FormAddress source)
    {
        return string.Join(", ", new[]
            {
                source.StreetAddress,
                source.Locality,
                source.PostalCode,
                source.Region,
                source.CountryName
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}

public class JsonValueResolver : IValueResolver<FormAnswerNonEf, FormAnswer, Dictionary<string, object>?>
{
    public Dictionary<string, object>? Resolve(FormAnswerNonEf source, FormAnswer destination, Dictionary<string, object>? destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.JsonValue))
        {
            return null;
        }

        var values = JsonSerializer.Deserialize<Dictionary<string, object>>(source.JsonValue);

        if (values != null && values.TryGetValue("type", out object? type)
            && source.Question?.Options?.ChoiceProviderStrategy == "ExclusionAppliesToChoiceProviderStrategy")
        {
            var newType = type;
            switch (type.ToString())
            {
                case "organisation":
                    newType = "self";
                    break;

                case "connected-entity":
                    var entityId = values["id"].ToString();

                    if (context.Items.TryGetValue("RootSource", out var rootObj) && rootObj is SharedConsentNonEf sharedConsent)
                    {
                        if (sharedConsent.Organisation.AssociatedPersons.Any(a => a.Guid.ToString() == entityId) == true)
                        {
                            newType = "associated-persons";
                        }
                        else
                        {
                            if (sharedConsent.Organisation.AdditionalEntities.Any(a => a.Guid.ToString() == entityId) == true)
                            {
                                newType = "additional-entities";
                            }
                        }
                    }
                    break;
            }

            values["type"] = newType;
        }

        return values;
    }
}

public class DocumentUriValueResolver(IConfiguration configuration) : IValueResolver<FormAnswerNonEf, FormAnswer, Uri?>
{
    public Uri? Resolve(FormAnswerNonEf source, FormAnswer destination, Uri? destMember, ResolutionContext context)
    {
        if (context.Items.TryGetValue("RootSource", out var rootObj) && rootObj is SharedConsentNonEf sharedConsent)
        {
            var dataSharingApiUrl = configuration["DataSharingApiUrl"]
                    ?? throw new Exception("Missing configuration key: DataSharingApiUrl.");

            if (source.Question.Type == Persistence.FormQuestionType.FileUpload)
            {
                return new Uri(new Uri(dataSharingApiUrl), $"/share/data/{sharedConsent.ShareCode}/document/{source.TextValue}");
            }
        }

        return null;
    }
}

public class FormQuestionOptionsResolver(IHtmlLocalizer<FormsEngineResource> localizer)
    : IValueResolver<FormQuestionNonEf, FormQuestion, List<FormQuestionOption>>
{
    public List<FormQuestionOption> Resolve(FormQuestionNonEf src, FormQuestion destination,
        List<FormQuestionOption> destMember, ResolutionContext context)
    {
        if (src.Options == null)
            return new List<FormQuestionOption>();

        if (src.Type == Persistence.FormQuestionType.GroupedSingleChoice && src.Options.Groups != null)
        {
            return src.Options.Groups
                .SelectMany(g => g.Choices ?? new List<Persistence.FormQuestionGroupChoice>())
                .Select(gc => new FormQuestionOption
                {
                    Id = gc.Id,
                    Value = localizer[gc.Title].Value
                })
                .ToList();
        }

        if (!string.IsNullOrEmpty(src.Options.ChoiceProviderStrategy))
        {
            return new List<FormQuestionOption>
            {
                new FormQuestionOption { Id = Guid.NewGuid(), Value = "Dynamic" }
            };
        }

        if (src.Options.Choices != null)
        {
            return src.Options.Choices
                .Select(c => new FormQuestionOption
                {
                    Id = c.Id,
                    Value = localizer[c.Title].Value
                })
                .ToList();
        }

        return new List<FormQuestionOption>();
    }
}
