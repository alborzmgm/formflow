namespace FormFlow.Components;

/// <summary>
/// Pairs a JSON <c>fieldType</c> string with the Blazor component <see cref="Type"/> that
/// should render it. Instances are registered in the DI container and collected by
/// <see cref="FieldComponentRegistry"/> at startup.
/// </summary>
public sealed class FieldTypeRegistration
{
    /// <summary>The value that must appear in the workflow JSON <c>fieldType</c> property.</summary>
    public string FieldType { get; }

    /// <summary>The Blazor component type to render for this field type. Must inherit <see cref="FieldComponentBase"/>.</summary>
    public Type ComponentType { get; }

    internal FieldTypeRegistration(string fieldType, Type componentType)
    {
        FieldType = fieldType;
        ComponentType = componentType;
    }
}
