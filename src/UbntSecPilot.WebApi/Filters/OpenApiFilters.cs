using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;

namespace UbntSecPilot.WebApi.Filters
{
    /// <summary>
    /// Custom schema filter for better enum documentation
    /// </summary>
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name => schema.Enum.Add(new OpenApiString($"{char.ToLower(name[0])}{name.Substring(1)}")));

                schema.Description += $"<br/>Available values: {string.Join(", ", Enum.GetNames(context.Type))}";
            }
        }
    }

    /// <summary>
    /// Schema filter for consistent response schemas
    /// </summary>
    public class ResponseSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Add common response properties
            if (context.Type.Name.EndsWith("Dto") || context.Type.Name.EndsWith("Response"))
            {
                if (!schema.Properties.ContainsKey("id"))
                {
                    schema.Properties.Add("id", new OpenApiSchema
                    {
                        Type = "string",
                        Description = "Unique identifier",
                        Example = new OpenApiString("64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n")
                    });
                }

                if (!schema.Properties.ContainsKey("createdAt"))
                {
                    schema.Properties.Add("createdAt", new OpenApiSchema
                    {
                        Type = "string",
                        Format = "date-time",
                        Description = "Creation timestamp",
                        Example = new OpenApiString("2024-01-15T10:30:00Z")
                    });
                }

                if (!schema.Properties.ContainsKey("updatedAt"))
                {
                    schema.Properties.Add("updatedAt", new OpenApiSchema
                    {
                        Type = "string",
                        Format = "date-time",
                        Description = "Last update timestamp",
                        Nullable = true,
                        Example = new OpenApiString("2024-01-15T10:30:00Z")
                    });
                }
            }
        }
    }

    /// <summary>
    /// Operation filter for security requirements
    /// </summary>
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the endpoint requires authorization
            var hasAuthorizeAttribute = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .Any();

            if (hasAuthorizeAttribute)
            {
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }

            // Add API key as optional security
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
    }

    /// <summary>
    /// Operation filter for response examples
    /// </summary>
    public class ResponseExamplesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Add response examples based on return type
            var returnType = context.MethodInfo.ReturnType;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var actualReturnType = returnType.GetGenericArguments()[0];

                if (actualReturnType == typeof(IResult))
                {
                    // Add common response examples
                    operation.Responses.Add("200", new OpenApiResponse
                    {
                        Description = "Success",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Examples = new Dictionary<string, OpenApiExample>
                                {
                                    ["success"] = new OpenApiExample
                                    {
                                        Summary = "Success response",
                                        Value = new OpenApiObject
                                        {
                                            ["success"] = new OpenApiBoolean(true),
                                            ["data"] = new OpenApiObject(),
                                            ["message"] = new OpenApiString("Operation completed successfully")
                                        }
                                    }
                                }
                            }
                        }
                    });

                    operation.Responses.Add("400", new OpenApiResponse
                    {
                        Description = "Bad Request",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Examples = new Dictionary<string, OpenApiExample>
                                {
                                    ["validation_error"] = new OpenApiExample
                                    {
                                        Summary = "Validation error",
                                        Value = new OpenApiObject
                                        {
                                            ["success"] = new OpenApiBoolean(false),
                                            ["message"] = new OpenApiString("Validation failed"),
                                            ["errors"] = new OpenApiArray
                                            {
                                                new OpenApiString("Field 'name' is required")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    operation.Responses.Add("401", new OpenApiResponse
                    {
                        Description = "Unauthorized",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Examples = new Dictionary<string, OpenApiExample>
                                {
                                    ["unauthorized"] = new OpenApiExample
                                    {
                                        Summary = "Authentication required",
                                        Value = new OpenApiObject
                                        {
                                            ["success"] = new OpenApiBoolean(false),
                                            ["message"] = new OpenApiString("Authentication required"),
                                            ["errors"] = new OpenApiArray
                                            {
                                                new OpenApiString("Valid JWT token required")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    operation.Responses.Add("403", new OpenApiResponse
                    {
                        Description = "Forbidden",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Examples = new Dictionary<string, OpenApiExample>
                                {
                                    ["forbidden"] = new OpenApiExample
                                    {
                                        Summary = "Insufficient permissions",
                                        Value = new OpenApiObject
                                        {
                                            ["success"] = new OpenApiBoolean(false),
                                            ["message"] = new OpenApiString("Insufficient permissions"),
                                            ["errors"] = new OpenApiArray
                                            {
                                                new OpenApiString("Admin role required for this operation")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    operation.Responses.Add("404", new OpenApiResponse
                    {
                        Description = "Not Found",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Examples = new Dictionary<string, OpenApiExample>
                                {
                                    ["not_found"] = new OpenApiExample
                                    {
                                        Summary = "Resource not found",
                                        Value = new OpenApiObject
                                        {
                                            ["success"] = new OpenApiBoolean(false),
                                            ["message"] = new OpenApiString("Resource not found"),
                                            ["errors"] = new OpenApiArray
                                            {
                                                new OpenApiString("No event found with the specified ID")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    operation.Responses.Add("500", new OpenApiResponse
                    {
                        Description = "Internal Server Error",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Examples = new Dictionary<string, OpenApiExample>
                                {
                                    ["server_error"] = new OpenApiExample
                                    {
                                        Summary = "Internal server error",
                                        Value = new OpenApiObject
                                        {
                                            ["success"] = new OpenApiBoolean(false),
                                            ["message"] = new OpenApiString("Internal server error"),
                                            ["errors"] = new OpenApiArray
                                            {
                                                new OpenApiString("An unexpected error occurred")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }
    }

    /// <summary>
    /// Document filter for API version metadata
    /// </summary>
    public class ApiVersionDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Add version information to the document
            swaggerDoc.Info.Version = "1.0.0";
            swaggerDoc.Info.Title = "UBNT SecPilot API";
            swaggerDoc.Info.Description = "Enterprise Security Threat Analysis and Monitoring API";

            // Add server information
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Url = "https://api.ubnt-security.com",
                Description = "Production server"
            });

            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Url = "http://localhost:8000",
                Description = "Development server"
            });

            // Add additional metadata
            swaggerDoc.Components ??= new OpenApiComponents();
            swaggerDoc.Components.Add(new KeyValuePair<string, OpenApiSchema>("ErrorResponse", new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["success"] = new OpenApiSchema
                    {
                        Type = "boolean",
                        Description = "Operation success status",
                        Example = new OpenApiBoolean(false)
                    },
                    ["message"] = new OpenApiSchema
                    {
                        Type = "string",
                        Description = "Error message",
                        Example = new OpenApiString("Validation failed")
                    },
                    ["errors"] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string" },
                        Description = "Detailed error messages",
                        Example = new OpenApiArray
                        {
                            new OpenApiString("Field 'name' is required")
                        }
                    }
                }
            }));

            // Add common response schemas
            swaggerDoc.Components.Add(new KeyValuePair<string, OpenApiSchema>("SuccessResponse", new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["success"] = new OpenApiSchema
                    {
                        Type = "boolean",
                        Description = "Operation success status",
                        Example = new OpenApiBoolean(true)
                    },
                    ["data"] = new OpenApiSchema
                    {
                        Type = "object",
                        Description = "Response data"
                    },
                    ["message"] = new OpenApiSchema
                    {
                        Type = "string",
                        Description = "Success message",
                        Example = new OpenApiString("Operation completed successfully")
                    }
                }
            }));

            // Add pagination schema
            swaggerDoc.Components.Add(new KeyValuePair<string, OpenApiSchema>("PaginatedResponse", new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["items"] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "object" },
                        Description = "Data items"
                    },
                    ["totalCount"] = new OpenApiSchema
                    {
                        Type = "integer",
                        Description = "Total number of items",
                        Example = new OpenApiInteger(100)
                    },
                    ["pageNumber"] = new OpenApiSchema
                    {
                        Type = "integer",
                        Description = "Current page number",
                        Example = new OpenApiInteger(1)
                    },
                    ["pageSize"] = new OpenApiSchema
                    {
                        Type = "integer",
                        Description = "Page size",
                        Example = new OpenApiInteger(20)
                    },
                    ["totalPages"] = new OpenApiSchema
                    {
                        Type = "integer",
                        Description = "Total number of pages",
                        Example = new OpenApiInteger(5)
                    }
                }
            }));
        }
    }
}
