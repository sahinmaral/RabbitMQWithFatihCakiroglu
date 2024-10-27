
using ClosedXML.Excel;

using CreateExcel.Shared;
using CreateExcel.WorkerService.Models;
using CreateExcel.WorkerService.Services;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Data;
using System.Text;
using System.Text.Json;

namespace CreateExcel.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private RabbitMQClientService _rabbitMQClientService;
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, RabbitMQClientService rabbitMQClientService)
        {
            // DbContext'in direkt olarak DI olarak alýnamamasýnýn nedeni yaþam döngüsü olarak Scoped olduðu içindir. 

            _logger = logger;
            _serviceProvider = serviceProvider;
            _rabbitMQClientService = rabbitMQClientService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: RabbitMQClientService.QueueName, autoAck: false, consumer);

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);

            var bodyByte = @event.Body.ToArray();
            var bodyString = Encoding.UTF8.GetString(bodyByte);

            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(bodyString);

            using var memoryStream = new MemoryStream();
            var workBook = new XLWorkbook();
            var dataSet = new DataSet();
            dataSet.Tables.Add(GetTable("products"));

            workBook.Worksheets.Add(dataSet);
            workBook.SaveAs(memoryStream);

            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            var httpContent = new ByteArrayContent(content: memoryStream.ToArray());
            multipartFormDataContent.Add(httpContent, name: "file", fileName: Guid.NewGuid().ToString() + ".xlsx");

            var baseUrl = "http://localhost:5278/api/Files/Upload";

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(requestUri: $"{baseUrl}?fileId={createExcelMessage.FileId}", content: multipartFormDataContent);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"File (Id : {createExcelMessage.FileId}) was created successfully");
                _channel.BasicAck(deliveryTag: @event.DeliveryTag, multiple: false);
            }

        }

        private DataTable GetTable(string tableName)
        {
            List<Product> products;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019DbContext>();
                products = context.Products.ToList();
            }

            DataTable table = new DataTable() { TableName = tableName };
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("Color", typeof(string));

            foreach (var product in products)
            {
                table.Rows.Add(product.ProductId, product.Name, product.ProductNumber, product.Color);
            }

            return table;
        }
    }
}
