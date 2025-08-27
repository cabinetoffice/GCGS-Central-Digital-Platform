using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using FormAnswer = CO.CDP.OrganisationApp.Models.FormAnswer;

namespace CO.CDP.OrganisationApp.Pages.Forms;

[FeatureGate(FeatureFlags.SupplierAdditionalModule)]
[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class FormsAdditionalSummaryModel(
    IFormsClient formsClient,
    IChoiceProviderService choiceProviderService,
    EvaluatorFactory evaluatorFactory,
    IAuthorizationService authorizationService,
    ITempDataService tempDataService,
    IFormsEngine formsEngine) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    public List<AnswerSummary> AnswerSummaries { get; set; } = [];
    public string? SectionTitle { get; set; }

    public async Task<IActionResult> OnGet()
    {
        CO.CDP.Forms.WebApiClient.SectionQuestionsResponse form;
        try
        {
            form = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId, OrganisationId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        if (form.Section.Type != CO.CDP.Forms.WebApiClient.FormSectionType.AdditionalSection)
        {
            return RedirectToPage("FormsAnswerSetSummary", new { OrganisationId, FormId, SectionId });
        }

        var authResult = await authorizationService.AuthorizeAsync(User, new { OrganisationId }, OrgScopeRequirement.Editor);
        if (authResult.Succeeded)
        {
            if (form.AnswerSets.Count > 0)
            {
                var answerSet = form.AnswerSets.First();
                var state = new FormQuestionAnswerState
                {
                    AnswerSetId = answerSet.Id,
                    Answers = answerSet.Answers.Select(a => new QuestionAnswer
                    {
                        QuestionId = a.QuestionId,
                        AnswerId = a.Id,
                        Answer = new FormAnswer
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

                var checkYourAnswersQuestionId = form.Questions?.FirstOrDefault(q => q.Type == CO.CDP.Forms.WebApiClient.FormQuestionType.CheckYourAnswers)?.Id;

                if (checkYourAnswersQuestionId.HasValue)
                {
                    return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = checkYourAnswersQuestionId });
                }

                var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, null);
                if (currentQuestion == null)
                {
                    return Redirect("/page-not-found");
                }

                return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = currentQuestion.Id });
            }
            else
            {
                var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, null);
                if (currentQuestion == null)
                {
                    return Redirect("/page-not-found");
                }

                return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = currentQuestion.Id });
            }
        }

        SectionTitle = form.Section.Title;

        if (form.AnswerSets.Count > 0)
        {
            var answerSet = form.AnswerSets.First();

            IAnswerSummaryStrategy strategy = form.Section.Configuration.SummaryRenderFormatter != null
                ? new FormatterAnswerSummaryStrategy(evaluatorFactory, choiceProviderService)
                : new DefaultAnswerSummaryStrategy(choiceProviderService);

            var context = new AnswerSummaryContext(strategy);
            AnswerSummaries = (await context.GetAnswerSummaries(form, answerSet)).ToList();
        }

        return Page();
    }

    private static Address? MapAddress(FormAddress? formAddress)
    {
        if (formAddress == null) return null;
        return new Address
        {
            AddressLine1 = formAddress.StreetAddress,
            TownOrCity = formAddress.Locality,
            Postcode = formAddress.PostalCode,
            CountryName = formAddress.CountryName,
            Country = formAddress.Country
        };
    }
}