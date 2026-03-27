# FormFlow

**FormFlow** is a JSON-driven, multi-step form workflow engine built with [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/) on **.NET 10**. It lets you define entire multi-step forms — fields, validation, conditional visibility, and dynamic data sources — declaratively in JSON, with zero UI code changes.

---

## Features

- **100% JSON-driven forms** — add, remove, or reorder steps and fields by editing a single JSON file; no C# or Razor changes required.
- **Multi-step wizard UI** — visual progress stepper, Back/Next/Submit navigation, and per-step validation.
- **Rich field types** — `text`, `textarea`, `number`, `select`, `searchableselect`, `checkboxlist`, `radiobuttonlist`, `repeater`, `date`, and `fileupload`.
- **Searchable select** — type-to-filter dropdown with keyboard navigation; set `"allowCustomValue": true` to let users enter free-text values not present in the option list.
- **Custom field components** — register any Blazor component as a new field type with a single `AddCustomFieldType<T>()` call; no library code changes required.
- **Conditional visibility** — show or hide any field based on the value of another field using operators: `equals`, `notEquals`, `hasValue`, `isEmpty`, `contains`, `greaterThan`, `lessThan`, `in`, `notIn`.
- **Cascading / dependent selects** — `select` and `searchableselect` fields can depend on a parent field; the option list reloads automatically when the parent value changes.
- **Declarative validation** — attach any combination of rules to a field: `required`, `minLength`, `maxLength`, `min`, `max`, `regex`, `email`, `minItems`, `maxItems`, `minEntries`, `maxEntries`.
- **Pluggable data sources** — implement `IDataSourceProvider` and register it with DI; no other changes needed to wire up a new dropdown source.
- **Repeater fields** — dynamically add/remove structured entry groups, each with its own sub-fields, conditions, and validation.
- **Bootstrap 5 + Bootstrap Icons** — clean, responsive UI out of the box.

---

## Project Structure

```
FormFlow.sln
├── FormFlow.Core/              # Engine — no Blazor / UI dependency
│   ├── WorkflowEngine/
│   │   ├── Models/
│   │   │   ├── WorkflowDefinition.cs   # Root JSON model
│   │   │   ├── StepDefinition.cs       # One wizard step
│   │   │   ├── FieldDefinition.cs      # One form field
│   │   │   ├── ConditionRule.cs        # Visibility condition
│   │   │   ├── ValidationRule.cs       # Validation constraint
│   │   │   └── FormContext.cs          # Runtime value bag
│   │   ├── ConditionEvaluator.cs       # Pure static visibility evaluator
│   │   ├── DependencyGraph.cs          # BFS cascade reset for dependent fields
│   │   └── WorkflowLoader.cs           # Reads wwwroot/workflows/*.json
│   ├── Services/
│   │   ├── ValidationService.cs        # Stateless rule evaluator
│   │   └── OptionService.cs            # Resolves IDataSourceProvider by key
│   └── Providers/
│       ├── IDataSourceProvider.cs      # Contract for dynamic option lists
│       └── OptionItem.cs               # (Value, Label) pair
│
├── FormFlow.Components/        # Blazor Razor component library
│   ├── Components/
│   │   ├── DynamicForm.razor           # Renders a list of FieldDefinitions
│   │   ├── FieldComponentBase.cs       # Shared base for all field components
│   │   ├── TextField.razor
│   │   ├── TextareaField.razor
│   │   ├── NumberField.razor
│   │   ├── SelectField.razor
│   │   ├── SearchableSelectField.razor # Type-to-filter dropdown with optional custom values
│   │   ├── CheckboxListField.razor
│   │   ├── RadioButtonListField.razor
│   │   ├── RepeaterField.razor
│   │   ├── DatePickerField.razor       # Built-in date picker
│   │   └── FileUploadField.razor       # Built-in file upload
│   ├── FieldComponentRegistry.cs       # Singleton registry for custom field types
│   ├── FieldTypeRegistration.cs        # Descriptor: fieldType string → component Type
│   └── FormFlowComponentExtensions.cs  # AddFormFlowComponents() / AddCustomFieldType<T>()
│
└── FormFlow.App/               # Demo Blazor Server application
    ├── Pages/WorkflowPage.razor        # Main wizard page
    ├── Components/
    │   └── ColorPickerField.razor      # Example custom field type (color picker)
    ├── Providers/                      # Example IDataSourceProvider implementations
    │   ├── CountryProvider.cs
    │   ├── CityProvider.cs
    │   ├── PostalCodeProvider.cs
    │   ├── StreetProvider.cs
    │   ├── InterestsProvider.cs
    │   └── ...
    └── wwwroot/workflows/              # Place your workflow JSON files here
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run the demo

```bash
git clone https://github.com/alborzmgm/formflow.git
cd formflow
dotnet run --project FormFlow.App
```

Open your browser at `https://localhost:PORT/workflow`.

