using BackEnd.Models;
using BackEnd.Models.Repository;
using System;
using System.Web.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using NLog;

namespace BackEnd.Controllers
{
    public class HomeController : Controller
    {
        private ClientRepo clientRepo = new ClientRepo();

        private Logger logger = LogManager.GetCurrentClassLogger();

        private ConnectionFactory factory = new ConnectionFactory();

        [HttpPost]
        public int PostOrder(Queue queue)
        {
            try
            {
                var order = JsonConvert.DeserializeObject<Order>((GetMessageFromRabbit(queue.QueueName)));
                order.Status = GetStatus();

                LogRequest(order);

                return (clientRepo.PostOrder(order));
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw ex;
            }
        }
        [HttpGet]
        public string GetOrder(string queue)
        {
            try
            {
                var parameters = JsonConvert.DeserializeObject<GetOrderModel>(GetMessageFromRabbit(queue));

                LogRequest(parameters);

                return JsonConvert.SerializeObject(clientRepo.GetOrder(parameters.Order_id));
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw ex;
            }
        }
        [HttpGet]
        public string GetOrders(string queue)
        {
            try
            {
                var parameters = JsonConvert.DeserializeObject<GetOrdersModel>(GetMessageFromRabbit(queue));

                LogRequest(parameters);

                var orders = JsonConvert.SerializeObject(clientRepo.GetOrders(parameters.Client_id, parameters.Department_address));

                return orders;
            }

            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw ex;
            }
        }

        private Status GetStatus()
        {
            var random = new Random();
            return (Status)random.Next(1, 4);
        }

        private string GetMessageFromRabbit(string queue)
        {
            string originalMessage = "";
            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

                    var consumer = new EventingBasicConsumer(model);
                    BasicGetResult result = model.BasicGet(queue, true);
                    if (result != null)
                    {
                        originalMessage =
                        Encoding.UTF8.GetString(result.Body);
                    }
                }
            }
            return originalMessage;
        }

        private void LogRequest<T>(T cri)
        {
            logger.Info("{@clientRequest}", cri);
        }
    }
}