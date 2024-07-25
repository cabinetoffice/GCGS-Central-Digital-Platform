using System.Reflection;
using System.Text;

namespace CO.CDP.EntityVerification.Persistence;

public class PersistenceBase
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        var type = this.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var name = property.Name;
            var value = property.GetValue(this) ?? "null";
            sb.AppendLine($"{name}: {value}");
        }

        return sb.ToString();
    }
}