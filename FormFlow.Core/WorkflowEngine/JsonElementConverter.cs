using System.Text.Json;

namespace FormFlow.Core.WorkflowEngine;

/// <summary>
/// Converts <see cref="JsonElement"/> values produced by <c>System.Text.Json</c> when
/// deserializing <c>Dictionary&lt;string, object?&gt;</c> back to the runtime types used
/// by <see cref="Models.FormContext"/>:
/// <c>string</c>, <c>List&lt;string&gt;</c>, or <c>List&lt;Dictionary&lt;string, object?&gt;&gt;</c>.
/// </summary>
internal static class JsonElementConverter
{
    internal static object? Convert(object? value) =>
        value is JsonElement element ? ConvertElement(element) : value;

    private static object? ConvertElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null    => null,
        JsonValueKind.String  => element.GetString(),
        // Numbers are stored as strings in scalar fields (e.g. NumberField).
        JsonValueKind.Number  => element.GetRawText(),
        JsonValueKind.True    => "true",
        JsonValueKind.False   => "false",
        JsonValueKind.Array   => ConvertArray(element),
        JsonValueKind.Object  => ConvertObject(element),
        _                     => null,
    };

    private static object ConvertArray(JsonElement element)
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
                    dict[prop.Name] = ConvertElement(prop.Value);
                entries.Add(dict);
            }
            return entries;
        }

        // Checkbox list: array of strings → List<string>
        // GetRawText() is a defensive fallback; in practice all checkbox values are strings.
        return items.Select(e => e.ValueKind == JsonValueKind.String
            ? e.GetString() ?? ""
            : e.GetRawText()).ToList();
    }

    private static Dictionary<string, object?> ConvertObject(JsonElement element)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in element.EnumerateObject())
            dict[prop.Name] = ConvertElement(prop.Value);
        return dict;
    }
}
