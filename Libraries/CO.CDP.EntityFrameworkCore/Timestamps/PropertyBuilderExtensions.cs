using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.EntityFrameworkCore.Timestamps;

public static class PropertyExtensions
{
    public static PropertyBuilder<DateTime> HasTimestampDefault(this PropertyBuilder<DateTime> builder)
    {
        return builder.IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}