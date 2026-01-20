namespace CO.CDP.Functional;

/// <summary>
/// Represents a void return type in a functional way.
/// Similar to void but can be used as a type parameter.
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}

