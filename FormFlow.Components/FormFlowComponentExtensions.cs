namespace FormFlow.Components;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for registering FormFlow component services.
/// </summary>
public static class FormFlowComponentExtensions
{
    /// <summary>
    /// Registers the core FormFlow component services, including the
    /// <see cref="FieldComponentRegistry"/> used by <c>DynamicForm</c> to resolve
    /// both built-in and custom field types.
    /// <para>
    /// Call this once during application startup. When registering custom field types
    /// via <see cref="AddCustomFieldType{TComponent}"/>, this method does not need to
    /// be called separately — <c>AddCustomFieldType</c> also ensures the registry is
    /// registered.
    /// </para>
    /// </summary>
    public static IServiceCollection AddFormFlowComponents(this IServiceCollection services)
    {
        services.TryAddSingleton<FieldComponentRegistry>();
        return services;
    }

    /// <summary>
    /// Registers a custom field component for a given <paramref name="fieldType"/> string.
    /// <para>
    /// The component type <typeparamref name="TComponent"/> must inherit
    /// <see cref="FieldComponentBase"/> so that <c>DynamicForm</c> can pass the standard
    /// parameter dictionary to it via <c>DynamicComponent</c>.
    /// </para>
    /// <para>
    /// Usage (mirrors the <c>IDataSourceProvider</c> pattern):
    /// <code>
    /// builder.Services.AddCustomFieldType&lt;DatePickerField&gt;("date");
    /// </code>
    /// Then reference the type in workflow JSON:
    /// <code>
    /// { "key": "BirthDate", "label": "Date of Birth", "fieldType": "date" }
    /// </code>
    /// </para>
    /// </summary>
    /// <typeparam name="TComponent">
    /// A Blazor component that inherits <see cref="FieldComponentBase"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="fieldType">
    /// The <c>fieldType</c> string used in workflow JSON to reference this component.
    /// Case-insensitive at resolution time.
    /// </param>
    public static IServiceCollection AddCustomFieldType<TComponent>(
        this IServiceCollection services,
        string fieldType)
        where TComponent : FieldComponentBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldType);

        services.TryAddSingleton<FieldComponentRegistry>();
        services.AddSingleton<FieldTypeRegistration>(
            new FieldTypeRegistration(fieldType, typeof(TComponent)));

        return services;
    }
}
