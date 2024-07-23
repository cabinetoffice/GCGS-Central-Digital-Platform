using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.EntityFrameworkCore.Timestamps;

public static class PropertyExtensions
{
    public static PropertyBuilder<DateTimeOffset> HasTimestampDefault(this PropertyBuilder<DateTimeOffset> builder)
    {
        return builder.IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}