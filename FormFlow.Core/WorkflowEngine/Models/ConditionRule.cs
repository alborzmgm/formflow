namespace FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// Declarative visibility condition attached to a FieldDefinition.
/// Evaluated by ConditionEvaluator — zero logic lives here.
///
/// Supported operators (case-insensitive):
///   equals | notEquals | hasValue | isEmpty | contains | greaterThan | lessThan | in | notIn
///
/// Example JSON (single value):
///   "visibleWhen": { "field": "CountryId", "operator": "equals", "value": "US" }
///
/// Example JSON (multiple values — OR semantics):
///   "visibleWhen": { "field": "CountryId", "operator": "in", "values": ["US", "Germany"] }
/// </summary>
public sealed class ConditionRule
{
    /// <summary>Key of the controlling field whose value is tested.</summary>
    public string  Field    { get; set; } = string.Empty;

    /// <summary>Comparison operator. Defaults to "equals".</summary>
    public string  Operator { get; set; } = "equals";

    /// <summary>Expected value (required for equals / notEquals / contains / greaterThan / lessThan).</summary>
    public string? Value    { get; set; }

    /// <summary>
    /// Expected set of values for the <c>in</c> / <c>notIn</c> operators.
    /// The field is visible when its value matches any entry in this list (case-insensitive).
    /// </summary>
    public List<string>? Values { get; set; }
}
