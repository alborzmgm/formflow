namespace FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// A serializable snapshot of workflow progress, capturing the currently active step and
/// all collected field values.  Consumers can persist this to any storage back-end
/// (e.g. browser localStorage, a database, session state, or a URL query string) and
/// pass it back via <see cref="FormFlow.Components.WorkflowViewer.InitialState"/> to let
/// users resume an in-progress session exactly where they left off.
/// </summary>
/// <remarks>
/// <para><b>Value types inside <see cref="Values"/>:</b></para>
/// <list type="bullet">
///   <item><c>string</c> or <c>null</c> — scalar fields (text, number, select, date, file, radio)</item>
///   <item><c>List&lt;string&gt;</c>                           — checkbox-list fields</item>
///   <item><c>List&lt;Dictionary&lt;string, object?&gt;&gt;</c> — repeater fields</item>
/// </list>
/// <para>
/// When a <see cref="FormFlowState"/> is obtained from <see cref="FormContext.Snapshot"/>
/// it contains strongly-typed values.  After a JSON round-trip the values will be
/// <see cref="System.Text.Json.JsonElement"/> instances; call
/// <see cref="FormContext.LoadValues"/> (which <c>WorkflowViewer</c> does automatically)
/// to convert them back to the expected runtime types.
/// </para>
/// </remarks>
public sealed class FormFlowState
{
    /// <summary>
    /// The <c>workflowKey</c> of the workflow this snapshot belongs to.
    /// Used by <c>WorkflowViewer</c> to verify the state matches the loaded workflow before
    /// restoring it.
    /// </summary>
    public string WorkflowKey { get; init; } = "";

    /// <summary>
    /// Zero-based index of the step that was active when the snapshot was taken.
    /// <c>WorkflowViewer</c> clamps this to the valid step range on restore.
    /// </summary>
    public int CurrentStepIndex { get; init; }

    /// <summary>
    /// All field values collected so far, keyed by field key (case-insensitive).
    /// </summary>
    public IReadOnlyDictionary<string, object?> Values { get; init; } =
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
}
