using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class FormsAnswerSetSummaryModel(
    IFormsClient formsClient,
    IFormsEngine formsEngine,
    ITempDataService tempDataService,
    IChoiceProviderService choiceProviderService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty]
    public bool? AddAnotherAnswerSet { get; set; }

    public List<(Guid answerSetId, IEnumerable<AnswerSummary> answers)> FormAnswerSets { get; set; } = [];

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

        if (AllowsMultipleAnswerSets && !AddAnotherAnswerSet.HasValue)
            ModelState.AddModelError(nameof(AddAnotherAnswerSet), "Please select an option");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (AddAnotherAnswerSet == true)
        {
            var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, null);
            if (currentQuestion == null)
            {
                return Redirect("/page-not-found");
            }
            return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = currentQuestion.Id });
        }
        else
        {
            return RedirectToPage("../Supplier/SupplierInformationSummary", new { Id = OrganisationId });
        }
    }

    public async Task<IActionResult> OnGetChange([FromQuery(Name = "answer-set-id")] Guid answerSetId)
    {
        SectionQuestionsResponse response;
        try
        {
            response = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId, OrganisationId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        var answerSet = response.AnswerSets.FirstOrDefault(a => a.Id == answerSetId);
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
                AnswerId = a.Id,
                Answer = new Models.FormAnswer
                {
                    BoolValue = a.BoolValue,
                    DateValue = a.DateValue,
                    EndValue = a.EndValue,
                    NumericValue = a.NumericValue,
                    OptionValue = a.OptionValue,
                    StartValue = a.StartValue,
                    TextValue = a.TextValue,
                    AddressValue = MapAddress(a.AddressValue),
                    JsonValue = a.JsonValue,
                }
            }).ToList()
        };

        tempDataService.Put($"Form_{OrganisationId}_{FormId}_{SectionId}_Answers", state);

        var checkYourAnswersQuestionId = response?.Questions?.FirstOrDefault(q => q.Type == FormQuestionType.CheckYourAnswers)?.Id;

        return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = checkYourAnswersQuestionId });
    }

    private static Models.Address? MapAddress(FormAddress? formAdddress)
    {
        if (formAdddress == null) return null;
        return new Models.Address
        {
            AddressLine1 = formAdddress.StreetAddress,
            TownOrCity = formAdddress.Locality,
            Postcode = formAdddress.PostalCode,
            CountryName = formAdddress.CountryName,
            Country = formAdddress.Country
        };
    }

    private async Task<bool> InitAndVerifyPage()
    {
        SectionQuestionsResponse form;
        try
        {
            form = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId, OrganisationId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }

        AllowsMultipleAnswerSets = form.Section.AllowsMultipleAnswerSets;
        AddAnotherAnswerLabel = form.Section.Configuration.AddAnotherAnswerLabel;

        if (form.AnswerSets.Any(a => a.FurtherQuestionsExempted == true))
        {
            Heading = "Not Applicable";
        }
        else
        {
            FormAnswerSets = await GetAnswers(form);
            Heading = form.Section.Configuration.SingularSummaryHeading;

            if (FormAnswerSets.Count != 1 && form.Section.Configuration.PluralSummaryHeadingFormat != null)
            {
                Heading = string.Format(form.Section.Configuration.PluralSummaryHeadingFormat, FormAnswerSets.Count);
            }
        }
        return true;
    }

    private async Task<List<(Guid answerSetId, IEnumerable<AnswerSummary> answers)>> GetAnswers(SectionQuestionsResponse form)
    {
        List<(Guid answerSetId, IEnumerable<AnswerSummary> answers)> summaryList = [];

        foreach (var answerSet in form.AnswerSets)
        {
            List<AnswerSummary> answerSummaries = [];
            foreach (var answer in answerSet.Answers)
            {
                var question = form.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question != null && question.Type != FormQuestionType.NoInput && question.Type != FormQuestionType.CheckYourAnswers)
                {
                    var choiceProviderStrategy = choiceProviderService.GetStrategy(question.Options.ChoiceProviderStrategy);
                    var answerString = question.Type switch
                    {
                        FormQuestionType.Text => answer.TextValue ?? "",
                        FormQuestionType.FileUpload => answer.TextValue ?? "",
                        FormQuestionType.YesOrNo => answer.BoolValue.HasValue ? (answer.BoolValue == true ? "Yes" : "No") : "",
                        FormQuestionType.Date => answer.DateValue.HasValue ? answer.DateValue.Value.ToString("dd/MM/yyyy") : "",
                        FormQuestionType.CheckBox => answer.BoolValue.HasValue ? question.Options.Choices?.FirstOrDefault()?.Title ?? "" : "",
                        FormQuestionType.Address => answer.AddressValue != null ? $"{answer.AddressValue.StreetAddress}, {answer.AddressValue.Locality}, {answer.AddressValue.PostalCode}, {answer.AddressValue.CountryName}" : "",
                        FormQuestionType.SingleChoice => await choiceProviderStrategy.RenderOption(answer),
                        FormQuestionType.MultiLine => answer.TextValue ?? "",
                        FormQuestionType.Url => answer.TextValue ?? "",
                        _ => ""
                    };

                    answerSummaries.Add(new AnswerSummary
                    {
                        Title = question.SummaryTitle ?? question.Title,
                        Answer = answerString ?? ""
                    });
                }
            }

            summaryList.Add((answerSet.Id, answerSummaries));
        }

        return summaryList;
    }
}