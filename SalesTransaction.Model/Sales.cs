using SalesTransactionApplication.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesTransaction.Model
{
    public  class Sales
    {
        [Key]
        public int SalesId { get; set; }

        [ForeignKey("Id")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string ProductName { get; set;  }

        [ForeignKey("Id")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public string CustomerName { get; set; }

        [ForeignKey("Id")]
        public int? InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; }
        //public string CustomerName { get; set; }

        public int Rate { get; set; }
        public int Quantity { get; set; }
        public decimal Total
        {
            get
            {
                return (Rate * Quantity);
            }
        }
        public DateTime InserDate { get; set; }
    }
}
