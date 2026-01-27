namespace CO.CDP.ApplicationRegistry.Core.Exceptions;

/// <summary>
/// Exception thrown when attempting to create an entity that already exists.
/// </summary>
public class DuplicateEntityException : ApplicationRegistryException
{
    public DuplicateEntityException(string entityName, string propertyName, object value)
        : base($"{entityName} with {propertyName} '{value}' already exists.")
    {
        EntityName = entityName;
        PropertyName = propertyName;
        Value = value;
    }

    public string EntityName { get; }
    public string PropertyName { get; }
    public object Value { get; }
}
