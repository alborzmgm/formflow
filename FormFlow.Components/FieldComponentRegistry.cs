namespace FormFlow.Components;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Singleton registry of custom <c>fieldType</c> → Blazor component mappings.
/// Populated at startup from all <see cref="FieldTypeRegistration"/> services registered in DI.
/// <para>
/// Register custom field types via
/// <see cref="FormFlowComponentExtensions.AddCustomFieldType{TComponent}"/>.
/// </para>
/// </summary>
public sealed class FieldComponentRegistry
{
    private readonly Dictionary<string, Type> _map;

    /// <param name="registrations">
    /// All <see cref="FieldTypeRegistration"/> instances resolved from the DI container.
    /// An empty enumerable is valid and produces an empty registry.
    /// </param>
    public FieldComponentRegistry(IEnumerable<FieldTypeRegistration> registrations)
    {
        _map = registrations.ToDictionary(
            r => r.FieldType,
            r => r.ComponentType,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tries to resolve a custom component type for the given <paramref name="fieldType"/>.
    /// Returns <see langword="false"/> when the type is not in the registry, meaning
    /// <see cref="DynamicForm"/> should fall back to its built-in switch.
    /// </summary>
    public bool TryResolve(string fieldType, [NotNullWhen(true)] out Type? componentType)
        => _map.TryGetValue(fieldType, out componentType);
}
