using SalesTransaction.DataAccess;
using SalesTransaction.Interfaces;
using SalesTransaction.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesTransactionApplication.Model;
using System.Web.Mvc;

namespace SalesTransaction.Repositories
{
    public class CustomerService : ICustomerService
    {
        private readonly SalesDBContext _context;
        public CustomerService(SalesDBContext context)
        {
            _context = context;
        }


        public Customer AddOrEdit(Customer customer)
        {
            Customer customerDetails = _context.Customers.Find(customer.CustomerId);
            if (customer.CustomerId > 0)
            {

                if (customerDetails != null)
                {
                    
                    customerDetails.CustomerName = customer.CustomerName;
                    customerDetails.InsertDate = DateTime.Now;
                    _context.Update(customerDetails);
                    _context.SaveChanges();
                    return customer;
                }
                
            }
            else
            {

                _context.Add(customer);
                 _context.SaveChanges();
                return customer;
            }
            return customer;
        }    

        public List<Customer> GetCustomers()
        {
            List<Customer> customers = _context.Customers.ToList();
            return customers;
        }
        public Customer GetCustomer(int id)
        {
            Customer customer = _context.Customers.Where(p => p.CustomerId == id).FirstOrDefault();
            return customer;
        }

        public Customer DeleteConfirmed(int id)
        {
            var customerModel = _context.Customers.Find(id);
            if(customerModel != null)
            {
                _context.Customers.Remove(customerModel);
                _context.SaveChanges();
            }
            return customerModel;
        }
    }
}
