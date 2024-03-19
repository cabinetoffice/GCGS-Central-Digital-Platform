using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class ModelValidationHelper
{
	public static IList<ValidationResult> Validate(object model)
	{
		var results = new List<ValidationResult>();
		var validationContext = new ValidationContext(model, null, null);
		Validator.TryValidateObject(model, validationContext, results, true);
		if (model is IValidatableObject result)
		{
			result.Validate(validationContext);
		}
		return results;
	}
}