using FluentAssertions;
using CO.CDP.RegisterOfCommercialTools.WebApi.Helpers;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Helpers;

public class DateHelperTests
{
    [Theory]
    [InlineData("2025-01-06", 8, "2025-01-16")] // Monday + 8 working days = Thursday
    [InlineData("2025-01-10", 3, "2025-01-15")] // Friday + 3 working days = Wednesday (skips weekend)
    [InlineData("2025-01-11", 1, "2025-01-13")] // Saturday + 1 working day = Monday
    [InlineData("2025-01-06", 0, "2025-01-06")] // Zero days should return start date
    [InlineData("2025-01-06", 1, "2025-01-07")] // Monday + 1 working day = Tuesday
    [InlineData("2025-01-06", 15, "2025-01-27")] // Monday + 15 working days = Monday (crossing weekends)
    public void AddWorkingDays_ShouldCalculateCorrectly(string startDateStr, int workingDays, string expectedDateStr)
    {
        var startDate = DateTime.Parse(startDateStr);
        var expectedDate = DateTime.Parse(expectedDateStr);

        var result = DateHelper.AddWorkingDays(startDate, workingDays);

        result.Should().Be(expectedDate);
    }
}
