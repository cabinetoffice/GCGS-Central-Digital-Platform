using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CO.CDP.Forms.WebApi.Model;

public record FormAnswer
{
    public required Guid Id { get; init; }
    public required FormQuestion Question { get; init; }
    [JsonIgnore]
    public FormAnswerSet? FormAnswerSet { get; init; }
    public bool? BoolValue { get; init; }
    public double? NumericValue { get; init; }
    public DateTime? DateValue { get; init; }
    public DateTime? StartValue { get; init; }
    public DateTime? EndValue { get; init; }
    public string? TextValue { get; init; }
    public string? OptionValue { get; init; }
}
