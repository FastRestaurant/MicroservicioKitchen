using System.Security.Claims;
using System.Text;
using API.Authorization;
using API.Http;
using API.Hubs;
using API.Realtime;
using API.Middlewares;
using Application.Interfaces;
using Application.Realtime;
using Application.Services;
using Application.UseCases.KitchenOrders.Handlers;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "La solicitud es invalida." : error.ErrorMessage)
                        .ToArray());

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "La solicitud contiene datos invalidos.",
                Instance = context.HttpContext.Request.Path,
                Type = "https://httpstatuses.com/400"
            };

            problemDetails.Extensions["errorCode"] = "VALIDATION_ERROR";
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthorizationHeaderPropagationHandler>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IKitchenOrderRepository, KitchenOrderRepository>();
builder.Services.AddScoped<IKitchenOrchestratorRepository, KitchenOrchestratorRepository>();
builder.Services.AddScoped<IKitchenOrderItemRepository, KitchenOrderItemRepository>();
builder.Services.AddSingleton<KitchenSchedulingPolicy>();
builder.Services.AddSignalR();
builder.Services.AddScoped<IKitchenOrchestrator, KitchenOrchestrator>();
builder.Services.AddScoped<IKitchenNotifier, SignalRKitchenNotifier>();
builder.Services.AddScoped<ICreateKitchenOrderHandler, CreateKitchenOrderHandler>();
builder.Services.AddScoped<ICompleteKitchenOrderItemHandler, CompleteKitchenOrderItemHandler>();
builder.Services.AddScoped<IOrderServiceClient, OrderServiceClient>();
builder.Services.AddScoped<IMaxConcurrentDishesHandler, MaxConcurrentDishesHandler>();
builder.Services.AddScoped<IGetKitchenConfigurationHandler, GetKitchenConfigurationHandler>();
builder.Services.AddScoped<ICancelKitchenOrderHandler, CancelKitchenOrderHandler>();

var ordersBaseUrl = builder.Configuration["ExternalServices:Orders:BaseUrl"]
    ?? throw new InvalidOperationException("Falta la configuracion ExternalServices:Orders:BaseUrl");

builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri(ordersBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddHttpMessageHandler<AuthorizationHeaderPropagationHandler>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Falta la configuracion Jwt:Key.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role
        };

        // SignalR no puede enviar el header Authorization en el handshake WebSocket/SSE,
        // por lo que el token se acepta tambien como query string (?access_token=...)
        // unicamente para las solicitudes dirigidas al hub de Kitchen.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(KitchenHubRoutes.Path))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await SeedConfigurationAsync(db);
}

if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
    app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<KitchenHub>(KitchenHubRoutes.Path);
app.MapHealthChecks("/health");

app.Run();

static async Task SeedConfigurationAsync(ApplicationDbContext db)
{
    if (!db.KitchenConfigurations.Any())
    {
        db.KitchenConfigurations.Add(new KitchenConfiguration
        {
            MaxConcurrentDishes = KitchenConfiguration.DefaultMaxConcurrentDishes,
            FactorMultiplierTime = KitchenConfiguration.DefaultFactorMultiplierTime,
            MaxQuantityTimeMultiplier = KitchenConfiguration.DefaultMaxQuantityTimeMultiplier
        });
        await db.SaveChangesAsync();
    }
}
