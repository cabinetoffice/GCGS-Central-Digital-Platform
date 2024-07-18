using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementFileUploadModel : FormElementModel, IValidatableObject
{
    private const int AllowedMaxFileSizeMB = 10;
    private readonly string[] AllowedExtensions = [".pdf", ".docx", ".csv", ".jpg", ".bmp", ".png", ".tif"];

    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    public string? UploadedFileName { get; set; }

    public override string? GetAnswer()
    {
        return UploadedFile?.FileName ?? UploadedFileName;
    }

    public override void SetAnswer(string? answer)
    {
        UploadedFileName = answer;
    }

    public (IFormFile formFile, string filename, string contentType)? GetUploadedFileInfo()
    {
        if (UploadedFile != null)
        {
            var filename = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{Path.GetExtension(UploadedFile.FileName)}";

            if (!new FileExtensionContentTypeProvider().TryGetContentType(UploadedFile.Name, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return (UploadedFile, filename, contentType);
        }

        return null;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FormQuestionType == Models.FormQuestionType.FileUpload && IsRequired == true && string.IsNullOrWhiteSpace(UploadedFileName))
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