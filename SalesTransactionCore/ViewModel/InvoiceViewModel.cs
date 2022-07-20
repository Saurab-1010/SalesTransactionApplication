using Microsoft.AspNetCore.Mvc.Rendering;
using SalesTransaction.Model;
using SalesTransactionApplication.Model;

namespace SalesTransactionCore.ViewModel
{
    public class InvoiceViewModel
    {
        public InvoiceViewModel()
        {
            CustomerList = new List<SelectListItem>();
            ProductList = new List<SelectListItem>();
            SalesList = new List<SelectListItem>();
        }

        public int InvoiceId { get; set; }
        public int InvoiceAmount { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
     
        public int SalesAmount { get; set; }
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
    
        public int CustomerId { get; set; }

        public int ProductId { get; set;  }
        public int SalesId { get; set; }

        //public string ProductName { get; set; }
        //public string CustomerName { get; set; }

        public List<SelectListItem> CustomerList { get; set; }
        public List<SelectListItem> ProductList { get; set; }
        public List<SelectListItem> SalesList { get; set; }
    }
}
