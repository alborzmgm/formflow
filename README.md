# FormFlow

**FormFlow** is a JSON-driven, multi-step form workflow engine built with [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/) on **.NET 10**. It lets you define entire multi-step forms — fields, validation, conditional visibility, and dynamic data sources — declaratively in JSON, with zero UI code changes.

---

## Features

- **100% JSON-driven forms** — add, remove, or reorder steps and fields by editing a single JSON file; no C# or Razor changes required.
- **Multi-step wizard UI** — visual progress stepper, Back/Next/Submit navigation, and per-step validation.
- **Rich field types** — `text`, `textarea`, `number`, `select`, `checkboxlist`, and `repeater` (nested, repeatable sub-forms).
- **Conditional visibility** — show or hide any field based on the value of another field using operators: `equals`, `notEquals`, `hasValue`, `isEmpty`, `contains`, `greaterThan`, `lessThan`.
- **Cascading / dependent selects** — `select` fields can depend on a parent field; the option list reloads automatically when the parent value changes.
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
│   └── Components/
│       ├── DynamicForm.razor           # Renders a list of FieldDefinitions
│       ├── FieldComponentBase.cs       # Shared base for all field components
│       ├── TextField.razor
│       ├── TextareaField.razor
│       ├── NumberField.razor
│       ├── SelectField.razor
│       ├── CheckboxListField.razor
│       └── RepeaterField.razor
│
└── FormFlow.App/               # Demo Blazor Server application
    ├── Pages/WorkflowPage.razor        # Main wizard page
    ├── Providers/                      # Example IDataSourceProvider implementations
    │   ├── CountryProvider.cs
    │   ├── CityProvider.cs
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
          "fieldType": "select",
          "dataSource": "Cities",
          "dependsOn": "CountryId",
          "order": 4,
          "visibleWhen": { "field": "CountryId", "operator": "hasValue" }
        }
      ]
    }
  ]
}
```

### Field Types

| `fieldType`     | Description                                                                 |
|-----------------|-----------------------------------------------------------------------------|
| `text`          | Single-line text input                                                      |
| `textarea`      | Multi-line text area (`rows` property controls height, default `4`)        |
| `number`        | Numeric input                                                               |
| `select`        | Dropdown populated by an `IDataSourceProvider` (`dataSource` key required) |
| `checkboxlist`  | Multi-select checkbox group, value stored as `List<string>`                 |
| `repeater`      | Dynamically add/remove structured entry groups with `subFields`             |

### Conditional Visibility

Use `visibleWhen` on any field to conditionally show it:

```jsonc
"visibleWhen": { "field": "CountryId", "operator": "equals", "value": "US" }
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

### Validation Rules

Attach `validationRules` to any field. All rules accept an optional `message` property to override the default error text.

**Scalar fields (`text`, `textarea`, `number`, `select`):**

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
{ "key": "RegionId", "label": "Region", "fieldType": "select", "dataSource": "Regions" }
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
  WorkflowPage.razor      (orchestrates steps, navigation, submission)
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
- **Open/Closed for extension** — adding a new field type requires one new `.razor` file and one line in `DynamicForm.razor`'s `Resolve` switch. Adding a new data source requires one new class and one DI registration.

---

## License

This project is licensed under the terms of the [LICENSE](LICENSE) file included in the repository. 