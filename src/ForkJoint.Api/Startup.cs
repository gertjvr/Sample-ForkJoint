namespace ForkJoint.Api;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Components.Activities;
using Components.Consumers;
using Components.Futures;
using Components.ItineraryPlanners;
using Contracts;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using MassTransit;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Services;


public class Startup
{
    static bool? _isRunningInContainer;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    IConfiguration Configuration { get; }

    static bool IsRunningInContainer =>
        _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

    public void ConfigureServices(IServiceCollection services)
    {
        services.TryAddScoped<IItineraryPlanner<OrderBurger>, BurgerItineraryPlanner>();
        services.TryAddSingleton<IGrill, Grill>();
        services.TryAddSingleton<IFryer, Fryer>();
        services.TryAddSingleton<IShakeMachine, ShakeMachine>();

        services.AddApplicationInsightsTelemetry(options =>
        {
            options.EnableDependencyTrackingTelemetryModule = true;
        });
        services.AddApplicationInsightsTelemetryProcessor<NoSqlTelemetryProcessor>();

        services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
        {
            module.IncludeDiagnosticSourceActivities.Add("MassTransit");
        });
            
        services.AddSingleton<ITelemetryInitializer, HttpDependenciesParsingTelemetryInitializer>();

        services.AddOpenTelemetryMetrics(builder =>
        {
            builder
                .AddMeter("MassTransit")
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("ForkJoint.Api")
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(IsRunningInContainer ? "http://grafana-agent:14317" : "http://localhost:14317");
                    o.Protocol = OtlpExportProtocol.Grpc;
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };  
                });
        });
        
        services.AddOpenTelemetryTracing(builder =>
        {
            builder
                .AddSource("MassTransit")
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("ForkJoint.Api")
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .AddAspNetCoreInstrumentation()
                .AddSqlClientInstrumentation(o =>
                {
                    o.EnableConnectionLevelAttributes = true;
                    o.RecordException = true;
                    o.SetDbStatementForText = true;
                    o.SetDbStatementForStoredProcedure = true;
                })
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(IsRunningInContainer ? "http://tempo:4317" : "http://localhost:4317");
                    o.Protocol = OtlpExportProtocol.Grpc;
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };
                })
                .AddJaegerExporter(o =>
                {
                    o.Endpoint = new Uri(IsRunningInContainer ? "http://jaeger:6831" : "http://localhost:6831");
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };
                });
        });

        var connectionString = GetConnectionString();
        
        services.AddMassTransit(x =>
        {
            x.ApplyCustomMassTransitConfiguration();

            x.AddServiceBusMessageScheduler();
            
            x.SetCosmosSagaRepositoryProvider(r =>
            {
                r.EndpointUri = connectionString.EndpointUri;
                r.Key = connectionString.Key;

                r.DatabaseId = "ForkJoint";
            });

            x.AddConsumersFromNamespaceContaining<CookOnionRingsConsumer>();

            x.AddActivitiesFromNamespaceContaining<GrillBurgerActivity>();

            x.AddFuturesFromNamespaceContaining<OrderFuture>();

            x.AddSagaRepository<FutureState>()
                .CosmosRepository(r =>
                {
                    r.EndpointUri = connectionString.EndpointUri;
                    r.Key = connectionString.Key;
                    
                    r.DatabaseId = "ForkJoint";
                    r.CollectionId = "FutureState";
                });
            
            x.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.AutoStart = true;
                
                cfg.UseInstrumentation();
                
                cfg.ApplyCustomBusConfiguration();
                
                cfg.Host(Configuration["ConnectionStrings:ServiceBus"]);

                cfg.UseServiceBusMessageScheduler();

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ForkJoint.Api",
                Version = "v1"
            });
        });
    }

    (string EndpointUri, string Key)  GetConnectionString()
    {
        var connectionString = Configuration.GetConnectionString("ForkJoint");

        if (IsRunningInContainer)
            connectionString = connectionString.Replace("localhost", "cosmosdb");
        
        var parts = connectionString.Split(";", StringSplitOptions.RemoveEmptyEntries)
            .Select(x =>
            {
                var p = x.IndexOf("=", StringComparison.InvariantCultureIgnoreCase);
                var key = x.Substring(0, p);
                var value = x.Substring(p + 1);

                return (key, value);
            })
            .ToDictionary(x => x.key, x => x.value);

        return (parts["AccountEndpoint"], parts["AccountKey"]);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ForkJoint.Api v1"));
        }
        
        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter
            });

            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions {ResponseWriter = HealthCheckResponseWriter});
        });
    }

    static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
    {
        var json = new JObject(
            new JProperty("status", result.Status.ToString()),
            new JProperty("results", new JObject(result.Entries.Select(entry => new JProperty(entry.Key, new JObject(
                new JProperty("status", entry.Value.Status.ToString()),
                new JProperty("description", entry.Value.Description),
                new JProperty("data", JObject.FromObject(entry.Value.Data))))))));

        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(json.ToString(Formatting.Indented));
    }
}