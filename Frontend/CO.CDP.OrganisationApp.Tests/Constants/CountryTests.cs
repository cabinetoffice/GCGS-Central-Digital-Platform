using Amazon.Runtime.Internal.Transform;
using CO.CDP.OrganisationApp.Constants;
using FluentAssertions;

public class CountryTests
{
    [Fact]
    public void UnitedKingdom_Constant_IsCorrect()
    {
        Country.UnitedKingdom.Should().Be("United Kingdom");
    }

    [Fact]
    public void UKCountryCode_Constant_IsCorrect()
    {
        Country.UKCountryCode.Should().Be("GB");
    }

    [Fact]
    public void Nationalities_ContainsExpectedValues()
    {
        var expectedNationalities = new List<string>
        {
            "Afghan", "Albanian", "Algerian", "American", "Andorran", "Angolan", "Antiguans", "Argentinean", "Armenian", "Australian",
            "Austrian", "Azerbaijani", "Bahamian", "Bahraini", "Bangladeshi", "Barbadian", "Barbudans", "Batswana", "Belarusian", "Belgian",
            "Belizean", "Beninese", "Bhutanese", "Bolivian", "Bosnian", "Brazilian", "British", "Bruneian", "Bulgarian", "Burkinabe",
            "Burmese", "Burundian", "Cambodian", "Cameroonian", "Canadian", "Cape Verdean", "Central African", "Chadian", "Chilean", "Chinese",
            "Colombian", "Comoran", "Congolese", "Costa Rican", "Croatian", "Cuban", "Cypriot", "Czech", "Danish", "Djibouti", "Dominican",
            "Dutch", "East Timorese", "Ecuadorean", "Egyptian", "Emirian", "Equatorial Guinean", "Eritrean", "Estonian", "Ethiopian", "Fijian",
            "Filipino", "Finnish", "French", "Gabonese", "Gambian", "Georgian", "German", "Ghanaian", "Greek", "Grenadian", "Guatemalan",
            "Guinea-Bissauan", "Guinean", "Guyanese", "Haitian", "Herzegovinian", "Honduran", "Hungarian", "I-Kiribati", "Icelander", "Indian",
            "Indonesian", "Iranian", "Iraqi", "Irish", "Israeli", "Italian", "Ivorian", "Jamaican", "Japanese", "Jordanian", "Kazakhstani",
            "Kenyan", "Kittian and Nevisian", "Kuwaiti", "Kyrgyz", "Laotian", "Latvian", "Lebanese", "Liberian", "Libyan", "Liechtensteiner",
            "Lithuanian", "Luxembourger", "Macedonian", "Malagasy", "Malawian", "Malaysian", "Maldivan", "Malian", "Maltese", "Marshallese",
            "Mauritanian", "Mauritian", "Mexican", "Micronesian", "Moldovan", "Monacan", "Mongolian", "Moroccan", "Mosotho", "Motswana",
            "Mozambican", "Namibian", "Nauruan", "Nepalese", "New Zealander", "Nicaraguan", "Nigerian", "Nigerien", "North Korean", "Northern Irish",
            "Norwegian", "Omani", "Pakistani", "Palauan", "Panamanian", "Papua New Guinean", "Paraguayan", "Peruvian", "Polish", "Portuguese",
            "Qatari", "Romanian", "Russian", "Rwandan", "Saint Lucian", "Salvadoran", "Samoan", "San Marinese", "Sao Tomean", "Saudi", "Scottish",
            "Senegalese", "Serbian", "Seychellois", "Sierra Leonean", "Singaporean", "Slovakian", "Slovenian", "Solomon Islander", "Somali",
            "South African", "South Korean", "Spanish", "Sri Lankan", "Sudanese", "Surinamer", "Swazi", "Swedish", "Swiss", "Syrian", "Taiwanese",
            "Tajik", "Tanzanian", "Thai", "Togolese", "Tongan", "Trinidadian or Tobagonian", "Tunisian", "Turkish", "Tuvaluan", "Ugandan",
            "Ukrainian", "Uruguayan", "Uzbekistani", "Venezuelan", "Vietnamese", "Welsh", "Yemenite", "Zambian", "Zimbabwean"
        };

        Country.Nationalities.Should().BeEquivalentTo(expectedNationalities);
    }

