namespace CO.CDP.Functional;

/// <summary>
/// Represents an optional value that may or may not exist.
/// Provides a type-safe alternative to null references.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public readonly struct Option<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    private Option(T value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }

    /// <summary>
    /// Creates an Option with a value
    /// </summary>
    public static Option<T> Some(T value) => new(value, true);

    /// <summary>
    /// Creates an Option without a value
    /// </summary>
    public static Option<T> None => new(default!, false);

    /// <summary>
    /// Pattern matches on the Option, executing the appropriate function
    /// </summary>
    /// <param name="some">Function to execute if value exists</param>
    /// <param name="none">Function to execute if no value</param>
    public void Match(Action<T> some, Action none)
    {
        if (_hasValue)
            some(_value);
        else
            none();
    }

    /// <summary>
    /// Pattern matches on the Option, returning a result
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="some">Function to execute if value exists</param>
    /// <param name="none">Function to execute if no value</param>
    /// <returns>The result from the executed function</returns>
    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) =>
        _hasValue ? some(_value) : none();

    /// <summary>
    /// Gets the value if it exists, otherwise returns the default
    /// </summary>
    public T GetValueOrDefault(T defaultValue = default!) =>
        _hasValue ? _value : defaultValue;

    /// <summary>
    /// Indicates whether this Option has a value
    /// </summary>
    public bool HasValue => _hasValue;
}
