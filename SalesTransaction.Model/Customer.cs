using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesTransactionApplication.Model
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime InsertDate { get; set; } = DateTime.Now;
    }
}
