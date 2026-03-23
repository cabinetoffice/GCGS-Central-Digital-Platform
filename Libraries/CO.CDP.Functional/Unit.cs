namespace CO.CDP.Functional;

/// <summary>Represents the absence of a meaningful value — used as the success type for void-returning operations.</summary>
public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = default;
    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public override string ToString() => "()";
}
