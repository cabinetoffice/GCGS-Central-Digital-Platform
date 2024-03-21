using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration
{
    public class OrganisationIdentificationModelTests
    {
        [Fact]
        public void OnGet_ShouldNotThrowException()
        {
            var model = new OrganisationIdentificationModel();
            var exception = Record.Exception(() => model.OnGet());
            exception.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void OnPost_WhenOrganisationTypeIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType)
        {

            var model = new OrganisationIdentificationModel
            {
                OrganisationType = organisationType
            };
            model.ModelState.AddModelError("OrganisationType", "Please select your organisation type");

            var result = model.OnPost();

            result.Should().BeOfType<PageResult>();
            model.ModelState.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("CHN", null)]
        [InlineData("CHN", "")]
        public void OnPost_WhenOrganisationTypeIsCHNAndCompaniesHouseNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string companiesHouseNumber)
        {
            var model = new OrganisationIdentificationModel
            {
                OrganisationType = organisationType,
                CompaniesHouseNumber = companiesHouseNumber
            };
            model.ModelState.AddModelError("CompaniesHouseNumber", "The Companies House Number field is required.");

            var result = model.OnPost();

            result.Should().BeOfType<PageResult>();
            model.ModelState.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("DUN", null)]
        [InlineData("DUN", "")]
        public void OnPost_WhenOrganisationTypeIsDUNAndDunBradstreetNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string dunBradstreetNumber)
        {
            var model = new OrganisationIdentificationModel
            {
                OrganisationType = organisationType,
                DunBradstreetNumber = dunBradstreetNumber
            };
            model.ModelState.AddModelError("DunBradstreetNumber", "The Dun & Bradstreet Number field is required.");

            var result = model.OnPost();

            result.Should().BeOfType<PageResult>();
            model.ModelState.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("CCEW", null)]
        [InlineData("CCEW", "")]
        public void OnPost_WhenOrganisationTypeIsCCEWAndCharityCommissionEnglandWalesNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string charityCommissionEnglandWalesNumber)
        {
            var model = new OrganisationIdentificationModel
            {
                OrganisationType = organisationType,
                CharityCommissionEnglandWalesNumber = charityCommissionEnglandWalesNumber
            };
            model.ModelState.AddModelError("CharityCommissionEnglandWalesNumber", "The Charity Commission for England & Wales Number field is required.");

            var result = model.OnPost();

            result.Should().BeOfType<PageResult>();
            model.ModelState.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("OSCR", null)]
        [InlineData("OSCR", "")]
        public void OnPost_WhenOrganisationTypeIsOSCRAndOfficeOfScottishCharityRegulatorNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string officeOfScottishCharityRegulatorNumber)
        {
            var model = new OrganisationIdentificationModel
            {
                OrganisationType = organisationType,
                OfficeOfScottishCharityRegulatorNumber = officeOfScottishCharityRegulatorNumber
            };
            model.ModelState.AddModelError("OfficeOfScottishCharityRegulatorNumber", "The Office of the Scottish Charity Regulator (OSCR) Number field is required.");

            var result = model.OnPost();

            result.Should().BeOfType<PageResult>();
            model.ModelState.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("CCNI", null)]
        [InlineData("CCNI", "")]
        public void OnPost_WhenOrganisationTypeIsCCNIAndCharityCommissionNorthernIrelandNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string charityCommissionNorthernIrelandNumber)
        {
            var model = new OrganisationIdentificationModel
            {
                OrganisationType = organisationType,
                CharityCommissionNorthernIrelandNumber = charityCommissionNorthernIrelandNumber
            };
            model.ModelState.AddModelError("CharityCommissionNorthernIrelandNumber", "The Charity Commission for Northren Ireland Number field is required.");

            var result = model.OnPost();

            result.Should().BeOfType<PageResult>();
            model.ModelState.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("NHOR", null)]
        [InlineData("NHOR", "")]
        public void OnPost_WhenOrganisationTypeIsNHORAndNationalHealthServiceOrganisationsRegistryNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string nationalHealthServiceOrganisationsRegistryNumber)
        {
            var model = new OrganisationIdentificationModel
            {
                OrganisationType = organisationType,
                NationalHealthServiceOrganisationsRegistryNumber = nationalHealthServiceOrganisationsRegistryNumber
            };
            model.ModelState.AddModelError("NationalHealthServiceOrganisationsRegistryNumber", "The National health Service Organisations Registry Number field is required.");

            var result = model.OnPost();

            result.Should().BeOfType<PageResult>();
            model.ModelState.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("DFE", null)]
        [InlineData("DFE", "")]
        public void OnPost_WhenOrganisationTypeIsDFEAndDepartmentForEducationNumberIsNullOrEmpty_ShouldReturnPageWithModelStateError(string organisationType, string departmentForEducationNumber)
        {

            var model = new OrganisationIdentificationModel
            {
                OrganisationType = organisationType,
                DepartmentForEducationNumber = departmentForEducationNumber
            };
            model.ModelState.AddModelError("DepartmentForEducationNumber", "The Department For Education Number field is required.");

            var result = model.OnPost();

            result.Should().BeOfType<PageResult>();
            model.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void OnPost_WhenModelStateIsValid_ShouldRedirectToOrganisationRegisteredAddress()
        {
            var model = new OrganisationIdentificationModel
            {
                OrganisationType = "Other"
            };
            var result = model.OnPost();
            result.Should().BeOfType<RedirectToPageResult>();
            (result as RedirectToPageResult)?.PageName.Should().Be("./OrganisationRegisteredAddress");
        }
    }
}
