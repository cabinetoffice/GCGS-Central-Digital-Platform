using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using Address = CO.CDP.OrganisationInformation.Address;
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
            .ForMember(m => m.Title, o => o.MapFrom(m => m.Title))
            .ForMember(m => m.Answer, o => o.MapFrom<CustomResolver>());

        CreateMap<Persistence.SharedConsentDetails, SharedConsentDetails>()
            .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt))
            .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.ShareCode));

        CreateMap<Persistence.SharedConsent, SupplierInformation>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Organisation.Guid))
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Organisation.Name))
            .ForMember(m => m.Identifier,
                o => o.MapFrom(m => m.Organisation.Identifiers.FirstOrDefault(x => x.Primary)))
            .ForMember(m => m.AdditionalIdentifiers,
                o => o.MapFrom(m => m.Organisation.Identifiers.Where(x => !x.Primary).ToList()))
            .ForMember(m => m.Address,
                o => o.MapFrom(m =>
                    m.Organisation.Addresses.FirstOrDefault(x =>
                        x.Type == AddressType.Registered)))
            .ForMember(m => m.ContactPoint, o => o.MapFrom(m => m.Organisation.ContactPoints.FirstOrDefault()))
            .ForMember(m => m.Roles, o => o.MapFrom(m => m.Organisation.Roles))
            .ForMember(m => m.Details, o => o.MapFrom(m => new Details()))
            .ForMember(m => m.SupplierInformationData, o => o.MapFrom(m => m))
            .ForMember(m => m.AssociatedPersons,
                o => o.MapFrom((_, _, _, context) => context.Items["AssociatedPersons"]))
            .ForMember(m => m.AdditionalEntities,
                o => o.MapFrom((_, _, _, context) => context.Items["AdditionalEntities"]))
            .ForMember(m => m.AdditionalParties,
                o => o.MapFrom((_, _, _, context) => context.Items["AdditionalParties"]));

        CreateMap<Organisation.Identifier, Identifier>()
            .ForMember(m => m.Scheme, o => o.MapFrom(m => m.Scheme))
            .ForMember(m => m.Id, o => o.MapFrom(m => m.IdentifierId))
            .ForMember(m => m.LegalName, o => o.MapFrom(m => m.LegalName))
            .ForMember(m => m.Uri, o => o.MapFrom(m => !string.IsNullOrWhiteSpace(m.Uri) ? new Uri(m.Uri) : default));

        CreateMap<Organisation.OrganisationAddress, Address>()
            .ForMember(m => m.StreetAddress, o => o.MapFrom(m => m.Address.StreetAddress))
            .ForMember(m => m.Locality, o => o.MapFrom(m => m.Address.Locality))
            .ForMember(m => m.Region, o => o.MapFrom(m => m.Address.Region))
            .ForMember(m => m.PostalCode, o => o.MapFrom(m => m.Address.PostalCode))
            .ForMember(m => m.CountryName, o => o.MapFrom(m => m.Address.CountryName))
            .ForMember(m => m.Country, o => o.MapFrom(m => m.Address.Country))
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type));

        CreateMap<Organisation.ContactPoint, ContactPoint>()
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Name))
            .ForMember(m => m.Email, o => o.MapFrom(m => m.Email))
            .ForMember(m => m.Telephone, o => o.MapFrom(m => m.Telephone))
            .ForMember(m => m.Url, o => o.MapFrom(m => !string.IsNullOrWhiteSpace(m.Url) ? new Uri(m.Url) : default));

        CreateMap<Persistence.SharedConsent, SupplierInformationData>()
            .ForMember(m => m.Form, o => o.MapFrom(m => m))
            .ForMember(m => m.AnswerSets, o => o.MapFrom(m => m.AnswerSets))
            .ForMember(m => m.Questions,
                o => o.MapFrom(m => m.AnswerSets.SelectMany(a => a.Section.Questions).DistinctBy(q => q.Id)));

        CreateMap<Persistence.SharedConsent, Form>()
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Form.Name))
            .ForMember(m => m.SubmissionState, o => o.MapFrom(m => m.SubmissionState.ToString()))
            .ForMember(m => m.SubmittedAt,
                o => o.MapFrom(m => m.SubmittedAt.HasValue ? m.SubmittedAt.Value.DateTime : default))
            .ForMember(m => m.OrganisationId, o => o.MapFrom(m => m.Organisation.Guid))
            .ForMember(m => m.FormId, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.FormVersionId, o => o.MapFrom(m => m.Form.Version))
            .ForMember(m => m.IsRequired, o => o.MapFrom(m => m.Form.IsRequired))
            .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.ShareCode));

        CreateMap<Persistence.FormAnswerSet, FormAnswerSet>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Answers, o => o.MapFrom(m => m.Answers.Where(x => x.Question.Type != Persistence.FormQuestionType.NoInput && x.Question.Type != Persistence.FormQuestionType.CheckYourAnswers)));

        CreateMap<Persistence.FormAnswer, FormAnswer>()
            .ForMember(m => m.QuestionName, o => o.MapFrom(m => m.Question.Guid))
            .ForMember(m => m.BoolValue, o => o.MapFrom(m => m.BoolValue))
            .ForMember(m => m.NumericValue, o => o.MapFrom(m => m.NumericValue))
            .ForMember(m => m.StartValue, o => o.MapFrom(m => m.StartValue))
            .ForMember(m => m.EndValue, o => o.MapFrom(m => m.EndValue))
            .ForMember(m => m.DateValue, o => o.MapFrom(m => ToDateOnly(m.DateValue)))
            .ForMember(m => m.TextValue, o => o.MapFrom(m => m.TextValue))
            .ForMember(m => m.OptionValue, o => o.Ignore());

        CreateMap<Persistence.FormQuestion, FormQuestion>()
            .ForMember(m => m.Type, o => o.MapFrom<CustomFormQuestionTypeResolver>())
            .ForMember(m => m.Title, o => o.MapFrom(m => m.Title))
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Text, o => o.MapFrom(m => m.Description))
            .ForMember(m => m.IsRequired, o => o.MapFrom(m => m.IsRequired))
            .ForMember(m => m.SectionName, o => o.MapFrom(m => m.Section.Title))
            .ForMember(m => m.Options, o => o.MapFrom(m => m.Options.Choices));

        CreateMap<Persistence.FormQuestionChoice, FormQuestionOption>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Id))
            .ForMember(m => m.Value, o => o.MapFrom(m => m.Title));
    }

    private DateOnly? ToDateOnly(DateTime? dateTime) => dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
}

