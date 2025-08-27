using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class FormsLandingPage(
    IFormsClient formsClient,
    IFormsEngine formsEngine,
    IUserInfoService userInfoService,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SectionQuestionsResponse form;
        try
        {
            form = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId, OrganisationId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        tempDataService.Remove($"Form_{OrganisationId}_{FormId}_{SectionId}_Questions");
        tempDataService.Remove($"Form_{OrganisationId}_{FormId}_{SectionId}_Answers");

        if (form.Section.Type != FormSectionType.Declaration)
        {
            if (form.Section.Type == FormSectionType.AdditionalSection)
            {
                return await HandleAdditionalSectionAsync(form);
            }

            if (await userInfoService.IsViewer())
            {
                return RedirectToPage("FormsAnswerSetSummary", new { OrganisationId, FormId, SectionId });
            }

            if (form.AnswerSets.Count != 0)
            {
                if (form.AnswerSets.Any(a => a.FurtherQuestionsExempted == true))
                {
                    return RedirectToPage("FormsCheckFurtherQuestionsExempted", new { OrganisationId, FormId, SectionId });
                }
                else
                {
                    return RedirectToPage("FormsAnswerSetSummary", new { OrganisationId, FormId, SectionId });
                }
            }
            else
            {
                if (form.Section.CheckFurtherQuestionsExempted)
                {
                    return RedirectToPage("FormsCheckFurtherQuestionsExempted", new { OrganisationId, FormId, SectionId });
                }
            }
        }

        var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, null);
        if (currentQuestion == null)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("FormsQuestionPage",
            new { OrganisationId, FormId, SectionId, CurrentQuestionId = currentQuestion.Id });
    }

    private Task<IActionResult> HandleAdditionalSectionAsync(SectionQuestionsResponse form)
    {
        return Task.FromResult<IActionResult>(RedirectToPage("FormsAdditionalSummary", new { organisationId = OrganisationId, formId = FormId, sectionId = SectionId }));
    }

    private IActionResult CreateRedirectResult(string pageName) =>
        RedirectToPage(pageName, new { OrganisationId, FormId, SectionId });

    private IActionResult CreateRedirectToQuestionResult(Guid questionId) =>
        RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = questionId });

    private static FormQuestion? FindCheckYourAnswersQuestion(IEnumerable<FormQuestion>? questions) =>
        questions?.FirstOrDefault(q => q.Type == FormQuestionType.CheckYourAnswers);

    private async Task<Result<Guid>> TryGetCurrentQuestionAsync()
    {
        var question = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, null);
        return question != null
            ? Result.Success(question.Id)
            : Result.Failure<Guid>("Current question not found");
    }

    private class Result<T>
    {
        private Result(bool isSuccess, T value, string error)
        {
        }

        public static Result<T> Success(T value) => new(true, value, string.Empty);

        public static Result<T> Failure(string error) => new(false, default!, error);
    }

    private static class Result
    {
        public static Result<T> Success<T>(T value) => Result<T>.Success(value);

        public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
    }
}