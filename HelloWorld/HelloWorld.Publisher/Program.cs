using RabbitMQ.Client;

using Shared;

using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

namespace HelloWorld.Publisher
{
    internal class Program
    {
        public enum LogNames
        {
            Critical = 1,
            Error = 2,
            Warning = 3,
            Info = 4
        }

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
                channel.QueueDeclare(queue: "hello-queue", durable: true, exclusive: false, autoDelete: false);

                Enumerable.Range(0, 50).ToList().ForEach((x) =>
                {
                    string message = $"Message {x}";

                    var messageBody = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: string.Empty, routingKey: "hello-queue", basicProperties: null, body: messageBody);

                    Console.WriteLine($"Mesaj gönderilmiştir : {message}");
                });

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
                channel.ExchangeDeclare(exchange: "logs-fanout", durable: true, type: ExchangeType.Fanout);

                Enumerable.Range(0, 50).ToList().ForEach((x) =>
                {
                    string message = $"log {x}";

                    var messageBody = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "logs-fanout", routingKey: string.Empty, basicProperties: null, body: messageBody);

                    Console.WriteLine($"Mesaj gönderilmiştir : {message}");
                });

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
                channel.ExchangeDeclare(exchange: "logs-direct", durable: true, type: ExchangeType.Direct);

                Enum.GetNames(typeof(LogNames)).ToList().ForEach((logName) =>
                {
                    var routeKey = $"route-{logName}";
                    var queueName = $"direct-queue-{logName}";
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
                    channel.QueueBind(queue: queueName, exchange: "logs-direct", routingKey: routeKey);
                });

                Enumerable.Range(0, 50).ToList().ForEach((x) =>
                {
                    LogNames randomLogName = (LogNames)new Random().Next(1, 5);

                    string message = $"log-type : {randomLogName}";

                    var messageBody = Encoding.UTF8.GetBytes(message);

                    var routeKey = $"route-{randomLogName}";

                    channel.BasicPublish(exchange: "logs-direct", routingKey: routeKey, basicProperties: null, body: messageBody);

                    Console.WriteLine($"Log gönderilmiştir : {message}");
                });

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

                var random = new Random();
                Enumerable.Range(0, 50).ToList().ForEach((x) =>
                {
                    LogNames randomLogName1 = (LogNames)random.Next(1, 5);
                    LogNames randomLogName2 = (LogNames)random.Next(1, 5);
                    LogNames randomLogName3 = (LogNames)random.Next(1, 5);

                    var routeKey = $"{randomLogName1}.{randomLogName2}.{randomLogName3}";
                    string message = $"log-type : {randomLogName1}-{randomLogName2}-{randomLogName3}";

                    var messageBody = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "logs-topic", routingKey: routeKey, basicProperties: null, body: messageBody);

                    Console.WriteLine($"Log gönderilmiştir : {message}");
                });

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
                channel.ExchangeDeclare(exchange: "header-exchange", durable: true, type: ExchangeType.Headers);

                Dictionary<string, object> headers = new Dictionary<string, object>
                {
                    { "format", "pdf" },
                    { "shape2", "a4" }
                };

                IBasicProperties publishProperties = channel.CreateBasicProperties();
                publishProperties.Headers = headers;

                // Mesajları kalıcı hale getirme
                // publishProperties.Persistent = true;

                var message = "header mesajım";
                var messageBody = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "header-exchange", routingKey: string.Empty, basicProperties: publishProperties, body: messageBody);

                Console.WriteLine($"Mesaj gönderilmiştir : {message}");

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
                channel.ExchangeDeclare(exchange: "header-exchange", durable: true, type: ExchangeType.Headers);

                Dictionary<string, object> headers = new Dictionary<string, object>
                {
                    { "format", "pdf" },
                    { "shape2", "a4" }
                };

                IBasicProperties publishProperties = channel.CreateBasicProperties();
                publishProperties.Headers = headers;
                publishProperties.Persistent = true;

                var product = new Product
                {
                    Id = 1,
                    Name = "kalem",
                    Price = 100,
                    Stock = 10
                };

                var productJsonString = JsonSerializer.Serialize(product);
                var messageBody = Encoding.UTF8.GetBytes(productJsonString);

                channel.BasicPublish(exchange: "header-exchange", routingKey: string.Empty, basicProperties: publishProperties, body: messageBody);

                Console.WriteLine($"Product gönderilmiştir : {productJsonString}");

                Console.ReadLine();
            }
        }
    }
}
