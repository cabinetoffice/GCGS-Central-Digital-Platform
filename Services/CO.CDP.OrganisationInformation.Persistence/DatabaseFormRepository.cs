using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseFormRepository(OrganisationInformationContext context) : IFormRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<FormSection?> GetSectionAsync(Guid formId, Guid sectionId)
    {
        var formId1 = Guid.NewGuid();
        var sectionId1 = Guid.NewGuid();

        var form = GivenForm(formId1);

        var section = GivenSection(sectionId1, form);

        form.Sections.Add(section);

        //await SaveFormAsync(form);

        return await context.Set<FormSection>()
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Form.Guid == formId && s.Guid == sectionId);
    }

    public async Task<IEnumerable<FormQuestion>> GetQuestionsAsync(Guid sectionId)
    {
        return await context.Set<FormQuestion>().Where(q => q.Section.Guid == sectionId).ToListAsync();
    }

    private static FormSection GivenSection(Guid sectionId, Form form)
    {
        return new FormSection
        {
            Guid = sectionId,
            Form = form,
            Questions = [],
            Title = "Test Section",
            AllowsMultipleAnswerSets = true,
            Configuration = new FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };
    }

    private static Form GivenForm(Guid formId)
    {
        return new Form
        {
            Guid = formId,
            Name = "Test Form",
            Version = "1.0",
            IsRequired = true,
            Scope = FormScope.SupplierInformation,
            Sections = [],
            Type = FormType.Standard
        };
    }

    public async Task SaveFormAsync(Form form)
    {
        context.Update(form);
        await context.SaveChangesAsync();
    }
}