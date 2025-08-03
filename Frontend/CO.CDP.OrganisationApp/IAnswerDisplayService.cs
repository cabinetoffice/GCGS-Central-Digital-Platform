using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp;

public interface IAnswerDisplayService
{
    Task<string> FormatAnswerForDisplayAsync(QuestionAnswer questionAnswer, FormQuestion question);
}