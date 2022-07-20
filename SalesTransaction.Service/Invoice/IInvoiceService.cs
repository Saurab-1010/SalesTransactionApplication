using SalesTransaction.Model;
using SalesTransactionApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesTransaction.Interfaces
{
    public interface IInvoiceService
    {
        List<Invoice> GetInvoices();
        Invoice GetInvoice(int id);
        Invoice Create(Invoice invoice);
        List<Customer> GetCustomers();
        List<Sales> GetSales();
        List<Product> GetProducts();
    }
}
