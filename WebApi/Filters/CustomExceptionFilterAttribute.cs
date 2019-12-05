using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace WebApi.Filters
{
    public class CustomExceptionFilter: ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is FormatException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Data you provided is not valid.")
                };
            }
        }
    }
}