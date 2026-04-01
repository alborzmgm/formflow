using System.Text.Json;

namespace FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// Runtime value bag for all field values in the current workflow session.
/// Passed to IDataSourceProvider so dependent providers can filter by parent values.
/// </summary>
public sealed class FormContext
{
    private readonly Dictionary<string, object?> _values =
        new(StringComparer.OrdinalIgnoreCase);

    public object? GetValue(string key) =>
        _values.TryGetValue(key, out var val) ? val : null;

    public void SetValue(string key, object? value) =>
        _values[key] = value;

    public void Reset(string key) =>
        _values.Remove(key);

    /// <summary>
    /// Returns true when the key exists and has a meaningful value.
    /// Handles scalar values, List&lt;string&gt; (CheckboxListField), and
    /// List&lt;Dictionary&lt;string, object?&gt;&gt; (RepeaterField).
    /// </summary>
    public bool HasValue(string key)
    {
        if (!_values.TryGetValue(key, out var val) || val is null)
            return false;

        // Checkbox list stores its selections as List<string>
        if (val is List<string> list)
            return list.Count > 0;

        // Repeater field stores entries as List<Dictionary<string, object?>>
        if (val is List<Dictionary<string, object?>> entries)
            return entries.Count > 0;

        return val.ToString() is { Length: > 0 };
    }

    public IReadOnlyDictionary<string, object?> AllValues => _values;

    // ── Snapshot / Restore ──────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="FormFlowState"/> snapshot of the current context so it can
    /// be persisted and later restored via <see cref="LoadValues"/>.
    /// </summary>
    public FormFlowState Snapshot(string workflowKey, int currentStepIndex) =>
        new()
        {
            WorkflowKey      = workflowKey,
            CurrentStepIndex = currentStepIndex,
            Values           = new Dictionary<string, object?>(_values, StringComparer.OrdinalIgnoreCase),
        };

    /// <summary>
    /// Populates this context from a previously saved values dictionary, replacing any
    /// existing values.  Handles <see cref="JsonElement"/> values that appear when a
    /// <see cref="FormFlowState"/> is deserialized from JSON, converting them back to
    /// the expected runtime types (<c>string</c>, <c>List&lt;string&gt;</c>, or
    /// <c>List&lt;Dictionary&lt;string, object?&gt;&gt;</c>).
    /// </summary>
    public void LoadValues(IReadOnlyDictionary<string, object?> values)
    {
        _values.Clear();
        foreach (var (key, value) in values)
            _values[key] = ConvertValue(value);
    }

    // ── JSON element conversion helpers ────────────────────────────────────

    private static object? ConvertValue(object? value) =>
        value is JsonElement element ? ConvertJsonElement(element) : value;

    private static object? ConvertJsonElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null    => null,
        JsonValueKind.String  => element.GetString(),
        // Numbers are stored as strings in scalar fields (e.g. NumberField).
        JsonValueKind.Number  => element.GetRawText(),
        JsonValueKind.True    => "true",
        JsonValueKind.False   => "false",
        JsonValueKind.Array   => ConvertJsonArray(element),
        JsonValueKind.Object  => ConvertJsonObject(element),
        _                     => null,
    };

    private static object ConvertJsonArray(JsonElement element)
    {
        var items = element.EnumerateArray().ToList();

        // Repeater field: array of objects → List<Dictionary<string, object?>>
        // FormFlow arrays are always homogeneous (all strings or all objects),
        // so checking the first element is sufficient for type detection.
        if (items.Count > 0 && items[0].ValueKind == JsonValueKind.Object)
        {
            var entries = new List<Dictionary<string, object?>>(items.Count);
            foreach (var item in items)
            {
                var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in item.EnumerateObject())
                    dict[prop.Name] = ConvertJsonElement(prop.Value);
                entries.Add(dict);
            }
            return entries;
        }

        // Checkbox list: array of strings → List<string>
        return items.Select(e => e.ValueKind == JsonValueKind.String
            ? e.GetString() ?? ""
            : e.GetRawText()).ToList();
    }

    private static Dictionary<string, object?> ConvertJsonObject(JsonElement element)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in element.EnumerateObject())
            dict[prop.Name] = ConvertJsonElement(prop.Value);
        return dict;
    }
}
