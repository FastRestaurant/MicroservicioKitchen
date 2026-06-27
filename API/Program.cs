using API.Middlewares;
using Application.Interfaces;
using Application.service;
using Application.UseCases.KitchenOrders.Comands;
using Application.UseCases.KitchenOrders.Handlers;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories
builder.Services.AddScoped<IKitchenOrderRepository, KitchenOrderRepository>();
builder.Services.AddScoped<IKitchenOrchestratorRepository, KitchenOrchestratorRepository>();
builder.Services.AddScoped<IKitchenOrderItemRepository, KitchenOrderItemRepository > ();

//manejadores y servicios 
builder.Services.AddScoped<IKitchenOrchestrator, KitchenOrchestrator>();
builder.Services.AddScoped<ICreateKitchenOrderHandler, CreateKitchenOrderHandler>();
builder.Services.AddScoped<ICompleteKitchenOrderItemHandler, CompleteKitchenOrderItemHandler>();
builder.Services.AddScoped<IOrderServiceClient,OrderServiceClient>();
builder.Services.AddScoped<IMaxConcurrentDishesHandler, MaxConcurrentDishesHandler>();
builder.Services.AddScoped<ICancelKitchenOrderHandler, CancelKitchenOrderHandler> ();




builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ExternalServices:Orders:BaseUrl"]);
});



// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();


app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();