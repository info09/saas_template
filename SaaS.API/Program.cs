using SaaS.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

if (await app.RunMigrationCommandAsync(args))
{
    return;
}

app.UseApiPipeline();
app.Run();

public partial class Program;