public class CustomFormQuestionTypeResolver : IValueResolver<Persistence.FormQuestion, FormQuestion, FormQuestionType>
{
    public FormQuestionType Resolve(Persistence.FormQuestion source,
        FormQuestion destination, FormQuestionType destMemb, ResolutionContext context)
    {
        switch (source.Type)
        {
            case Persistence.FormQuestionType.YesOrNo:
            case Persistence.FormQuestionType.CheckBox:
                return FormQuestionType.Boolean;
            case Persistence.FormQuestionType.Text:
            case Persistence.FormQuestionType.FileUpload:
            case Persistence.FormQuestionType.Address:
                return FormQuestionType.Text;
            case Persistence.FormQuestionType.SingleChoice:
            case Persistence.FormQuestionType.MultipleChoice:
                return FormQuestionType.Option;
            case Persistence.FormQuestionType.Date:
                return FormQuestionType.Date;
            case Persistence.FormQuestionType.NoInput:
            case Persistence.FormQuestionType.CheckYourAnswers:
                return FormQuestionType.None;
            default:
                return FormQuestionType.None;
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
            default: return "";
        }
    }

    private string ToHtmlString(Persistence.FormAddress source)
    {
        return
            $"{source.StreetAddress}<br/>{source.Locality}<br/>{source.PostalCode}<br/>{source.Region}<br/>{source.CountryName}";
    }
}