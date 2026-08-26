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
            "Afghan", "Albanian", "Algerian", "American", "Andorran", "Angolan", "Anguillan", "Antiguans",
            "Argentine", "Argentinean", "Armenian", "Australian", "Austrian", "Azerbaijani", "Bahamian", "Bahraini",
            "Bangladeshi", "Barbadian", "Barbudans", "Batswana", "Belarusian", "Belgian", "Belizean", "Beninese",
            "Bermudian", "Bhutanese", "Bolivian", "Bosnian", "Botswanan", "Brazilian", "British", "British Virgin Islander",
            "Bruneian", "Bulgarian", "Burkinabe", "Burkinan", "Burmese", "Burundian", "Cambodian", "Cameroonian",
            "Canadian", "Cape Verdean", "Cayman Islander", "Central African", "Chadian", "Chilean", "Chinese", "Citizen of Antigua and Barbuda",
            "Citizen of Bosnia and Herzegovina", "Citizen of Guinea-Bissau", "Citizen of Kiribati", "Citizen of Seychelles", "Citizen of Vanuatu", "Citizen of the Dominican Republic", "Colombian", "Comoran",
            "Congolese", "Congolese (Congo)", "Congolese (DRC)", "Cook Islander", "Costa Rican", "Croatian", "Cuban", "Cymraes",
            "Cymro", "Cypriot", "Czech", "Danish", "Djibouti", "Djiboutian", "Dominican", "Dutch",
            "East Timorese", "Ecuadorean", "Egyptian", "Emirati", "Emirian", "English", "Equatorial Guinean", "Eritrean",
            "Estonian", "Ethiopian", "Faroese", "Fijian", "Filipino", "Finnish", "French", "Gabonese",
            "Gambian", "Georgian", "German", "Ghanaian", "Gibraltarian", "Greek", "Greenlandic", "Grenadian",
            "Guamanian", "Guatemalan", "Guinea-Bissauan", "Guinean", "Guyanese", "Haitian", "Herzegovinian", "Honduran",
            "Hong Konger", "Hungarian", "I-Kiribati", "Icelander", "Icelandic", "Indian", "Indonesian", "Iranian",
            "Iraqi", "Irish", "Israeli", "Italian", "Ivorian", "Jamaican", "Japanese", "Jordanian",
            "Kazakh", "Kazakhstani", "Kenyan", "Kittian and Nevisian", "Kittitian", "Kosovan", "Kuwaiti", "Kyrgyz",
            "Lao", "Laotian", "Latvian", "Lebanese", "Liberian", "Libyan", "Liechtenstein citizen", "Liechtensteiner",
            "Lithuanian", "Luxembourger", "Macanese", "Macedonian", "Malagasy", "Malawian", "Malaysian", "Maldivan",
            "Maldivian", "Malian", "Maltese", "Marshallese", "Martiniquais", "Mauritanian", "Mauritian", "Mexican",
            "Micronesian", "Moldovan", "Monacan", "Monegasque", "Mongolian", "Montenegrin", "Montserratian", "Moroccan",
            "Mosotho", "Motswana", "Mozambican", "Namibian", "Nauruan", "Nepalese", "New Zealander", "Nicaraguan",
            "Nigerian", "Nigerien", "Niuean", "North Korean", "Northern Irish", "Norwegian", "Omani", "Pakistani",
            "Palauan", "Palestinian", "Panamanian", "Papua New Guinean", "Paraguayan", "Peruvian", "Pitcairn Islander", "Polish",
            "Portuguese", "Prydeinig", "Puerto Rican", "Qatari", "Romanian", "Russian", "Rwandan", "Saint Lucian",
            "Salvadoran", "Salvadorean", "Sammarinese", "Samoan", "San Marinese", "Sao Tomean", "Saudi", "Saudi Arabian",
            "Scottish", "Senegalese", "Serbian", "Seychellois", "Sierra Leonean", "Singaporean", "Slovak", "Slovakian",
            "Slovenian", "Solomon Islander", "Somali", "South African", "South Korean", "South Sudanese", "Spanish", "Sri Lankan",
            "St Helenian", "St Lucian", "Stateless", "Sudanese", "Surinamer", "Surinamese", "Swazi", "Swedish",
            "Swiss", "Syrian", "Taiwanese", "Tajik", "Tanzanian", "Thai", "Togolese", "Tongan",
            "Trinidadian", "Trinidadian or Tobagonian", "Tristanian", "Tunisian", "Turkish", "Turkmen", "Turks and Caicos Islander", "Tuvaluan",
            "Ugandan", "Ukrainian", "Uruguayan", "Uzbek", "Uzbekistani", "Vatican citizen", "Venezuelan", "Vietnamese",
            "Vincentian", "Wallisian", "Welsh", "Yemeni", "Yemenite", "Zambian", "Zimbabwean"
        };

        Country.Nationalities.Should().BeEquivalentTo(expectedNationalities);
        Country.Nationalities.Should().BeInAscendingOrder();
        Country.Nationalities.Should().OnlyHaveUniqueItems();
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
