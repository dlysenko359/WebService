using System.Collections.Generic;

namespace BackEnd.Models
{
    public class Client
    {
        public int Id { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}