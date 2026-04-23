using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Infrastructure.Interceptors;
using ImovelStand.Infrastructure.Conversao;
using ImovelStand.Infrastructure.Notificacoes;
using ImovelStand.Infrastructure.IA;
using ImovelStand.Infrastructure.Storage;
using ImovelStand.Application.Abstractions;
using ImovelStand.Application.Mapping;
using ImovelStand.Application.Services;
using ImovelStand.Api.Middleware;
using ImovelStand.Api.Services;
using ImovelStand.Jobs.Jobs;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Mapster;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var sentryDsn = builder.Configuration["Sentry:Dsn"];
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
        if (!string.IsNullOrWhiteSpace(sentryDsn))
        {
            configuration.WriteTo.Sentry(o =>
            {
                o.Dsn = sentryDsn;
                o.MinimumBreadcrumbLevel = Serilog.Events.LogEventLevel.Information;
                o.MinimumEventLevel = Serilog.Events.LogEventLevel.Error;
                o.Environment = context.HostingEnvironment.EnvironmentName;
            });
        }
    });

    if (!string.IsNullOrWhiteSpace(sentryDsn))
    {
        builder.WebHost.UseSentry(o =>
        {
            o.Dsn = sentryDsn;
            o.TracesSampleRate = builder.Environment.IsProduction() ? 0.1 : 1.0;
            o.Environment = builder.Environment.EnvironmentName;
        });
    }

    // Application Insights (opcional — se ConnectionString configurado)
    var aiConn = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
        ?? builder.Configuration["ApplicationInsights:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(aiConn))
    {
        builder.Services.AddApplicationInsightsTelemetry(o => o.ConnectionString = aiConn);
    }

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

    // File storage (MinIO/Azure Blob compatível)
    builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
    builder.Services.AddSingleton<IFileStorage, MinioFileStorage>();
    builder.Services.AddSingleton<ImageProcessor>();
    builder.Services.AddSingleton<CalculadoraFinanceira>();
    builder.Services.AddSingleton<EspelhoPdfGenerator>();
    builder.Services.AddSingleton<ContratoTemplateEngine>();
    builder.Services.AddSingleton<DashboardService>();
    builder.Services.AddSingleton<ExcelExporter>();
    builder.Services.AddSingleton<ExcelImporter>();
    builder.Services.AddScoped<WebhookDispatcher>();
    builder.Services.Configure<IuguOptions>(builder.Configuration.GetSection("Iugu"));
    builder.Services.AddScoped<IuguBillingService>();

    // Notificações
    builder.Services.Configure<NotificacaoOptions>(builder.Configuration.GetSection("Notificacao"));
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<INotificador, MailKitNotificador>();

    // Módulo IA (Claude / Anthropic) — copiloto para corretor
    builder.Services.Configure<IAOptions>(builder.Configuration.GetSection(IAOptions.SectionName));
    builder.Services.AddMemoryCache();
    builder.Services.AddHttpClient("anthropic", c =>
    {
        c.Timeout = TimeSpan.FromSeconds(60);
    });
    builder.Services.AddScoped<IIAService, ClaudeIAService>();
    builder.Services.AddScoped<CopilotoService>();

    // Jobs
    builder.Services.AddScoped<ExpirarReservasJob>();
    builder.Services.AddScoped<ExpirarPropostasJob>();
    builder.Services.AddScoped<LembreteReservaVencendoJob>();
    builder.Services.AddScoped<EspelhoSemanalJob>();

    // DOCX -> PDF
    builder.Services.Configure<DocxToPdfOptions>(builder.Configuration.GetSection("DocxToPdf"));
    builder.Services.AddScoped<IDocxToPdfConverter, GotenbergDocxToPdfConverter>();

    // Hangfire (storage SQL usa a mesma connection string do app)
    var hangfireConn = builder.Configuration.GetConnectionString("Hangfire")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddHangfire(cfg => cfg
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(hangfireConn, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));
    builder.Services.AddHangfireServer();

    builder.Services.AddSingleton<HistoricoPrecoInterceptor>();
    builder.Services.AddScoped<TenantAssignmentInterceptor>();

    builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(
                   sp.GetRequiredService<HistoricoPrecoInterceptor>(),
                   sp.GetRequiredService<TenantAssignmentInterceptor>()));

    builder.Services.AddScoped<TokenService>();

    // Mapster
    var mapsterConfig = TypeAdapterConfig.GlobalSettings;
    MappingRegistry.Register(mapsterConfig);
    builder.Services.AddSingleton(mapsterConfig);
    builder.Services.AddScoped<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();

    // FluentValidation — auto-registro de todos os validators em Application
    builder.Services.AddValidatorsFromAssembly(typeof(MappingRegistry).Assembly);
    builder.Services.AddFluentValidationAutoValidation();

    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;

        // /api/auth/login: 10 tentativas por IP a cada 5 minutos.
        options.AddPolicy("auth", httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            });
        });
    });

    // Health checks: DB + readiness
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "sqlserver",
            tags: new[] { "db", "ready" })
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
            tags: new[] { "live" });

    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            // Enums como string no JSON (ex: "Disponivel" em vez de 0). Sem isso,
            // o frontend recebe numeros e quebra toda a visualizacao de status.
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ImovelStand API",
            Version = "v1",
            Description = "API para gerenciamento de vendas e reservas de apartamentos"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
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
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ProblemDetailsMiddleware>();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao aplicar migrations no banco de dados.");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ImovelStand API v1");
        });
    }

    app.UseCors("AllowAll");

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Kubernetes-style probes: /api/health/live (qualquer estado == 200 = processo vivo),
    // /api/health/ready (200 = pronto pra receber tráfego — DB + dependências OK)
    app.MapHealthChecks("/api/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });
    app.MapHealthChecks("/api/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("ready")
    });

    // Dashboard do Hangfire — acessível em /hangfire, protegido em prod por auth filter
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAdminAuthorizationFilter(app.Environment) }
    });

    // Registra RecurringJobs
    RecurringJob.AddOrUpdate<ExpirarReservasJob>(
        "expirar-reservas",
        job => job.ExecuteAsync(CancellationToken.None),
        "*/10 * * * *"); // a cada 10min

    RecurringJob.AddOrUpdate<ExpirarPropostasJob>(
        "expirar-propostas",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Daily(3)); // 3h UTC

    RecurringJob.AddOrUpdate<LembreteReservaVencendoJob>(
        "lembrete-reserva-vencendo",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Daily(11)); // 8h BRT

    RecurringJob.AddOrUpdate<EspelhoSemanalJob>(
        "espelho-semanal",
        job => job.ExecuteAsync(CancellationToken.None),
        "0 21 * * 5"); // Sexta 18h BRT (21h UTC)

    app.Run();
}
catch (Exception ex) when (ex is not Microsoft.Extensions.Hosting.HostAbortedException)
{
    Log.Fatal(ex, "Aplicação encerrou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
