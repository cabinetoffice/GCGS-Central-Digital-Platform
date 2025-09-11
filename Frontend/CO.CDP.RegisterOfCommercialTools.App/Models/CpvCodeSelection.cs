using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class CpvCodeSelection : IValidatableObject
{
    public List<string> SelectedCodes { get; set; } = [];

    public List<CpvCode> SelectedItems { get; set; } = [];

    public string? SearchQuery { get; set; }

    public List<string> ExpandedNodes { get; set; } = [];

    public const int MaxSelections = 20;

    public bool IsAtMaxSelections => SelectedCodes.Count >= MaxSelections;

    public int RemainingSelections => Math.Max(0, MaxSelections - SelectedCodes.Count);

    public void AddSelection(string code, string descriptionEn, string descriptionCy)
    {
        if (!SelectedCodes.Contains(code) && !IsAtMaxSelections)
        {
            SelectedCodes.Add(code);
            SelectedItems.Add(new CpvCode { 
                Code = code, 
                DescriptionEn = descriptionEn, 
                DescriptionCy = descriptionCy, 
                IsSelected = true 
            });
        }
    }

    public void RemoveSelection(string code)
    {
        SelectedCodes.Remove(code);
        SelectedItems.RemoveAll(x => x.Code == code);
    }

    public bool IsSelected(string code) => SelectedCodes.Contains(code);

    public void Clear()
    {
        SelectedCodes.Clear();
        SelectedItems.Clear();
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SelectedCodes.Count > MaxSelections)
        {
            yield return new ValidationResult(
                $"The number of selections must be {MaxSelections} or fewer",
                [nameof(SelectedCodes)]
            );
        }
    }
}