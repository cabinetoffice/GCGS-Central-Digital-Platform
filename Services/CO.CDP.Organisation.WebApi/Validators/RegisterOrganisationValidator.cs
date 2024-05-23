using CO.CDP.Organisation.WebApi.Model;
using FluentValidation;
namespace CO.CDP.Organisation.WebApi.Validators;

public class RegisterOrganisationValidator : AbstractValidator<RegisterOrganisation>
{
    public RegisterOrganisationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name must not be empty.");
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("PersonId must not be empty.")
                                 .Must(id => Guid.TryParse(id.ToString(), out _))
                                 .WithMessage("PersonId must be a valid GUID.");
    }
}