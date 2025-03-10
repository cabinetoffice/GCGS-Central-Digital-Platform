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
            {"AF", "Afghanistan"}, {"AL", "Albania"}, {"DZ", "Algeria"}, {"AD", "Andorra"}, {"AO", "Angola"},
            {"AG", "Antigua and Barbuda"}, {"AR", "Argentina"}, {"AM", "Armenia"}, {"AU", "Australia"}, {"AT", "Austria"},
            {"AZ", "Azerbaijan"}, {"BH", "Bahrain"}, {"BD", "Bangladesh"}, {"BB", "Barbados"}, {"BY", "Belarus"},
            {"BE", "Belgium"}, {"BZ", "Belize"}, {"BJ", "Benin"}, {"BT", "Bhutan"}, {"BO", "Bolivia"},
            {"BA", "Bosnia and Herzegovina"}, {"BW", "Botswana"}, {"BR", "Brazil"}, {"BN", "Brunei"}, {"BG", "Bulgaria"},
            {"BF", "Burkina Faso"}, {"BI", "Burundi"}, {"KH", "Cambodia"}, {"CM", "Cameroon"}, {"CA", "Canada"},
            {"CV", "Cape Verde"}, {"CF", "Central African Republic"}, {"TD", "Chad"}, {"CL", "Chile"}, {"CN", "China"},
            {"CO", "Colombia"}, {"KM", "Comoros"}, {"CG", "Congo"}, {"CD", "Congo (Democratic Republic)"}, {"CR", "Costa Rica"},
            {"CI", "Cote d'Ivoire"}, {"HR", "Croatia"}, {"CU", "Cuba"}, {"CY", "Cyprus"}, {"CZ", "Czechia"},
            {"DK", "Denmark"}, {"DJ", "Djibouti"}, {"DM", "Dominica"}, {"DO", "Dominican Republic"},
            {"TL", "East Timor"}, {"EC", "Ecuador"}, {"EG", "Egypt"},
            {"SV", "El Salvador"}, {"GQ", "Equatorial Guinea"}, {"ER", "Eritrea"}, {"EE", "Estonia"}, {"SZ", "Eswatini"},
            {"ET", "Ethiopia"}, {"FJ", "Fiji"}, {"FI", "Finland"}, {"FR", "France"}, {"GA", "Gabon"}, {"GE", "Georgia"},
            {"DE", "Germany"}, {"GH", "Ghana"}, {"GR", "Greece"}, {"GD", "Grenada"}, {"GT", "Guatemala"}, {"GN", "Guinea"},
            {"GW", "Guinea-Bissau"}, {"GY", "Guyana"}, {"HT", "Haiti"}, {"HN", "Honduras"}, {"HU", "Hungary"}, {"IS", "Iceland"},
            {"IN", "India"}, {"ID", "Indonesia"}, {"IR", "Iran"}, {"IQ", "Iraq"}, {"IE", "Ireland"}, {"IL", "Israel"},
            {"IT", "Italy"}, {"JM", "Jamaica"}, {"JP", "Japan"}, {"JO", "Jordan"}, {"KZ", "Kazakhstan"},
            {"KE", "Kenya"}, {"KI", "Kiribati"}, {"XK", "Kosovo"}, {"KW", "Kuwait"}, {"KG", "Kyrgyzstan"}, {"LA", "Laos"},
            {"LV", "Latvia"}, {"LB", "Lebanon"}, {"LS", "Lesotho"}, {"LR", "Liberia"}, {"LY", "Libya"}, {"LI", "Liechtenstein"},
            {"LT", "Lithuania"}, {"LU", "Luxembourg"}, {"MG", "Madagascar"}, {"MW", "Malawi"}, {"MY", "Malaysia"}, {"MV", "Maldives"},
            {"ML", "Mali"}, {"MT", "Malta"}, {"MH", "Marshall Islands"}, {"MR", "Mauritania"}, {"MU", "Mauritius"}, {"MX", "Mexico"},
            {"FM", "Federated States of Micronesia"}, {"MD", "Moldova"}, {"MC", "Monaco"}, {"MN", "Mongolia"}, {"ME", "Montenegro"},
            {"MA", "Morocco"}, {"MZ", "Mozambique"}, {"MM", "Myanmar (Burma)"}, {"NA", "Namibia"}, {"NR", "Nauru"}, {"NP", "Nepal"},
            {"NL", "Netherlands"}, {"NZ", "New Zealand"}, {"NI", "Nicaragua"}, {"NE", "Niger"}, {"NG", "Nigeria"}, {"KP", "North Korea"},
            {"MK", "North Macedonia"}, {"NO", "Norway"}, {"OM", "Oman"}, {"PK", "Pakistan"}, {"PW", "Palau"}, {"PA", "Panama"},
            {"PG", "Papua New Guinea"}, {"PY", "Paraguay"}, {"PE", "Peru"}, {"PH", "Philippines"}, {"PL", "Poland"}, {"PT", "Portugal"},
            {"QA", "Qatar"}, {"RO", "Romania"}, {"RU", "Russia"}, {"RW", "Rwanda"}, {"KN", "St Kitts and Nevis"}, {"LC", "St Lucia"},
            {"VC", "St Vincent"}, {"WS", "Samoa"}, {"SM", "San Marino"}, {"ST", "Sao Tome and Principe"}, {"SA", "Saudi Arabia"},
            {"SN", "Senegal"}, {"RS", "Serbia"}, {"SC", "Seychelles"}, {"SL", "Sierra Leone"}, {"SG", "Singapore"}, {"SK", "Slovakia"},
            {"SI", "Slovenia"}, {"SB", "Solomon Islands"}, {"SO", "Somalia"}, {"ZA", "South Africa"}, {"KR", "South Korea"},
            {"SS", "South Sudan"}, {"ES", "Spain"}, {"LK", "Sri Lanka"}, {"SD", "Sudan"}, {"SR", "Suriname"}, {"SE", "Sweden"},
            {"CH", "Switzerland"}, {"SY", "Syria"}, {"TJ", "Tajikistan"}, {"TZ", "Tanzania"}, {"TH", "Thailand"}, {"BS", "The Bahamas"},
            {"GM", "The Gambia"}, {"TG", "Togo"}, {"TO", "Tonga"}, {"TT", "Trinidad and Tobago"}, {"TN", "Tunisia"}, {"TR", "Turkey"},
            {"TM", "Turkmenistan"}, {"TV", "Tuvalu"}, {"UG", "Uganda"}, {"UA", "Ukraine"}, {"AE", "United Arab Emirates"},
            {"US", "United States"}, {"UY", "Uruguay"}, {"UZ", "Uzbekistan"}, {"VU", "Vanuatu"}, {"VA", "Vatican City"},
            {"VE", "Venezuela"}, {"VN", "Vietnam"}, {"YE", "Yemen"}, {"ZM", "Zambia"}, {"ZW", "Zimbabwe"}
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