---

## Defining a Workflow (JSON)

Place a `.json` file in `FormFlow.App/wwwroot/workflows/`. A workflow has a key, a display name, and an ordered list of steps. Each step contains an ordered list of fields.

```jsonc
{
  "workflowKey": "contact-form",
  "name": "Contact Form",
  "steps": [
    {
      "stepKey": "personal",
      "title": "Personal Info",
      "description": "Tell us about yourself.",
      "icon": "bi-person-fill",
      "fields": [
        {
          "key": "FirstName",
          "label": "First Name",
          "fieldType": "text",
          "required": true,
          "order": 1
        },
        {
          "key": "Email",
          "label": "Email Address",
          "fieldType": "text",
          "order": 2,
          "validationRules": [
            { "type": "required" },
            { "type": "email", "message": "Please enter a valid email." }
          ]
        },
        {
          "key": "CountryId",
          "label": "Country",
          "fieldType": "select",
          "dataSource": "Countries",
          "required": true,
          "order": 3
        },
        {
          "key": "CityId",
          "label": "City",
          "fieldType": "searchableselect",
          "dataSource": "Cities",
          "dependsOn": "CountryId",
          "order": 4,
          "visibleWhen": [{ "field": "CountryId", "operator": "hasValue" }]
        }
      ]
    }
  ]
}
```

### Field Types

| `fieldType`        | Description                                                                                                    |
|--------------------|----------------------------------------------------------------------------------------------------------------|
| `text`             | Single-line text input                                                                                         |
| `textarea`         | Multi-line text area (`rows` property controls height, default `4`)                                           |
| `number`           | Numeric input                                                                                                  |
| `select`           | Dropdown populated by an `IDataSourceProvider` (`dataSource` key required)                                    |
| `searchableselect` | Filterable dropdown with keyboard navigation; set `"allowCustomValue": true` to accept free-text values       |
| `checkboxlist`     | Multi-select checkbox group, value stored as `List<string>`                                                    |
| `radiobuttonlist`  | Single-select radio button group                                                                               |
| `repeater`         | Dynamically add/remove structured entry groups with `subFields`                                                |
| `date`             | Date picker (`<input type="date">`), value stored as `"YYYY-MM-DD"` string                                   |
| `fileupload`       | File chooser; stores the selected filename as the field value                                                  |

### Searchable Select Field

The `searchableselect` type renders a text input that filters a dynamic option list as the user types. It supports the same `dataSource` and `dependsOn` properties as a regular `select` field, and adds two additional capabilities:

- **Keyboard navigation** — ArrowUp/Down moves through the filtered list; Enter selects the highlighted item; Escape dismisses the dropdown.
- **Custom values** — when `"allowCustomValue": true`, a **"Use `<typed text>`"** entry appears at the bottom of the dropdown when the user's input does not exactly match any option. Selecting it submits the typed text as the field value.

```jsonc
{
  "key": "PostalCode",
  "label": "Postal Code",
  "fieldType": "searchableselect",
  "dataSource": "PostalCodes",
  "dependsOn": "CityId",
  "placeholder": "Search or enter a postal code…",
  "allowCustomValue": true,
  "visibleWhen": [{ "field": "CityId", "operator": "hasValue" }],
  "validationRules": [
    { "type": "regex", "pattern": "^[A-Za-z0-9\\s\\-]{2,10}$", "message": "Postal code must be 2-10 alphanumeric characters." }
  ]
}
```

