using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaaS.API.Middlewares;
using SaaS.Application;
using SaaS.Infrastructure;
using SaaS.Infrastructure.Persistence;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "superSecretKey12345678901234567890"))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middlewares
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();
app.UseMiddleware<TenantAuthorizationMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Automatic Database Migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var catalogContext = services.GetRequiredService<SaaS.Infrastructure.Persistence.CatalogDbContext>();
        await catalogContext.Database.MigrateAsync();

        await services.ApplyTenantMigrationsAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration.");
    }
}

app.Run();
