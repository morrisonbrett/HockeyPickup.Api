using HockeyPickup.Api.Data.Context;
using HockeyPickup.Api.Data.Repositories;
using HockeyPickup.Api.GraphQL;
using HockeyPickup.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace HockeyPickup.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // This keeps PascalCase
            });

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
                TermsOfService = new Uri("https://hockeypickup.com"),
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

        builder.Services.AddDbContext<HockeyPickupContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

        builder.Services.AddLogging();
        builder.Services.AddSingleton(typeof(ILogger), typeof(Logger<Program>));

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddGraphQLServer()
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddType<UserResponseType>();

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
            app.MapHealthChecks("/health", new HealthCheckOptions
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
                            data = e.Value.Data,
                            exception = e.Value.Exception?.Message,
                            duration = e.Value.Duration
                        }),
                        totalDuration = report.TotalDuration
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
}

[ExcludeFromCodeCoverage]
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IServiceScopeFactory scopeFactory, ILogger<DatabaseHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HockeyPickupContext>();
            var connection = dbContext.Database.GetDbConnection();

            _logger.LogInformation("Starting database health check. Initial connection state: {State}", connection.State);

            // Ensure the connection is closed before we start
            if (connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
            }

            try
            {
                // Explicitly open the connection
                await connection.OpenAsync(cancellationToken);
                _logger.LogInformation("Successfully opened database connection");

                var connectionInfo = new
                {
                    DatabaseName = connection.Database,
                    ServerVersion = connection is SqlConnection sqlConn ? sqlConn.ServerVersion : "Unknown",
                    State = connection.State.ToString()
                };

                // Try the query with the open connection
                _ = await dbContext.AspNetUsers
                    .TagWith("HealthCheck")
                    .FirstOrDefaultAsync(cancellationToken);

                _logger.LogInformation("Successfully executed test query");

                return HealthCheckResult.Healthy("Database connection is healthy", new Dictionary<string, object>
                {
                    { "ConnectionInfo", connectionInfo },
                    { "LastChecked", DateTime.UtcNow }
                });
            }
            catch (Exception queryEx)
            {
                _logger.LogError(queryEx, "Health check query failed. Connection state: {State}", connection.State);
                return HealthCheckResult.Unhealthy(
                    "Database query failed",
                    queryEx,
                    new Dictionary<string, object>
                    {
                        { "ConnectionState", connection.State.ToString() },
                        { "QueryError", queryEx.Message },
                        { "LastChecked", DateTime.UtcNow }
                    });
            }
            finally
            {
                // Always ensure the connection is closed
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                    _logger.LogInformation("Closed database connection");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                ex,
                new Dictionary<string, object>
                {
                    { "Error", ex.Message },
                    { "LastChecked", DateTime.UtcNow }
                });
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