> **Tip:** `searchableselect` follows the same scalar validation rules as `text` and `select` (`required`, `minLength`, `maxLength`, `regex`, etc.).

### Conditional Visibility

`visibleWhen` accepts an array of condition rules. The field is shown only when **all** rules are satisfied (AND semantics). Use a single-element array for a simple condition:

```jsonc
"visibleWhen": [{ "field": "CountryId", "operator": "equals", "value": "US" }]
```

To require multiple conditions (field visible only when **all** are true):

```jsonc
"visibleWhen": [
  { "field": "CountryId", "operator": "equals", "value": "US" },
  { "field": "StateTaxId", "operator": "hasValue" }
]
```

To match against multiple values (OR semantics within a single rule), use the `in` operator with a `values` array:

```jsonc
"visibleWhen": [{ "field": "CountryId", "operator": "in", "values": ["US", "Germany"] }]
```

**Supported operators:**

| Operator       | Applies to               | Description                                 |
|----------------|--------------------------|---------------------------------------------|
| `equals`       | scalar                   | Value equals target (case-insensitive)      |
| `notEquals`    | scalar                   | Value does not equal target                 |
| `hasValue`     | scalar, list, repeater   | Field is non-empty                          |
| `isEmpty`      | scalar, list, repeater   | Field is empty                              |
| `contains`     | scalar, checkboxlist     | Scalar contains substring / list contains item |
| `greaterThan`  | scalar (numeric)         | Numeric value > target                      |
| `lessThan`     | scalar (numeric)         | Numeric value < target                      |
| `in`           | scalar                   | Value matches any entry in `values` list (case-insensitive) |
| `notIn`        | scalar                   | Value does not match any entry in `values` list (case-insensitive) |

### Validation Rules

Attach `validationRules` to any field. All rules accept an optional `message` property to override the default error text.

**Scalar fields (`text`, `textarea`, `number`, `select`, `searchableselect`):**

| `type`      | `value` / `pattern` | Description                   |
|-------------|----------------------|-------------------------------|
| `required`  | —                    | Field must not be empty       |
| `minLength` | integer              | Minimum character count       |
| `maxLength` | integer              | Maximum character count       |
| `min`       | decimal              | Minimum numeric value         |
| `max`       | decimal              | Maximum numeric value         |
| `regex`     | `pattern` string     | Value must match regex        |
| `email`     | —                    | Must be a valid email address |

**CheckboxList fields:**

| `type`     | `value`  | Description                        |
|------------|----------|------------------------------------|
| `required` | —        | At least one option must be chosen |
| `minItems` | integer  | Minimum number of selections       |
| `maxItems` | integer  | Maximum number of selections       |

**Repeater fields:**

| `type`        | `value`  | Description                        |
|---------------|----------|------------------------------------|
| `required`    | —        | At least one entry must be added   |
| `minEntries`  | integer  | Minimum number of entries          |
| `maxEntries`  | integer  | Maximum number of entries          |

### Repeater Fields

A `repeater` field renders a list of entry cards, each containing a complete sub-form defined by `subFields`. Entries support all field types, validation rules, and visibility conditions:

```jsonc
{
  "key": "Products",
  "label": "Products",
  "fieldType": "repeater",
  "itemLabel": "product",
  "itemLabelPlural": "products",
  "required": true,
  "order": 1,
  "subFields": [
    { "key": "Name",  "label": "Product Name", "fieldType": "text",   "required": true, "order": 1 },
    { "key": "Price", "label": "Price",         "fieldType": "number", "required": true, "order": 2 }
  ]
}
```

---

## Adding a Custom Field Component

The built-in field types cover common cases, but real-world forms often need specialized UI controls — color pickers, rich-text editors, signature pads, date-range selectors, star ratings, and so on. FormFlow lets you register any Blazor component as a first-class field type without touching the library.

### 1. Create the component

Create a Razor component in your application and inherit `FieldComponentBase`. This gives you the standard parameter set (`Field`, `Value`, `OnValueChanged`, `FormContext`, `OptionService`, `ValidationErrors`):

