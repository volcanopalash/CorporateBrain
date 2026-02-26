using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
namespace CorporateBrain.Api.OpenApi;

public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer", // Scalar likes this lowercase
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Id = "Bearer",
                Type = ReferenceType.SecurityScheme
            }
        };

        // 2. Add the scheme to the docuemnt components 
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", authenticationScheme);

        // 3. Apply the lock to all endpoints globally in the UI
        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations.Values))
        {
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [authenticationScheme] = Array.Empty<string>()
            });
        }

        return Task.CompletedTask;
    }
}
