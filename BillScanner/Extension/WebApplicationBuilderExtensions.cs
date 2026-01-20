using Asp.Versioning;
using BillScanner.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace BillScanner.Extension
{
    public static class WebApplicationBuilderExtensions
    {
        public static void AddPresentation(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            // Add Serilog
            builder.Host.UseSerilog((context, loggerConfig) => 
                loggerConfig.ReadFrom.Configuration(context.Configuration));

            // Add OpenAPI with document transformers
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = "BillScanner API",
                        Version = "v1",
                        Description = "API for BillScanner application"
                    };
                    return Task.CompletedTask;
                });
            });

            // Add Controllers with Endpoints
            builder.Services.AddControllers(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtAccessTokenSettings:Secret"]!)),
                    ValidIssuer = builder.Configuration["JwtAccessTokenSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtAccessTokenSettings:Audience"],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(3)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken)
                            && (path.StartsWithSegments("/hub/notifications") ||
                                path.StartsWithSegments("/hub/signaling") ||
                                path.StartsWithSegments("/hub/messaging")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"[JWT] Token validated for user: {context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)}");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[JWT] Authentication failed: {context.Exception?.Message}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"[JWT] Authentication challenged: {context.Error} - {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });

            // Add Authorization
            builder.Services.AddAuthorization(options =>
            {
                var requireAuthPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.DefaultPolicy = requireAuthPolicy;
            });

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("Location");
                });

                options.AddPolicy("AllowAllOrigin", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders("Location");
                });
            });

            // Add endpoints API explorer for OpenAPI/Swagger
            builder.Services.AddEndpointsApiExplorer();

            // Add Swagger UI
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.Response;
                logging.MediaTypeOptions.AddText("application/javascript");
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;
                logging.CombineLogs = true;
            });

            // Add API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Version"),
                    new MediaTypeApiVersionReader("X-Version"));
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        }
    }

    /// <summary>
    /// OpenAPI document transformer that adds JWT Bearer authentication scheme
    /// </summary>
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) 
        : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
            {
                // Add the security scheme at the document level
                var securityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid JWT token"
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = securityScheme;

                // Apply it as a requirement for all operations
                foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
                {
                    operation.Value.Security ??= [];
                    operation.Value.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
                }
            }
        }
    }
}
