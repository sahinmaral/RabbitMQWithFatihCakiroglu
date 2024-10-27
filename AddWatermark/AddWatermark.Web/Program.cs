using AddWatermark.Web.BackgroundServices;
using AddWatermark.Web.Models;
using AddWatermark.Web.Services;

using Microsoft.EntityFrameworkCore;

using RabbitMQ.Client;

using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(builder.Configuration["RabbitMQ:Uri"]), DispatchConsumersAsync = true });
builder.Services.AddSingleton<RabbitMQClientService>();
builder.Services.AddSingleton<RabbitMQPublisher>();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseInMemoryDatabase(databaseName: "ProductDb");
});

builder.Services.AddHostedService<ImageWatermarkProcessBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
