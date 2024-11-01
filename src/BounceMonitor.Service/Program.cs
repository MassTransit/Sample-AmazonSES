using BounceMonitor.Consumers;
using BounceMonitor.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSwag;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(cfg => cfg.PostProcess = d =>
{
    d.Info.Title = "Amazon SES Bounce Monitor";
    d.Info.Contact = new OpenApiContact
    {
        Name = "Amazon SES Bounce Monitor"
    };
});

builder.Services.AddMassTransit(x =>
{
    x.AddDelayedMessageScheduler();

    x.AddConsumersFromNamespaceContaining<EmailBounceConsumer>();

    x.SetKebabCaseEndpointNameFormatter();

    x.AddConfigureEndpointsCallback((provider, name, cfg) =>
    {
        if (cfg is IAmazonSqsReceiveEndpointConfigurator sqs)
        {
            // long polling is healthy
            sqs.WaitTimeSeconds = 10;
        }
    });

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.UseDelayedMessageScheduler();

        cfg.Message<AmazonSesBounceNotification>(m => m.SetEntityName("ses-bounces"));
        cfg.Message<AmazonSesComplaintNotification>(m => m.SetEntityName("ses-complaints"));

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddOptions<AmazonSqsTransportOptions>()
    .BindConfiguration("TransportOptions");

builder.Services.AddOptions<MassTransitHostOptions>()
    .Configure(options =>
    {
        options.WaitUntilStarted = true;
        options.StartTimeout = TimeSpan.FromMinutes(1);
        options.StopTimeout = TimeSpan.FromMinutes(1);
    });

builder.Services.AddOptions<HostOptions>()
    .Configure(options => options.ShutdownTimeout = TimeSpan.FromMinutes(1));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseOpenApi();
app.UseSwaggerUi();

app.UseRouting();
app.UseAuthorization();

static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
{
    context.Response.ContentType = "application/json";

    return context.Response.WriteAsync(result.ToJsonString());
}

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter
});

app.MapHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });

app.MapControllers();

await app.RunAsync();