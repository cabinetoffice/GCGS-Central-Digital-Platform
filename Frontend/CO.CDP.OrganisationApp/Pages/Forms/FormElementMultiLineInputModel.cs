using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementMultiLineInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? TextInput { get; set; }

    public override FormAnswer? GetAnswer()
    {
        return string.IsNullOrWhiteSpace(TextInput) ? null : new FormAnswer { TextValue = TextInput };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.TextValue != null)
        {
            TextInput = answer.TextValue;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (IsRequired == true && string.IsNullOrWhiteSpace(TextInput))
        {
            yield return new ValidationResult("All information is required on this page", new[] { nameof(TextInput) });
        }

        if (TextInput != null && TextInput.Length > 10000)
        {
            yield return new ValidationResult("Enter 10,000 characters or fewer", new[] { nameof(TextInput) });
        }
    }
}
