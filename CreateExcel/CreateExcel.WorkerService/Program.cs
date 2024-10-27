using CreateExcel.WorkerService;
using CreateExcel.WorkerService.Models;
using CreateExcel.WorkerService.Services;

using Microsoft.EntityFrameworkCore;

using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration Configuration = hostContext.Configuration;

        services.AddHostedService<Worker>();

        services.AddDbContext<AdventureWorks2019DbContext>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("SQLServer"));
        });

        services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(Configuration["RabbitMQ:Uri"]), DispatchConsumersAsync = true });
        services.AddSingleton<RabbitMQClientService>();
    })
    .Build();

await host.RunAsync();
