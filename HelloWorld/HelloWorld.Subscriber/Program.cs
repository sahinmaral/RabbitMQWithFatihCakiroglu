using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared;

using System.Text;
using System.Text.Json;

namespace HelloWorld.Subscriber
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // WithoutExchange();
            // FanoutExchange();
            // DirectExchange();
            // TopicExchange();
            // HeaderExchange();
            HeaderExchangeWithComplexType();
        }

        static void WithoutExchange()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                // Global:
                // false ise her subscriber için prefetchCount da ayarlanan değer kadar mesaj gönderilir
                // true ise subscriberlar için toplam prefetchCount da ayarlanan kadar mesaj gönderilir. 
                channel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);
                // channel.QueueDeclare(queue: "hello-queue", durable: true, exclusive: false, autoDelete: false);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "hello-queue", autoAck: false, consumer);

                consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());
                    Console.WriteLine("Gelen mesaj: " + message);

                    Thread.Sleep(1500);

                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                };

                Console.ReadLine();
            }
        }

        static void FanoutExchange()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                // channel.ExchangeDeclare(exchange: "logs-fanout", durable: true, type: ExchangeType.Fanout);

                var randomQueueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: randomQueueName, exchange: "logs-fanout", routingKey: string.Empty);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: randomQueueName, autoAck: false, consumer);

                Console.WriteLine("Loglar dinleniyor");

                consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());
                    Console.WriteLine("Gelen mesaj: " + message);

                    Thread.Sleep(1500);

                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                };

                Console.ReadLine();
            }
        }

        static void FanoutExchangePermanentBind()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                // channel.ExchangeDeclare(exchange: "logs-fanout", durable: true, type: ExchangeType.Fanout);

                var randomQueueName = "log-database-save-queue";
                channel.QueueDeclare(queue: randomQueueName, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(queue: randomQueueName, exchange: "logs-fanout", routingKey: string.Empty);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: randomQueueName, autoAck: false, consumer);

                Console.WriteLine("Loglar dinleniyor");

                consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());
                    Console.WriteLine("Gelen mesaj: " + message);

                    Thread.Sleep(1500);

                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                };

                Console.ReadLine();
            }
        }

        static void DirectExchange()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var desiredQueueName = "direct-queue-Critical";

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: desiredQueueName, autoAck: false, consumer);

                Console.WriteLine("Loglar dinleniyor");

                consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());

                    Thread.Sleep(1500);

                    Console.WriteLine("Gelen mesaj: " + message);
                    // ExportStringToTxtFile("log-critical.txt", message);
                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                };

                Console.ReadLine();
            }
        }

        static void TopicExchange()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: "logs-topic", durable: true, type: ExchangeType.Topic);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var desiredRouteKey = "Info.#";

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName, exchange: "logs-topic", routingKey: desiredRouteKey);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: queueName, autoAck: false, consumer);

                Console.WriteLine("Loglar dinleniyor");

                consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());

                    Thread.Sleep(1500);

                    Console.WriteLine("Gelen mesaj: " + message);
                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                };

                Console.ReadLine();
            }
        }

        static void HeaderExchange()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);  

                var queueName = channel.QueueDeclare().QueueName;

                Dictionary<string, object> headers = new Dictionary<string, object>
                {
                    { "format", "pdf" },
                    { "shape", "a4" },
                    { "x-match", "any" }
                };

                channel.QueueBind(queue: queueName, exchange: "header-exchange", routingKey: string.Empty, arguments: headers);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: queueName, autoAck: false, consumer);

                Console.WriteLine("Mesaj dinleniyor");

                consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());

                    Thread.Sleep(1500);

                    Console.WriteLine("Gelen mesaj: " + message);
                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                };

                Console.ReadLine();
            }
        }

        static void HeaderExchangeWithComplexType()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var queueName = channel.QueueDeclare().QueueName;

                Dictionary<string, object> headers = new Dictionary<string, object>
                {
                    { "format", "pdf" },
                    { "shape", "a4" },
                    { "x-match", "any" }
                };

                channel.QueueBind(queue: queueName, exchange: "header-exchange", routingKey: string.Empty, arguments: headers);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: queueName, autoAck: false, consumer);

                Console.WriteLine("Mesaj dinleniyor");

                consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());

                    var product = JsonSerializer.Deserialize<Product>(message);

                    Thread.Sleep(1500);

                    Console.WriteLine("Gelen product => Id: " + product.Id + " Name: " + product.Name +" Price: " + product.Price +" Stock: " + product.Stock);
                    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                };

                Console.ReadLine();
            }
        }

        static void ExportStringToTxtFile(string fileName, string content)
        {
            File.AppendAllText(fileName, content + "\n");
        }
    }
}
