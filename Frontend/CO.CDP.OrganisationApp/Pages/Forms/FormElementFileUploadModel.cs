using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementFileUploadModel : FormElementModel, IValidatableObject
{
    private const int AllowedMaxFileSizeMB = 10;
    private readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf", ".txt", ".xls", ".xlsx", ".csv", ".docx", ".doc"];

    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    public string? UploadedFileName { get; set; }

    public override FormAnswer? GetAnswer()
    {
        return string.IsNullOrWhiteSpace(UploadedFileName) ? null : new FormAnswer { TextValue = UploadedFileName };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.TextValue != null)
        {
            UploadedFileName = answer.TextValue;
        }
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
        if (CurrentFormQuestionType == FormQuestionType.FileUpload && IsRequired == true && string.IsNullOrWhiteSpace(UploadedFileName))
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

    private static string SanitizeFilename(string filename)
    {
        var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", Regex.Escape(new string([.. Path.GetInvalidFileNameChars(), ' ', '-'])));

        var newname = Regex.Replace(Path.GetFileNameWithoutExtension(filename), invalidRegStr, "_");

        newname = string.Join("_", newname.Split("_", StringSplitOptions.RemoveEmptyEntries));

        return $"{newname}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{Path.GetExtension(filename)}";
    }
}