using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Authentication;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Forms.WebApi.UseCase;

public class GetFormSectionQuestionsUseCase(IFormRepository formRepository, IMapper mapper)
    : IUseCase<(Guid formId, Guid sectionId), SectionQuestionsResponse?>
{

    public async Task<SectionQuestionsResponse?> Execute((Guid formId, Guid sectionId) input)
    {
        var (formId, sectionId) = input;

        var section = await formRepository.GetSectionAsync(formId, sectionId);

        if (section == null)
            return null;

        return new SectionQuestionsResponse
        {
            Section = mapper.Map<FormSection>(section),
            Questions = mapper.Map<List<FormQuestion>>(section.Questions)
        };
    }
}