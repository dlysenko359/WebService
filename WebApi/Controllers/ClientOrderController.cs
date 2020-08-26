using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http;
using NLog;
using WebApi.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WebApi.Filters;
using System.Net;
using WebApi.Common;

namespace WebApi.Controllers
{
    [CustomExceptionFilter]
    public class ClientOrderController : ApiController
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        private Rabbit messageBroker = new Rabbit();
        private Validator validator = new Validator();

        [HttpPost]
        public async Task<IHttpActionResult> PostOrderAsync(HttpRequestMessage request)
        {
            try
            {
                string queue = "PostClientOrderQueue";

                string jsonContent = request.Content.ReadAsStringAsync().Result;

                var clientRequestInfo = JsonConvert.DeserializeObject<ClientRequestInfo>(jsonContent);

                clientRequestInfo.Client_ip = GetClientIp(request);

                LogRequest(clientRequestInfo);

                validator.IsClientOrderValid(clientRequestInfo);

                messageBroker.SendMassegeToRabbit(JsonConvert.SerializeObject(clientRequestInfo), queue);

                var response = await Task.Run(() => PostOrder(queue));


                return Ok(Int32.Parse(response));
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw ex;
            }

        }

        [HttpGet]
        public async Task<IHttpActionResult> GetOrderAsync(int id)
        {
            try
            {
                string queue = "GetClientOrderQueue";

                var requestInfo = new GetOrderModel() { Order_id = id, Client_ip = GetClientIp() };

                if (requestInfo.Order_id < 1)
                {
                    throw new FormatException();
                }

                LogRequest(requestInfo);

                messageBroker.SendMassegeToRabbit(JsonConvert.SerializeObject(requestInfo), queue);

                var response = JsonConvert.DeserializeObject<OrderModelFromDb>(await Task.Run(() => GetOrder(queue)));
                if (response == null)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw ex;
            }

        }

        [HttpGet]
        public async Task<IHttpActionResult> GetOrdersAsync(int client_id, string department_address)
        {
            try
            {
                string queue = "GetClientOrdersQueue";

                var requestInfo = new GetOrdersModel() { Client_id = client_id, Department_address = department_address, Client_ip = GetClientIp() };

                if (requestInfo.Client_id < 1)
                {
                    throw new FormatException();
                }

                LogRequest(requestInfo);

                messageBroker.SendMassegeToRabbit(JsonConvert.SerializeObject(requestInfo), queue);

                var response = JsonConvert.DeserializeObject<List<OrderModelFromDb>>(await Task.Run(() => GetOrders(queue)));
                if (response.Count == 0)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw ex;
            }
        }


        private string GetClientIp(HttpRequestMessage request = null)
        {
            request = request ?? Request;

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)this.Request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }


        private async Task<string> GetOrder(string queue)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44362");

                var result = await client.GetAsync("/Home/GetOrder/?queue=" + queue);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    return resultContent;
                }
                throw CustomException(result);

            }
        }

        private async Task<string> GetOrders(string queue)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44362");

                var result = await client.GetAsync("/Home/GetOrders/?queue=" + queue);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    return resultContent;
                }
                throw CustomException(result);
            }
        }

        private async Task<string> PostOrder(string queue)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44362");

                var content = new Queue()
                {
                    QueueName = queue
                };

                var result = await client.PostAsJsonAsync("/Home/PostOrder", content);
                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    return resultContent;
                }
                throw CustomException(result);
            }
        }
        private void LogRequest<T>(T cri)
        {
            logger.Info("{@clientRequest}", cri);
        }
        private void LogException(Exception ex)
        {
            logger.Error(ex, ex.Message);
        }

        public HttpResponseException CustomException(HttpResponseMessage message)
        {
            return new HttpResponseException(Request.CreateErrorResponse(message.StatusCode, message.ReasonPhrase));
        }

    }
}
