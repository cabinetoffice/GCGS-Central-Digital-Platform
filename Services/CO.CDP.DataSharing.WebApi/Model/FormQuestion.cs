namespace CO.CDP.DataSharing.WebApi.Model;

internal record FormQuestion
{
    public FormQuestionType? Type { get; init; }

    /// <example>"_Steel01"</example>
    public string? Name { get; init; }

    /// <example><![CDATA["<span style=\"font-weight: bold;\">Central Government Only - UK</span><span style=\"font-size:16.000000000000004px\"><br /></span><p><span style=\"font-size:13.333299999999998px\">For contracts which relate to projects/programmes (i) with a value of £10 million or more; or (ii) a value of less than £10 million where it is anticipated that the project will require in excess of 500 tonnes of steel; please describe the steel specific supply chain management systems, policies, standards and procedures you have in place to ensure robust supply chain management and compliance with relevant legislation.</span><span style=\"font-size:16.000000000000004px\"></span></p><span style=\"font-size:13.333299999999998px\">Please provide details of previous similar projects where you have demonstrated a high level of competency and effectiveness in managing all supply chain members involved in steel supply or production to ensure a sustainable and resilient supply of steel.</span>"]]></example>
    public string? Text { get; init; }

    /// <example>false</example>
    public bool IsRequired { get; init; }

    /// <example>"Steel"</example>
    public string? SectionName { get; init; }

    public List<FormQuestionOption> Options { get; init; } = new();
}