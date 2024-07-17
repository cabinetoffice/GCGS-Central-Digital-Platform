using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class DynamicFormsPageModel(IFormsEngine formsEngine, ITempDataService tempDataService) : PageModel
{
    public SectionQuestionsResponse? SectionWithQuestions { get; set; }
    public Models.FormQuestion? CurrentQuestion { get; set; }
    public Guid FormId { get; set; }
    public Guid SectionId { get; set; }
    public Guid OrganisationId { get; set; }
    public bool IsFirstQuestion => IsCurrentQuestionFirst();

    public string EncType => GetEncType();
    public Guid? PreviousQuestionId { get; private set; }
    public new HttpRequest? Request { get; set; }

    [BindProperty]
    public string? Answer { get; set; }

    [BindProperty]
    public string? UploadedFile { get; set; }
    

    public async Task OnGetAsync(Guid organisationId, Guid formId, Guid sectionId, Guid? questionId)
    {
        OrganisationId = organisationId;
        FormId = formId;
        SectionId = sectionId;

        await LoadSectionWithQuestionsAsync(formId, sectionId);

        if (SectionWithQuestions?.Questions != null && SectionWithQuestions.Questions.Any())
        {
            CurrentQuestion = questionId.HasValue
                ? await formsEngine.GetCurrentQuestion(formId, sectionId, questionId.Value)
                : SectionWithQuestions.Questions.FirstOrDefault();

            if (CurrentQuestion != null)
            {
                RetrieveAnswerFromTempData();
            }

            SaveCurrentQuestionIdToTempData(CurrentQuestion?.Id);
            SetPreviousQuestionId();
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId, string action)
    {
        OrganisationId = organisationId;
        FormId = formId;
        SectionId = sectionId;

        await LoadSectionWithQuestionsAsync(formId, sectionId);

        if (SectionWithQuestions?.Questions != null && SectionWithQuestions.Questions.Any())
        {
            CurrentQuestion = SectionWithQuestions.Questions.FirstOrDefault(q => q.Id == currentQuestionId);

            if (CurrentQuestion == null)
            {
                return Page();
            }

            if (action == "next")
            {
                if (!ValidateCurrentQuestion())
                {
                    return Page();
                }

                SaveAnswerToTempData();
                CurrentQuestion = await formsEngine.GetNextQuestion(FormId, SectionId, currentQuestionId);
            }
            else if (action == "back")
            {
                CurrentQuestion = await formsEngine.GetPreviousQuestion(FormId, SectionId, currentQuestionId);
            }

            SaveCurrentQuestionIdToTempData(CurrentQuestion?.Id);
            SetPreviousQuestionId();
        }

        return Page();
    }

    public string? GetPartialViewName(FormQuestionType questionType)
    {
        return FormQuestionPartials.TryGetValue(questionType, out var partialView) ? partialView : null;
    }

    private static readonly Dictionary<FormQuestionType, string> FormQuestionPartials = new()
        {
            { FormQuestionType.NoInput, "_FormElementNoInput" },
            { FormQuestionType.YesOrNo, "_FormElementYesNoInput" },
            { FormQuestionType.FileUpload, "_FormElementFileUpload" },
            { FormQuestionType.Date, "_FormElementDateInput" },
            { FormQuestionType.Text, "_FormElementTextInput" }
        };

    private async Task LoadSectionWithQuestionsAsync(Guid formId, Guid sectionId)
    {
        SectionWithQuestions = await formsEngine.LoadFormSectionAsync(formId, sectionId);
    }

    private void SaveCurrentQuestionIdToTempData(Guid? questionId)
    {
        var key = $"CurrentQuestionId_{FormId}_{SectionId}";
        tempDataService.Put(key, questionId);
    }

    private void SetPreviousQuestionId()
    {
        if (CurrentQuestion == null || SectionWithQuestions?.Questions == null)
        {
            PreviousQuestionId = null;
            return;
        }

        var currentIndex = SectionWithQuestions.Questions.FindIndex(q => q.Id == CurrentQuestion.Id);

        PreviousQuestionId = currentIndex > 0
            ? SectionWithQuestions.Questions[currentIndex - 1].Id
            : (Guid?)null;
    }

    private bool IsCurrentQuestionFirst()
    {
        return CurrentQuestion?.Id == SectionWithQuestions?.Questions?.FirstOrDefault()?.Id;
    }

    private string GetEncType()
    {
        return CurrentQuestion?.Type == FormQuestionType.FileUpload ? "multipart/form-data" : "application/x-www-form-urlencoded";
    }

    private bool ValidateCurrentQuestion()
    {
        if (CurrentQuestion?.Type == FormQuestionType.YesOrNo)
        {
            return ValidateYesNoAnswer();
        }

        if (CurrentQuestion?.Type == FormQuestionType.Text)
        {
            return ValidateTextAnswer();
        }

        if (CurrentQuestion?.Type == FormQuestionType.FileUpload)
        {
            return ValidateFileUpload();
        }

        // Future validation for other question types can be added here

        return true;
    }

    private bool ValidateYesNoAnswer()
    {
        if (string.IsNullOrEmpty(Answer))
        {
            ModelState.AddModelError("Answer", "Please select an option.");
            return false;
        }

        return true;
    }

    private bool ValidateTextAnswer()
    {
        if (string.IsNullOrEmpty(Answer))
        {
            ModelState.AddModelError("Answer", "Please enter a value.");
            return false;
        }

        return true;
    }

    public bool ValidateFileUpload()
    {           
        var allowedFileSizeMB = 10;
        var allowedExtensions = new List<string> { ".pdf", ".docx", ".csv", ".jpg", ".bmp", ".png", ".tif" };

        if (Request?.Form == null || Request.Form.Files == null || Request.Form.Files.Count == 0)
        {
            ModelState.AddModelError("Answer", "No file selected.");
            return false;
        }

        var file = Request.Form.Files["UploadedFile"];

        if (file == null)
        {
            ModelState.AddModelError("Answer", "No file selected.");
            return false;
        }

        var maxFileLength = allowedFileSizeMB * 1024 * 1024;

        if (file.Length > maxFileLength)
        {
            ModelState.AddModelError("Answer", $"The file size must not exceed {allowedFileSizeMB}MB.");
            return false;
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            ModelState.AddModelError("Answer", $"Please upload a file which has one of the following extensions: {string.Join(", ", allowedExtensions)}");
            return false;
        }

        return true;
    }

    private void SaveAnswerToTempData()
    {
        if (CurrentQuestion != null)
        {
            var questionAnswer = new QuestionAnswer
            {
                QuestionId = CurrentQuestion.Id,
                Answer = Answer
            };

            var key = $"Answer_{FormId}_{SectionId}_{CurrentQuestion.Id}";
            tempDataService.Put(key, questionAnswer);
        }
    }

    private void RetrieveAnswerFromTempData()
    {
        if (CurrentQuestion != null)
        {
            var key = $"Answer_{FormId}_{SectionId}_{CurrentQuestion.Id}";
            var questionAnswer = tempDataService.Get<QuestionAnswer>(key);
            Answer = questionAnswer?.Answer ?? string.Empty;
        }
    }
}