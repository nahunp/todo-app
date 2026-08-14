using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApp.WebApi.Common;

/// <summary>
/// Swashbuckle's schema generator doesn't read JsonSerializerOptions.Converters
/// — registering JsonStringEnumConverter in Program.cs (so the API actually
/// sends/accepts "High"/"Work" over the wire, not raw ints) doesn't change
/// what docs/api/openapi.json says on its own. Found by generating the spec
/// and diffing it against a real request/response, not by assuming they'd
/// agree. Without this filter, the schema still shows PriorityLevel as
/// integer enum [0,1,2] — actively misleading, since copilot-instructions.md
/// tells every frontend/mobile consumer to treat this file as the source of
/// truth over the C# source.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
            return;

        schema.Enum.Clear();
        schema.Type = "string";
        schema.Format = null;

        foreach (var name in Enum.GetNames(context.Type))
        {
            schema.Enum.Add(new OpenApiString(name));
        }
    }
}
