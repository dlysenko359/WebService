namespace WebApi.Models
{
    public class ClientRequestInfo
    {
        public int Client_id {get;set;}
        public string Department_address {get;set;}
        public decimal Amount {get;set;}
        public string Currency {get;set;}
        public string Client_ip {get;set;}
    }
}