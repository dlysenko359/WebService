namespace BackEnd.Models
{
    public class Order
    {
       public string Department_address { get; set; }
       public double Amount { get; set; }
       public string Currency { get; set; }
       public int Client_id { get; set; }
       public string Client_ip { get; set; }
       public Status Status  { get; set; }
    }
}