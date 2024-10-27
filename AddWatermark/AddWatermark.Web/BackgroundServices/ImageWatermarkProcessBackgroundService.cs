
using AddWatermark.Web.Services;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Drawing;
using System.Text;
using System.Text.Json;

namespace AddWatermark.Web.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
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

            _channel.BasicConsume(
                queue: RabbitMQClientService.QueueName,
                autoAck: false,
                consumer
                );

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var bodyByte = @event.Body.ToArray();
                var bodyString = Encoding.UTF8.GetString(bodyByte);
                var imageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(bodyString);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageCreatedEvent.ImageName);

                var siteName = "www.mysite.com";

                using var image = Image.FromFile(path);
                using var graphic = Graphics.FromImage(image);
                var font = new Font(FontFamily.GenericMonospace, 40, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = graphic.MeasureString(siteName, font);
                var textColor = Color.FromArgb(128, 255, 255, 255);
                var brush = new SolidBrush(textColor);

                var position = new Point(image.Width - ((int)textSize.Width + 30), image.Height - ((int)textSize.Height + 30));
                
                graphic.DrawString(siteName, font, brush, position);

                image.Save("wwwroot/images/watermarks/" + imageCreatedEvent.ImageName);

                image.Dispose();
                graphic.Dispose();

                _channel.BasicAck(deliveryTag: @event.DeliveryTag, multiple: false);
            }
            catch (Exception)
            {
                throw;
            }

            return Task.CompletedTask;

        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
