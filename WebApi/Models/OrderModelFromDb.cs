namespace WebApi.Models
{
    public class OrderModelFromDb
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }
}