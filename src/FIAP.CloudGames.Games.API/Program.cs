using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FIAP.CloudGames.Games.API.Extensions;
using FIAP.CloudGames.Games.API.Middlewares;
using FIAP.CloudGames.Games.Application.Interfaces;
using FIAP.CloudGames.Games.Application.Services;
using FIAP.CloudGames.Games.Domain.Interfaces.Repositories;
using FIAP.CloudGames.Games.Infrastructure.Configuration.Auth;
using FIAP.CloudGames.Games.Infrastructure.Context;
using FIAP.CloudGames.Games.Infrastructure.Logging;
using FIAP.CloudGames.Games.Infrastructure.Repositories;
using FIAP.CloudGames.Games.Infrastructure.Configuration.Search;
using FIAP.CloudGames.Games.Infrastructure.Search;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();

// DbContext
builder.Services.AddDbContext<GamesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

#region Application Services Configuration

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddControllers();

#endregion

#region JWT Authentication Configuration

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Secret))
    throw new InvalidOperationException("JWT settings are not configured properly. Please check your appsettings.json.");

var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role
        };
    });

#endregion

builder.Services.Configure<ElasticsearchSettings>(
    builder.Configuration.GetSection("Elasticsearch"));

builder.Services.AddSingleton(sp =>
{
    var settings = builder.Configuration
        .GetSection("Elasticsearch")
        .Get<ElasticsearchSettings>() ?? new ElasticsearchSettings();

    var clientSettings = new ElasticsearchClientSettings(new Uri(settings.Url));
    return new ElasticsearchClient(clientSettings);
});

builder.Services.AddSingleton(sp =>
{
    return builder.Configuration
        .GetSection("Elasticsearch")
        .Get<ElasticsearchSettings>() ?? new ElasticsearchSettings();
});

builder.Services.AddScoped<IGameSearchService, GameSearchService>();

builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<ElasticsearchSettings>>().Value;

    var uri = new Uri(settings.Url);
    var clientSettings = new ElasticsearchClientSettings(uri)
        .DisableDirectStreaming(); // ajuda a debugar

    return new ElasticsearchClient(clientSettings);
});

builder.Services.AddScoped<IGameSearchService, GameSearchService>();

var app = builder.Build();

app.UsePathBase("/games");
app.UseRouting();

// Middlewares
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

// Migrações (igual Users, porém sem Seed)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
    db.Database.Migrate();
}

app.MapGet("/health", () => Results.Ok("OK")).AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    var swaggerBasePath = builder.Configuration["SwaggerBasePath"] ?? "/games";

    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swagger, req) =>
        {
            swagger.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = swaggerBasePath }
            };
        });
    });

    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint("v1/swagger.json", "Games API v1");
    });
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
