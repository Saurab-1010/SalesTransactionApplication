using SalesTransaction.Model;
using SalesTransactionApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesTransaction.Interfaces
{
    public interface ISalesService
    {
      
        List<Sales> GetSales();
        Sales GetSale(int id);
        Sales AddOrEdit(Sales sales);
        Sales DeleteConfirmed(int id);

        List<Customer> GetCustomers();
        List<Product> GetProducts();
    }
}