    [Fact]
    public void NonUKCountries_ContainsExpectedValues()
    {
        var expectedNonUKCountries = new Dictionary<string, string>
        {
            {"AF", "Afghanistan"}, {"AX", "Aland Islands"}, {"AL", "Albania"}, {"DZ", "Algeria"}, {"AS", "American Samoa"},
            {"AD", "Andorra"}, {"AO", "Angola"}, {"AI", "Anguilla"}, {"AQ", "Antarctica"}, {"AG", "Antigua and Barbuda"},
            {"AR", "Argentina"}, {"AM", "Armenia"}, {"AW", "Aruba"}, {"AU", "Australia"}, {"AT", "Austria"},
            {"AZ", "Azerbaijan"}, {"BH", "Bahrain"}, {"BD", "Bangladesh"}, {"BB", "Barbados"}, {"BY", "Belarus"},
            {"BE", "Belgium"}, {"BZ", "Belize"}, {"BJ", "Benin"}, {"BM", "Bermuda"}, {"BT", "Bhutan"},
            {"BO", "Bolivia"}, {"BQ", "Bonaire"}, {"BA", "Bosnia and Herzegovina"}, {"BW", "Botswana"}, {"BV", "Bouvet Island"},
            {"BR", "Brazil"}, {"IO", "British Indian Ocean Territory"}, {"VG", "British Virgin Islands"}, { "BN", "Brunei"}, {"BG", "Bulgaria"}, {"BF", "Burkina Faso"},
            {"BI", "Burundi"}, {"KH", "Cambodia"}, {"CM", "Cameroon"}, {"CA", "Canada"}, {"KY", "Cayman Islands"},
            {"CV", "Cape Verde"}, {"CF", "Central African Republic"}, {"TD", "Chad"}, {"CX", "Christmas Island"}, {"CL", "Chile"},
            {"CN", "China"}, {"CP", "Clipperton"}, {"CC", "Cocos Islands"}, {"CO", "Colombia"}, {"KM", "Comoros"},
            {"CK", "Cook Islands"}, {"CG", "Congo"}, {"CD", "Congo (Democratic Republic)"}, {"CR", "Costa Rica"},
            {"CI", "Cote d'Ivoire"}, {"HR", "Croatia"}, {"CU", "Cuba"}, {"CW", "Curacao"}, {"CY", "Cyprus"}, {"CZ", "Czechia"},
            {"DK", "Denmark"}, {"DJ", "Djibouti"}, {"DM", "Dominica"}, {"DO", "Dominican Republic"},
            {"TL", "East Timor"}, {"EC", "Ecuador"}, {"EG", "Egypt"},
            {"SV", "El Salvador"}, {"GQ", "Equatorial Guinea"}, {"ER", "Eritrea"}, {"EE", "Estonia"}, {"SZ", "Eswatini"},
            {"ET", "Ethiopia"}, {"FK", "Falkland Islands"}, {"FO", "Faroe Islands"}, {"FJ", "Fiji"}, {"FI", "Finland"},
            {"FR", "France"}, {"GF", "French Guiana"}, {"PF", "French Polynesia"}, {"TF", "French Southern Territories"}, {"GA", "Gabon"},
            {"GE", "Georgia"}, {"DE", "Germany"}, {"GH", "Ghana"}, {"GI", "Gibraltar"}, {"GR", "Greece"}, {"GL", "Greenland"},
            {"GD", "Grenada"}, {"GP", "Guadeloupe"}, {"GT", "Guatemala"}, {"GG", "Guernsey"}, {"GN", "Guinea"},
            {"GW", "Guinea-Bissau"}, {"GU", "Guam"}, {"GY", "Guyana"}, {"HT", "Haiti"}, {"HM", "Heard Island and McDonald Islands"},
            {"HN", "Honduras"}, {"HK", "Hong Kong"}, {"HU", "Hungary"}, {"IS", "Iceland"},
            {"IN", "India"}, {"ID", "Indonesia"}, {"IR", "Iran"}, {"IQ", "Iraq"}, {"IE", "Ireland"}, {"IM", "Isle of Man"}, {"IL", "Israel"},
            {"IT", "Italy"}, {"JM", "Jamaica"}, {"JP", "Japan"}, {"JE", "Jersey"}, {"JO", "Jordan"}, {"KZ", "Kazakhstan"},
            {"KE", "Kenya"}, {"KI", "Kiribati"}, {"XK", "Kosovo"}, {"KW", "Kuwait"}, {"KG", "Kyrgyzstan"}, {"LA", "Laos"},
            {"LV", "Latvia"}, {"LB", "Lebanon"}, {"LS", "Lesotho"}, {"LR", "Liberia"}, {"LY", "Libya"}, {"LI", "Liechtenstein"},
            {"LT", "Lithuania"}, {"LU", "Luxembourg"}, {"MO", "Macau"}, {"MG", "Madagascar"}, {"MW", "Malawi"}, {"MY", "Malaysia"}, {"MV", "Maldives"},
            {"ML", "Mali"}, {"MT", "Malta"}, {"MH", "Marshall Islands"}, {"MQ", "Martinique"}, {"MR", "Mauritania"}, {"MU", "Mauritius"}, {"YT", "Mayotte"},
            {"MX", "Mexico"}, {"FM", "Federated States of Micronesia"}, {"MD", "Moldova"}, {"MC", "Monaco"}, {"MN", "Mongolia"}, {"ME", "Montenegro"},
            {"MS", "Montserrat"}, {"MA", "Morocco"}, {"MZ", "Mozambique"}, {"MM", "Myanmar (Burma)"}, {"NA", "Namibia"}, {"NR", "Nauru"}, {"NP", "Nepal"},
            {"NL", "Netherlands"}, {"NC", "New Caledonia"}, {"NZ", "New Zealand"}, {"NI", "Nicaragua"}, {"NE", "Niger"}, {"NG", "Nigeria"},
            {"NU", "Niue"}, {"NF", "Norfolk Island"}, {"KP", "North Korea"}, {"MK", "North Macedonia"}, {"MP", "Northern Mariana Islands"},
            {"NO", "Norway"}, {"OM", "Oman"}, {"PK", "Pakistan"}, {"PW", "Palau"}, {"PS", "Palestine"}, {"PA", "Panama"},
            {"PG", "Papua New Guinea"}, {"PY", "Paraguay"}, {"PE", "Peru"}, {"PH", "Philippines"}, {"PN", "Pitcairn Islands"}, {"PL", "Poland"}, {"PT", "Portugal"},
            {"PR", "Puerto Rico"}, {"QA", "Qatar"}, {"RE", "Reunion"}, {"RO", "Romania"}, {"RU", "Russia"}, {"RW", "Rwanda"},
            {"BL", "Saint Barthelemy"}, {"SH", "Saint Helena"}, {"KN", "St Kitts and Nevis"}, {"LC", "St Lucia"}, {"MF", "Saint Martin"}, {"PM", "Saint Pierre and Miquelon"},
            {"VC", "St Vincent"}, {"WS", "Samoa"}, {"SM", "San Marino"}, {"ST", "Sao Tome and Principe"}, {"SA", "Saudi Arabia"},
            {"SN", "Senegal"}, {"RS", "Serbia"}, {"SC", "Seychelles"}, {"SL", "Sierra Leone"}, {"SX", "Sint Maarten"}, {"SG", "Singapore"},
            {"SK", "Slovakia"}, {"SI", "Slovenia"}, {"SB", "Solomon Islands"}, {"SO", "Somalia"}, {"ZA", "South Africa"},
            {"GS", "South Georgia and the South Sandwich Islands"}, {"KR", "South Korea"}, {"SS", "South Sudan"}, {"ES", "Spain"}, {"LK", "Sri Lanka"}, {"SD", "Sudan"},
            {"SR", "Suriname"}, {"SJ", "Svalbard and Jan Mayen"}, {"SE", "Sweden"}, {"CH", "Switzerland"}, {"SY", "Syria"}, {"TW", "Taiwan"},
            {"TJ", "Tajikistan"}, {"TZ", "Tanzania"}, {"BS", "The Bahamas"}, {"GM", "The Gambia"}, {"TH", "Thailand"}, {"TG", "Togo"},
            {"TK", "Tokelau"}, {"TO", "Tonga"}, {"TT", "Trinidad and Tobago"}, {"TN", "Tunisia"}, {"TR", "Turkey"}, {"TM", "Turkmenistan"},
            {"TC", "Turks and Caicos Islands"}, {"TV", "Tuvalu"}, {"UG", "Uganda"}, {"UA", "Ukraine"}, {"AE", "United Arab Emirates"},
            {"US", "United States"}, {"UM", "United States Minor Outlying Islands"}, {"VI", "United States Virgin Islands"}, {"UY", "Uruguay"}, {"UZ", "Uzbekistan"},
            {"VU", "Vanuatu"}, {"VA", "Vatican City"}, {"VE", "Venezuela"}, {"VN", "Vietnam"},
            {"WF", "Wallis and Futuna"}, {"EH", "Western Sahara"}, {"YE", "Yemen"}, {"ZM", "Zambia"}, {"ZW", "Zimbabwe"}
        };

        Country.NonUKCountries.Should().BeEquivalentTo(expectedNonUKCountries);
    }

    [Fact]
    public void UKCountries_ContainsExpectedValues()
    {
        var expectedUKCountries = new Dictionary<string, string>
        {
            {"GB-ENG", "England"},
            {"GB-NIR", "Northern Ireland"},
            {"GB-SCT", "Scotland"},
            {"GB-WLS", "Wales"}
        };

        Country.UKCountries.Should().BeEquivalentTo(expectedUKCountries);
    }

    [Fact]
    public void GetAllCountries_ReturnsCombinedAndSortedDictionary()
    {
        var allCountries = Country.GetAllCountries();
        var expectedCountries = Country.NonUKCountries
            .Concat(Country.UKCountries)
            .OrderBy(pair => pair.Value)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        allCountries.Should().BeEquivalentTo(expectedCountries);
        allCountries.Should().BeInAscendingOrder(pair => pair.Value);
    }
}