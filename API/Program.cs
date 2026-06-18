using API.Middlewares;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Application.UseCases.KitchenOrders.Handlers;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
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



// Handlers
builder.Services.AddScoped<ICreateKitchenOrderHandler, CreateKitchenOrderHandler>();
builder.Services.AddScoped<IRecalculateOrderHandler, RecalculateOrderHandler>();
builder.Services.AddScoped<IGetKitchenOrderByIdHandler, GetKitchenOrderByIdHandler>();
builder.Services.AddScoped<IGetKitchenOrderByOrderIdHandler, GetKitchenOrderByOrderIdHandler>();
builder.Services.AddScoped<IMarkAsDeliveredHandler, MarkAsDeliveredHandler>();

builder.Services.AddScoped<Application.UseCases.Handlers.GetKitchenQueueHandler>();
builder.Services.AddScoped<Application.UseCases.Handlers.StartItemPreparationHandler>();
builder.Services.AddScoped<Application.UseCases.Handlers.CompleteItemHandler>();
builder.Services.AddScoped<Application.UseCases.Handlers.CancelItemHandler>();

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