using CO.CDP.Forms.WebApiClient;
using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

[Authorize]
public class FormsAddAnotherAnswerSetModel(
    IFormsClient formsClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty]
    [RequiredIf(nameof(AllowsMultipleAnswerSets), true, ErrorMessage = "Please select an option")]
    public bool? AddAnotherAnswerSet { get; set; }

    public ICollection<FormAnswerSet> FormAnswerSets { get; set; } = [];

    public string? Heading { get; set; }

    public string? AddAnotherAnswerLabel { get; set; }

    public bool AllowsMultipleAnswerSets { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var verify = await InitAndVerifyPage();
        if (!verify)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var verify = await InitAndVerifyPage();
        if (!verify)
        {
            return Redirect("/page-not-found");
        }

        ModelState.Clear();
        if (!TryValidateModel(this))
        {
            return Page();
        }

        if (AddAnotherAnswerSet == true)
        {
            return RedirectToPage("DynamicFormsPage", new { OrganisationId, FormId, SectionId });
        }
        else
        {
            return RedirectToPage("SupplierBasicInformation", new { OrganisationId });
        }
    }

    public async Task<IActionResult> OnGetChange([FromQuery(Name = "answer-set-id")] Guid answerSetId)
    {
        SectionQuestionsResponse? response;
        try
        {
            response = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        var answerSet = response?.AnswerSets.FirstOrDefault(a => a.Id == answerSetId);
        if (answerSet == null)
        {
            return Redirect("/page-not-found");
        }

        var state = new Models.FormQuestionAnswerState
        {
            AnswerSetId = answerSetId,
            Answers = answerSet.Answers.Select(a => new Models.QuestionAnswer
            {
                QuestionId = a.QuestionId,
                Answer = new Models.FormAnswer
                {
                    BoolValue = a.BoolValue,
                    DateValue = a.DateValue,
                    EndValue = a.EndValue,
                    NumericValue = a.NumericValue,
                    OptionValue = a.OptionValue,
                    StartValue = a.StartValue,
                    TextValue = a.TextValue
                }
            }).ToList()
        };

        tempDataService.Put($"Form_{OrganisationId}_{FormId}_{SectionId}_Answers", state);

        var checkYourAnswersQuestionId = response?.Questions?.FirstOrDefault(q => q.Type == FormQuestionType.CheckYourAnswers)?.Id;

        return RedirectToPage("DynamicFormsPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = checkYourAnswersQuestionId });
    }

    private async Task<bool> InitAndVerifyPage()
    {
        try
        {
            var response = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId);
            FormAnswerSets = response.AnswerSets;
            var valid = response != null;

            if (valid)
            {
                AllowsMultipleAnswerSets = response!.Section.AllowsMultipleAnswerSets;
                AddAnotherAnswerLabel = response.Section.Configuration.AddAnotherAnswerLabel;
                Heading = response.Section.Configuration.SingularSummaryHeading;

                if (FormAnswerSets.Count > 1)
                {
                    Heading = string.Format(response.Section.Configuration.PluralSummaryHeadingFormat, FormAnswerSets.Count);
                }
            }

            return valid;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }
    }
}