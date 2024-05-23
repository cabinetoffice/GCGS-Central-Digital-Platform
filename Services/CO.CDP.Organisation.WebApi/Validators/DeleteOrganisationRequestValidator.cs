using CO.CDP.Organisation.WebApi.Model;
using FluentValidation;

namespace CO.CDP.Organisation.WebApi.Validators;

public class DeleteOrganisationRequestValidator : AbstractValidator<DeleteOrganisationRequest>
{
    public DeleteOrganisationRequestValidator()
    {
        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.")
            .Must(BeAValidGuid).WithMessage("OrganisationId must be a valid GUID.")
            .NotEqual(Guid.Empty).WithMessage("OrganisationId must not be an empty GUID.");
    }

    private bool BeAValidGuid(Guid organisationId)
    {
        return organisationId != Guid.Empty;
    }
}

