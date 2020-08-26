using RabbitMQ.Client;
using System.Text;

namespace WebApi.Common
{
    public class Rabbit
    {
        public void SendMassegeToRabbit(string request, string queue)
        {
            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

                    var body = Encoding.UTF8.GetBytes(request);

                    model.BasicPublish(exchange: string.Empty, routingKey: queue, body: body);
                }
            }
        }

        private ConnectionFactory factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
        };
    }
}