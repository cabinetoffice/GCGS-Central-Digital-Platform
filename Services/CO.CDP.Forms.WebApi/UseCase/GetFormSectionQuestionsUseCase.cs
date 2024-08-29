using AutoMapper;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Forms.WebApi.UseCase;

public class GetFormSectionQuestionsUseCase(IFormRepository formRepository, IMapper mapper)
    : IUseCase<(Guid formId, Guid sectionId, Guid organisationId), SectionQuestionsResponse?>
{
    public async Task<SectionQuestionsResponse?> Execute((Guid formId, Guid sectionId, Guid organisationId) input)
    {
        var (formId, sectionId, organisationId) = input;

        var section = await formRepository.GetSectionAsync(formId, sectionId);

        if (section == null)
            return null;

        var answerSets = await formRepository.GetFormAnswerSetsAsync(sectionId, organisationId);
        if (answerSets != null)
        {
            var currentSharedConsent = answerSets.Select(x => x.SharedConsent).OrderByDescending(x => x.UpdatedOn).FirstOrDefault();
            answerSets = answerSets.Where(x => x.SharedConsent == currentSharedConsent).ToList();
        }

        return new SectionQuestionsResponse
        {
            Section = mapper.Map<FormSection>(section),
            Questions = mapper.Map<List<FormQuestion>>(section.Questions),
            AnswerSets = mapper.Map<List<FormAnswerSet>>(answerSets)
        };
    }
}