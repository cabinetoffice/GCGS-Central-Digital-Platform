namespace CO.CDP.RegisterOfCommercialTools.WebApi.Helpers;

public static class DateHelper
{
    public static DateTime AddWorkingDays(DateTime startDate, int workingDays)
    {
        return Enumerable.Range(1, workingDays + 10)
            .Select(day => startDate.AddDays(day))
            .Where(date => date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            .Take(workingDays)
            .LastOrDefault(startDate);
    }
}
