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
    public  class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        //[ForeignKey("Id")]
        //public int ProductId { get; set; }
        //public virtual Product Product { get; set; }
        //public string ProductName { get; set; }


        //[ForeignKey("Id")]
        //public int CustomerId { get; set; }
        //public virtual Customer Customer { get; set; }
        //public string CustomerName { get; set; }

        [ForeignKey("Id")]
        public int SalesId { get; set; }
        public virtual Sales Sales { get; set; }

        public int InvoiceAmount { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }    
        public int ProductId { get; set; }
        public string ProductName { get; set; }

    }
}
