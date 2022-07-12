using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using SalesTransactionApplication.Model;

namespace SalesTransaction.Interfaces
{
    public interface ICustomerService
    {     
        List<Customer> GetCustomers();
        Customer GetCustomer(int id);
        Customer AddOrEdit(Customer customer);
        Customer DeleteConfirmed(int id);

    }
}
