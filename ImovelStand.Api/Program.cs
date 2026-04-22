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
using ImovelStand.Infrastructure.Storage;
using ImovelStand.Application.Abstractions;
using ImovelStand.Application.Mapping;
using ImovelStand.Application.Services;
using ImovelStand.Api.Middleware;
using ImovelStand.Api.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

    // File storage (MinIO/Azure Blob compatível)
    builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
    builder.Services.AddSingleton<IFileStorage, MinioFileStorage>();
    builder.Services.AddSingleton<ImageProcessor>();

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

    builder.Services.AddControllers();
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
