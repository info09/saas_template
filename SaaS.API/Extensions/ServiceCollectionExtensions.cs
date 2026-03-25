using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SaaS.API.Infrastructure.Swagger;
using SaaS.API.Options;
using SaaS.Application;
using SaaS.Infrastructure;
using System.Text;

namespace SaaS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfiguredCors(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddProblemDetails();
        services.Configure<TokenVersionValidationOptions>(
            configuration.GetSection(TokenVersionValidationOptions.SectionName));
        services.AddConfiguredRedis(configuration);
        services.AddConfiguredAuthentication(configuration);
        services.AddConfiguredHealthChecks(configuration);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddConfiguredSwagger();

        return services;
    }

    private static IServiceCollection AddConfiguredHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var masterConnectionString = configuration.GetConnectionString("MasterConnection")
            ?? throw new InvalidOperationException("Missing master connection string.");
        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";

        services.AddHealthChecks()
            .AddNpgSql(masterConnectionString, name: "postgres")
            .AddRedis(redisConnectionString, name: "redis");

        return services;
    }

    private static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200", "http://localhost:3000", "http://localhost:8080"])
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    private static IServiceCollection AddConfiguredRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            options.InstanceName = configuration["Redis:InstanceName"] ?? "saas:";
        });

        return services;
    }

    private static IServiceCollection AddConfiguredAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "superSecretKey12345678901234567890"))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        var problemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            Title = "Unauthorized.",
                            Detail = "A valid bearer token is required to access this resource."
                        };

                        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                        await context.Response.WriteAsJsonAsync(problemDetails);
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/problem+json";

                        var problemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status403Forbidden,
                            Title = "Forbidden.",
                            Detail = "You do not have permission to access this resource."
                        };

                        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                        await context.Response.WriteAsJsonAsync(problemDetails);
                    }
                };
            });

        return services;
    }

    private static IServiceCollection AddConfiguredSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SaaS API", Version = "v1" });
            c.OperationFilter<TenantHeaderFilter>();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    []
                }
            });
        });

        return services;
    }
}
