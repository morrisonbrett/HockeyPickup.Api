using HotChocolate.Execution.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace HockeyPickup.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            var baseUrl = "/api";
            o.AddServer(new OpenApiServer
            {
                Url = baseUrl
            });
            o.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "1.0.0",
                Title = "HockeyPickup.Api",
                Description = "HockeyPickup APIs using strict OpenAPI specification.",
                TermsOfService = new Uri("https://hockeypickup.com/terms"),
                Contact = new OpenApiContact()
                {
                    Name = "HockeyPickup IT",
                    Email = "info@hockeypickup.com",
                    Url = new Uri("https://github.com/morrisonbrett/HockeyPickup.Api/issues")
                },
                License = new OpenApiLicense()
                {
                    Name = "MIT License",
                    Url = new Uri("https://raw.githubusercontent.com/morrisonbrett/HockeyPickup.Api/master/LICENSE")
                }
            });
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Please enter a valid HockeyPickup.API token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            o.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        builder.Services.AddLogging();
        builder.Services.AddSingleton(typeof(ILogger), typeof(Logger<Program>));
        builder.Services.AddGraphQLServer().AddQueryType<Query>();
        builder.Services.AddHealthChecks()
            .AddCheck("Api", () => HealthCheckResult.Healthy("Api is healthy"))
            .AddCheck<DatabaseHealthCheck>("Database");

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseOpenApi();
        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api" }
                    };
            });
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "HockeyPickup.Api");
            c.RoutePrefix = string.Empty;
            c.DisplayRequestDuration();
            c.EnableDeepLinking();
            c.EnableValidator();
        });

        app.UsePathBase("/api");
        app.UseRouting();
        app.UseWebSockets();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(e =>
        {
            e.MapControllers();
            e.MapGraphQL();
            e.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.ToString()
                        }),
                        duration = report.TotalDuration
                    };
                    await context.Response.WriteAsJsonAsync(response);
                }
            });
            e.MapGet("/", context =>
            {
                context.Response.Redirect("/index.html");
                return Task.CompletedTask;
            });
        });

        app.MapControllers();

        app.Run();
    }

    [ExcludeFromCodeCoverage]
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseHealthCheck(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                //using var scope = _scopeFactory.CreateScope();
                //var dbContext = scope.ServiceProvider.GetRequiredService<ITrueVoteDbContext>();
                //await dbContext.EnsureCreatedAsync();
                return HealthCheckResult.Healthy("Database connection is healthy");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database connection is unhealthy", ex);
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the endpoint (action) has the Authorize attribute
            var hasAuthorizeAttribute = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>().Any() ||
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (hasAuthorizeAttribute)
            {
                // If the endpoint has [Authorize] attribute, display the "Authorize" button
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
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
                            new string[] {}
                        }
                    }
                };
            }
        }
    }
}

internal class AuthorizeCheckOperationFilter
{
}
