# Custom MCP Tool Registration — Full Reference

Source: Unity MCP 2.0.0-pre.1 (`com.unity.ai.assistant`)

---

## Method 1: Static Method with Typed Parameters (Recommended)

Best for simple tools. Schema is auto-generated from the parameter class.

```csharp
using Unity.AI.MCP.Editor.ToolRegistry;

[McpTool("my_tool", "Description of what this tool does")]
public static object MyTool(MyParameters parameters)
{
    return new { success = true, message = $"Processed {parameters.Action} for {parameters.Name}" };
}

public class MyParameters
{
    [McpDescription("Action to perform", Required = true, EnumType = typeof(ActionType))]
    public string Action { get; set; }

    [McpDescription("Target name")]
    public string Name { get; set; } = "default";
}

public enum ActionType { Create, Update, Delete }
```

Unity MCP automatically:
- Discovers and registers the tool at editor startup
- Generates JSON schema from the parameter type
- Validates parameters and converts types
- Exposes the tool to connected MCP clients

---

## Method 2: Static Method with JObject Parameters

Use for flexible or nested data structures with custom schemas:

```csharp
using Newtonsoft.Json.Linq;
using Unity.AI.MCP.Editor.ToolRegistry;

[McpTool("flexible_tool", "Tool with custom schema")]
public static object HandleFlexibleTool(JObject parameters)
{
    var action = parameters["action"]?.ToString();
    var data = parameters["data"];
    return new { processed = action, data };
}

[McpSchema("flexible_tool")]
public static object GetFlexibleToolSchema()
{
    return new
    {
        type = "object",
        properties = new
        {
            action = new { type = "string", @enum = new[] { "process", "validate" } },
            data = new { type = "object", description = "Flexible data object" }
        },
        required = new[] { "action" }
    };
}
```

---

## Method 3: Class-Based Tool (Stateful)

Use when the tool needs internal state or helper methods:

```csharp
using Unity.AI.MCP.Editor.ToolRegistry;

[McpTool("stateful_tool", "Tool with internal state")]
public class StatefulTool : IUnityMcpTool<StatefulParams>
{
    readonly Dictionary<string, object> _cache = new();

    public Task<object> ExecuteAsync(StatefulParams parameters)
    {
        var result = new { id = parameters.Id, processed = true };
        _cache[parameters.Id] = result;
        return Task.FromResult<object>(result);
    }
}

public class StatefulParams
{
    [McpDescription("Unique identifier", Required = true)]
    public string Id { get; set; }
}
```

Implement `IUnityMcpTool` (without generics) for `JObject`-parameter tools that provide custom
schemas via `GetInputSchema()` and `GetOutputSchema()`.

---

## Method 4: Runtime API (Dynamic Registration)

Register or unregister tools programmatically:

```csharp
using Unity.AI.MCP.Editor.ToolRegistry;

// Register
var tool = new MyTool();
McpToolRegistry.RegisterTool<MyParams>("dynamic_tool", tool, "A dynamically registered tool");

// Unregister when no longer needed
McpToolRegistry.UnregisterTool("dynamic_tool");
```

---

## Attribute Reference

### `[McpTool(name, description)]`

Marks a static method or class as an MCP tool.

| Property | Type | Required | Description |
|---|---|---|---|
| `Name` | string | ✅ | Unique tool identifier exposed to MCP clients |
| `Description` | string | — | Human-readable description |
| `Title` | string | — | Display title (defaults to description) |
| `Groups` | string[] | — | Category tags for organizing tools |

### `[McpDescription(description)]`

Adds descriptions and constraints to parameter properties.

| Property | Type | Required | Description |
|---|---|---|---|
| `Description` | string | ✅ | Human-readable description of the parameter |
| `Required` | bool | — | Marks the parameter as mandatory (default: false) |
| `EnumType` | Type | — | Enum type for constraining string values |
| `Default` | object | — | Explicit default value for the schema |

### `[McpSchema(toolName)]`

Links a static method to a tool as its custom input schema provider.
Use with `JObject`-parameter tools.

### `[McpOutputSchema(toolName)]`

Links a static method to a tool as its custom output schema provider.

---

## Auto-Generated JSON Schema

Given this parameter class:

```csharp
public class ExampleParams
{
    [McpDescription("Name of the object", Required = true)]
    public string Name { get; set; }

    [McpDescription("Scale multiplier", Default = 1.0)]
    public float Scale { get; set; } = 1.0f;

    [McpDescription("Object type", EnumType = typeof(ObjectType))]
    public string Type { get; set; }
}

public enum ObjectType { Cube, Sphere, Cylinder }
```

Unity MCP generates:

```json
{
  "type": "object",
  "properties": {
    "name":  { "type": "string", "description": "Name of the object" },
    "scale": { "type": "number", "description": "Scale multiplier", "default": 1.0 },
    "type":  { "type": "string", "description": "Object type", "enum": ["cube","sphere","cylinder"] }
  },
  "required": ["name"]
}
```

---

## Registry Events

```csharp
McpToolRegistry.ToolsChanged += (args) =>
{
    switch (args.ChangeType)
    {
        case McpToolRegistry.ToolChangeType.Added:
            Debug.Log($"Tool added: {args.ToolName}");
            break;
        case McpToolRegistry.ToolChangeType.Removed:
            Debug.Log($"Tool removed: {args.ToolName}");
            break;
        case McpToolRegistry.ToolChangeType.Refreshed:
            Debug.Log("All tools refreshed");
            break;
    }
};
```

---

## Unity.RunCommand Tool

The built-in `Unity.RunCommand` tool executes arbitrary C# at edit time:

```
Parameters:
  Code (string, required) — C# script implementing IRunCommand or valid C# script
  Title (string, optional) — label for the execution command
```

Example prompt for clients: *"Run a command to count all GameObjects in the active scene."*
