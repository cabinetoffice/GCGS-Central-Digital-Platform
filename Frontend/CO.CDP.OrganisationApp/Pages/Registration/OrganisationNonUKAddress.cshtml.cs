using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
[ValidateRegistrationStep]
public class OrganisationNonUKAddressModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => OrganisationNonUKAddressPage;
    public override ISession SessionContext => session;

    [BindProperty]
    [DisplayName("Address line 1")]
    [Required(ErrorMessage = "Enter your address line 1")]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [DisplayName("Address line 2 (optional)")]
    public string? AddressLine2 { get; set; }

    [BindProperty]
    [DisplayName("Town or city")]
    [Required(ErrorMessage = "Enter your town or city")]
    public string? TownOrCity { get; set; }

    [BindProperty]
    [DisplayName("Country, State or Province (optional)")]
    public string? Region { get; set; }

    [BindProperty]
    [DisplayName("Postal or Zip code")]
    [Required(ErrorMessage = "Enter your postcode")]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName("Country")]
    [Required(ErrorMessage = "Enter your country")]
    public string? Country { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IEnumerable<string> Countries { get; } = ["Afghanistan", "Åland Islands", "Albania", "Algeria", "American Samoa", "Andorra", "Angola", "Anguilla", "Antarctica", "Antigua and Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bosnia and Herzegovina", "Botswana", "Bouvet Island", "Brazil", "British Indian Ocean Territory", "Brunei Darussalam", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Canada", "Cape Verde", "Cayman Islands", "Central African Republic", "Chad", "Chile", "China", "Christmas Island", "Cocos (Keeling) Islands", "Colombia", "Comoros", "Congo", "Congo, The Democratic Republic of the", "Cook Islands", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Ethiopia", "Falkland Islands (Malvinas)", "Faroe Islands", "Fiji", "Finland", "France", "French Guiana", "French Polynesia", "French Southern Territories", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Grenada", "Guadeloupe", "Guam", "Guatemala", "Guernsey", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Heard Island and Mcdonald Islands", "Holy See (Vatican City State)", "Honduras", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iran, Islamic Republic Of", "Iraq", "Ireland", "Isle of Man", "Israel", "Italy", "Jamaica", "Japan", "Jersey", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Korea, Republic of", "Kuwait", "Kyrgyzstan", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libyan Arab Jamahiriya", "Liechtenstein", "Lithuania", "Luxembourg", "Macao", "North Macedonia", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Martinique", "Mauritania", "Mauritius", "Mayotte", "Mexico", "Micronesia, Federated States of", "Moldova, Republic of", "Monaco", "Mongolia", "Montserrat", "Morocco", "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal", "Netherlands", "Netherlands Antilles", "New Caledonia", "New Zealand", "Nicaragua", "Niger", "Nigeria", "Niue", "Norfolk Island", "Northern Mariana Islands", "Norway", "Oman", "Pakistan", "Palau", "Palestinian Territory, Occupied", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Pitcairn Islands", "Poland", "Portugal", "Puerto Rico", "Qatar", "Reunion", "Romania", "Russian Federation", "Rwanda", "Saint Helena", "Saint Kitts and Nevis", "Saint Lucia", "Saint Pierre and Miquelon", "Saint Vincent and the Grenadines", "Samoa", "San Marino", "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia and Montenegro", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa", "South Georgia and the South Sandwich Islands", "Spain", "Sri Lanka", "Sudan", "Suriname", "Svalbard and Jan Mayen", "Swaziland", "Sweden", "Switzerland", "Syrian Arab Republic", "Taiwan", "Tajikistan", "Tanzania, United Republic of", "Thailand", "Timor-Leste", "Togo", "Tokelau", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan", "Turks and Caicos Islands", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United States of America", "United States Minor Outlying Islands", "Uruguay", "Uzbekistan", "Vanuatu", "Venezuela", "Vietnam", "Virgin Islands, British", "Virgin Islands, U.S.", "Wallis and Futuna", "Western Sahara", "Yemen", "Zambia", "Zimbabwe"];

    public void OnGet()
    {
        if (RegistrationDetails.OrganisationCountry != "United Kingdom")
        {
            AddressLine1 = RegistrationDetails.OrganisationAddressLine1;
            AddressLine2 = RegistrationDetails.OrganisationAddressLine2;
            TownOrCity = RegistrationDetails.OrganisationCityOrTown;
            Region = RegistrationDetails.OrganisationRegion;
            Postcode = RegistrationDetails.OrganisationPostcode;
            Country = RegistrationDetails.OrganisationCountry;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationAddressLine1 = AddressLine1 ?? RegistrationDetails.OrganisationAddressLine1;
        RegistrationDetails.OrganisationAddressLine2 = AddressLine2 ?? RegistrationDetails.OrganisationAddressLine2;
        RegistrationDetails.OrganisationCityOrTown = TownOrCity ?? RegistrationDetails.OrganisationCityOrTown;
        RegistrationDetails.OrganisationRegion = Region ?? RegistrationDetails.OrganisationRegion;
        RegistrationDetails.OrganisationPostcode = Postcode ?? RegistrationDetails.OrganisationPostcode;
        RegistrationDetails.OrganisationCountry = Country ?? RegistrationDetails.OrganisationCountry;

        session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RegistrationDetails.OrganisationType == OrganisationType.Buyer)
        {
            if (RedirectToSummary == true)
            {
                return RedirectToPage("OrganisationDetailsSummary");
            }
            else
            {
                return RedirectToPage("BuyerOrganisationType");
            }
        }
        else
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
    }
}