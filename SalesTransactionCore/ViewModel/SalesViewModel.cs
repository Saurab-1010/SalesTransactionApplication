using Microsoft.AspNetCore.Mvc.Rendering;
using SalesTransaction.Model;
using SalesTransactionApplication.Model;

namespace SalesTransactionCore.ViewModel
{
    public class SalesViewModel
    {
        public SalesViewModel()
        {
            CustomerList = new List<SelectListItem>();
            ProductList = new List<SelectListItem>();
        }
        public int SalesId { get; set; }
        public int ProductId { get; set; }
        //public virtual Product Product { get; set; }

        //public string ProductName { get; set; }
       // public string CustomerName { get; set; }
        public int CustomerId { get; set; }

        public int InvoiceId { get; set; }

        //public virtual Customer Customers { get; set; }
        public int Rate { get; set; }
        public int Quantity { get; set; }
        public decimal Total
        {
            get
            {
                return (Rate * Quantity);
            }
        }
        //public Decimal Total { get; }
        public DateTime InserDate { get; set; }


        public List<SelectListItem> CustomerList { get; set; }
        public List<SelectListItem> ProductList { get; set; }


    }
}
