using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementFileUploadModel : FormElementModel, IValidatableObject
{
    private const int AllowedMaxFileSizeMB = 25;
    private readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf", ".txt", ".xls", ".xlsx", ".csv", ".docx", ".doc"];

    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    [BindProperty]
    public bool? HasValue { get; set; }

    public string? UploadedFileName { get; set; }

    public override FormAnswer? GetAnswer()
    {
        FormAnswer? formAnswer = null;

        if (HasValue != null)
        {
            formAnswer = new FormAnswer { BoolValue = HasValue };
        }

        if (HasValue != false && !string.IsNullOrWhiteSpace(UploadedFileName))
        {
            formAnswer ??= new FormAnswer();
            formAnswer.TextValue = UploadedFileName;
        }

        return formAnswer;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer == null) return;

        HasValue = answer.BoolValue;
        UploadedFileName = answer.TextValue;
    }

    public (IFormFile formFile, string filename, string contentType)? GetUploadedFileInfo()
    {
        if (UploadedFile != null)
        {
            if (!new FileExtensionContentTypeProvider().TryGetContentType(UploadedFile.FileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return (UploadedFile, SanitizeFilename(UploadedFile.FileName), contentType);
        }

        return null;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validateField = IsRequired;

        if (IsRequired == false)
        {
            if (HasValue == null)
            {
                yield return new ValidationResult("Select an option", [nameof(HasValue)]);
            }
            else if (HasValue == true)
            {
                validateField = true;
            }
        }

        if (validateField)
        {
            if (string.IsNullOrWhiteSpace(UploadedFileName) || UploadedFile != null)
            {
                if (UploadedFile == null)
                {
                    yield return new ValidationResult("No file selected.", [nameof(UploadedFile)]);
                }
                else
                {
                    if (UploadedFile.Length > AllowedMaxFileSizeMB * 1024 * 1024)
                    {
                        yield return new ValidationResult($"The file size must not exceed {AllowedMaxFileSizeMB}MB.", [nameof(UploadedFile)]);
                    }

                    var fileExtension = Path.GetExtension(UploadedFile.FileName).ToLowerInvariant();
                    if (!AllowedExtensions.Contains(fileExtension))
                    {
                        yield return new ValidationResult($"Please upload a file which has one of the following extensions: {string.Join(", ", AllowedExtensions)}", [nameof(UploadedFile)]);
                    }
                }
            }
        }
    }

    private static string SanitizeFilename(string filename)
    {
        // local copy of Path.GetInvalidFileNameChars(), test failing on git pipeline
        var invalidFileNameChars = new char[]
        {
            '\"', '<', '>', '|', '\0',
            (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31, ':', '*', '?', '\\', '/'
        };

        var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", Regex.Escape(new string([.. invalidFileNameChars, ' ', '-'])));

        var newname = Regex.Replace(Path.GetFileNameWithoutExtension(filename), invalidRegStr, "_");

        newname = string.Join("_", newname.Split("_", StringSplitOptions.RemoveEmptyEntries));

        return $"{newname}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{Path.GetExtension(filename)}";
    }
}