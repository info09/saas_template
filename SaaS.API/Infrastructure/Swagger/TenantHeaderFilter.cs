using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SaaS.API.Infrastructure.Swagger;

public class TenantHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Id",
            In = ParameterLocation.Header,
            Required = false, // Set to true if you want it mandatory for all
            Schema = new OpenApiSchema
            {
                Type = "string"
            },
            Description = "Tenant identifier for multi-tenant resolution"
        });
    }
}
