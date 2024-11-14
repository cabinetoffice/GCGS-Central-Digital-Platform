namespace CO.CDP.DataSharing.WebApi.Model;

public record FormQuestion
{
    public required FormQuestionType Type { get; init; }

    /// <example>"_Steel01"</example>
    public required string Name { get; init; }

    /// <example>"Upload your accounts"</example>
    public required string Title { get; init; }

    /// <example><![CDATA["<span>Central Government Only - UK</span><br /><p>For contracts which relate to projects/programmes (i) with a value of £10 million or more; or (ii) a value of less than £10 million where it is anticipated that the project will require in excess of 500 tonnes of steel; please describe the steel specific supply chain management systems, policies, standards and procedures you have in place to ensure robust supply chain management and compliance with relevant legislation.</p>Please provide details of previous similar projects where you have demonstrated a high level of competency and effectiveness in managing all supply chain members involved in steel supply or production to ensure a sustainable and resilient supply of steel."]]></example>
    public required string Text { get; init; }

    /// <example>false</example>
    public required bool IsRequired { get; init; }

    /// <example>"Steel"</example>
    public required string SectionName { get; init; }

    public List<FormQuestionOption>? Options { get; init; }

    public required int SortOrder { get; set; }
}