```razor
@namespace MyApp.Components
@inherits FormFlow.Components.FieldComponentBase

<div class="mb-3">
    <label class="form-label" for="@_id">@Field.Label</label>

    <input id="@_id" type="color"
           class="form-control form-control-color"
           value="@_color"
           @onchange="HandleChange" />
</div>

@code {
    private string _id    = string.Empty;
    private string _color = "#000000";

    protected override void OnInitialized()  => _id    = $"field-{Field.Key.ToLowerInvariant()}";
    protected override void OnParametersSet() => _color = Value is string s ? s : "#000000";

    private async Task HandleChange(ChangeEventArgs e) =>
        await OnValueChanged.InvokeAsync(e.Value?.ToString());
}
```

### 2. Register the component

Call `AddFormFlowComponents()` once as the baseline, then `AddCustomFieldType<T>()` for each custom type. The API mirrors `IDataSourceProvider` registration:

```csharp
// Program.cs
builder.Services.AddFormFlowComponents();                      // baseline registry
builder.Services.AddCustomFieldType<ColorPickerField>("color"); // custom type
```

### 3. Reference the type in JSON

```jsonc
{
  "key": "ThemeColor",
  "label": "Preferred Theme Color",
  "fieldType": "color"
}
```

That's all — `DynamicForm` checks the registry before its built-in switch, so the new type is live immediately.

> **Demo:** `FormFlow.App/Components/ColorPickerField.razor` is a fully working example. It is wired up in `Program.cs` and used in the *Preferences* step of the sample workflow.

---

## Adding a Custom Data Source

1. Implement `IDataSourceProvider` in `FormFlow.App/Providers/` (or your own project):

```csharp
public sealed class RegionProvider : IDataSourceProvider
{
    public string Key => "Regions";

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context)
    {
        // Use context.GetValue("ParentFieldKey") for dependent filtering.
        return Task.FromResult<IEnumerable<OptionItem>>(
        [
            new OptionItem("north", "North"),
            new OptionItem("south", "South"),
        ]);
    }
}
```

2. Register it in `Program.cs`:

```csharp
builder.Services.AddScoped<IDataSourceProvider, RegionProvider>();
```

3. Reference the key in your workflow JSON:

```jsonc
{ "key": "RegionId", "label": "Region", "fieldType": "searchableselect", "dataSource": "Regions" }
```

That's all — no other code changes are required.

---

## Architecture

FormFlow follows a clean three-layer architecture:

```
JSON Workflow File
       │
       ▼
  WorkflowLoader          (deserializes JSON → WorkflowDefinition)
       │
       ▼
  WorkflowViewer.razor    (self-contained component — accepts WorkflowFile="…" or Workflow="…";
  │                        loads the definition if needed, then orchestrates steps,
  │                        navigation, validation, and submission)
  │  ┌─ used as a page via:
  │  └─ WorkflowPage.razor   (@page "/workflow" — single-line host for WorkflowViewer)
       │
       ▼
  DynamicForm.razor        (renders FieldDefinitions via DynamicComponent)
       │
       ├── ConditionEvaluator   (pure static — decides field visibility)
       ├── ValidationService    (pure stateless — evaluates validation rules)
       ├── DependencyGraph      (BFS cascade reset on dependent field change)
       └── OptionService        (resolves IDataSourceProvider → OptionItem list)
```

**Key design principles:**
- **Models carry no logic** — `WorkflowDefinition`, `FieldDefinition`, `ConditionRule`, and `ValidationRule` are plain data classes.
- **Services are stateless** — `ValidationService` and `ConditionEvaluator` are pure functions / singletons with no side effects.
- **Open/Closed for extension** — adding a new built-in field type requires one new `.razor` file and one line in `DynamicForm.razor`'s `Resolve` switch. Application-level custom types need only a Razor component and one `AddCustomFieldType<T>()` call. Adding a new data source requires one new class and one DI registration.

---

## License

This project is licensed under the terms of the [LICENSE](LICENSE) file included in the repository. 