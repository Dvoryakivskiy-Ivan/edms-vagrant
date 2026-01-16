using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EDMS.MvcClient.Swagger;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // Create a Swagger document per API version (v1, v2, etc.)
        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title = "EDMS.MvcClient API",
                Version = desc.ApiVersion.ToString(),
                Description = "Electronic Document Management System API"
            });
        }


        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            // docName is like "v1" or "v2"
            var versions = apiDesc.ActionDescriptor.EndpointMetadata
                .OfType<ApiVersionAttribute>()
                .SelectMany(a => a.Versions)
                .Select(v => $"v{v.MajorVersion}")
                .Distinct()
                .ToList();

            if (versions.Count == 0)
                return docName == "v1";

            return versions.Contains(docName);
        });

    }
}
