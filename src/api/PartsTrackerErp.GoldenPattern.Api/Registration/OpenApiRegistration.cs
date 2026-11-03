using System.Text.Json;
using Microsoft.OpenApi;

namespace PartsTrackerErp.GoldenPattern.Api.Registration;

internal static class OpenApiRegistration
{
    private const string SuccessEnvelopePrefix = "ApiResponseOf";

    internal static IHostApplicationBuilder AddApiOpenApi(this IHostApplicationBuilder builder)
    {
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi(options =>
        {
            // ASP.NET Core accepts a number written as a string, so the generator describes every integer and decimal as
            // a number or a string. The API only ever sends a number, and that is what a caller should read.
            options.AddSchemaTransformer((schema, _, _) =>
            {
                const JsonSchemaType numbers = JsonSchemaType.Integer | JsonSchemaType.Number;
                if (schema.Type is { } type && (type & numbers) != 0 && type.HasFlag(JsonSchemaType.String))
                {
                    schema.Type = type & ~JsonSchemaType.String;
                    schema.Pattern = null;
                }

                return Task.CompletedTask;
            });

            // ApiResponse<T>.Data has the type parameter's type, which carries no nullability, so the generator offers
            // null beside the schema of T. A success always carries its data. This runs on the finished document,
            // because the generator adds the null after the schema transformers have run.
            options.AddDocumentTransformer((document, _, _) =>
            {
                foreach ((string name, IOpenApiSchema envelope) in document.Components?.Schemas ?? new Dictionary<string, IOpenApiSchema>())
                {
                    if (!name.StartsWith(SuccessEnvelopePrefix, StringComparison.Ordinal)
                        || envelope.Properties is not { } properties
                        || !properties.TryGetValue("data", out IOpenApiSchema? data)
                        || data.OneOf is not { } alternatives)
                    {
                        continue;
                    }

                    IOpenApiSchema[] nonNull = [.. alternatives.Where(alternative => alternative is not OpenApiSchema { Type: JsonSchemaType.Null })];
                    if (nonNull.Length == 1)
                    {
                        properties["data"] = nonNull[0];
                    }
                }

                return Task.CompletedTask;
            });

            // Model binding ignores the case of a query parameter name, so the document uses camelCase, as the JSON
            // bodies and ADR 0014 do.
            options.AddOperationTransformer((operation, _, _) =>
            {
                foreach (OpenApiParameter parameter in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
                {
                    if (parameter.In == ParameterLocation.Query && parameter.Name is not null)
                    {
                        parameter.Name = JsonNamingPolicy.CamelCase.ConvertName(parameter.Name);
                    }
                }

                return Task.CompletedTask;
            });
        });

        return builder;
    }
}
