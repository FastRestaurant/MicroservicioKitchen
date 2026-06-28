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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<ICancelKitchenOrderHandler, CancelKitchenOrderHandler>();

var ordersBaseUrl = builder.Configuration["ExternalServices:Orders:BaseUrl"]
    ?? throw new InvalidOperationException("Falta la configuracion ExternalServices:Orders:BaseUrl");

builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri(ordersBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddHttpMessageHandler<AuthorizationHeaderPropagationHandler>();

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)),
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
        db.KitchenConfigurations.Add(new KitchenConfiguration { MaxConcurrentDishes = 10 });
        await db.SaveChangesAsync();
    }
}
