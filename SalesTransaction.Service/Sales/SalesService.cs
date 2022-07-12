using SalesTransaction.DataAccess;
using SalesTransaction.Interfaces;
using SalesTransaction.Model;
using SalesTransactionApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesTransaction.Repositories
{
    public class SalesService : ISalesService
    {
        private readonly SalesDBContext _context;
        public SalesService(SalesDBContext context)
        {
            _context = context;
        }

       

        public Sales AddOrEdit(Sales sales)
        {
            Sales salesDetails = _context.Sailing.Find(sales.SalesId);
            if (sales.SalesId > 0)
            {
                if (salesDetails != null)
                {
                    salesDetails.CustomerId = Convert.ToInt32(sales.CustomerId);
                    salesDetails.CustomerName = _context.Customers.Find(sales.CustomerId).CustomerName;  //sales.CustomerName;
                    salesDetails.ProductId = Convert.ToInt32(sales.ProductId);
                    salesDetails.ProductName = _context.Products.Find(sales.ProductId).ProductName;
                    salesDetails.Rate = sales.Rate;
                    salesDetails.Quantity = sales.Quantity;
                    sales.InserDate = sales.InserDate;
                    _context.Update(salesDetails);
                    _context.SaveChanges();
                    return sales;

                }
            }
            else
            {
                _context.Add(sales);
                _context.SaveChanges();
                return sales;
            }
            return sales;
        }

        public Sales DeleteConfirmed(int id)
        {
            var salesModel = _context.Sailing.Find(id);
            if(salesModel != null)
            {
                _context.Sailing.Remove(salesModel);
                _context.SaveChanges();
             
            }

            return salesModel;
        }

      

        public Sales GetSale(int id)
        {
            Sales sales = _context.Sailing.Where(s => s.SalesId == id).FirstOrDefault();
            return sales;
        }

        public List<Sales> GetSales()
        {
            List<Sales> sales = _context.Sailing.ToList();
            return sales; 
        }

        public List<Customer> GetCustomers()
        {
            List<Customer> customers = _context.Customers.ToList();
            return customers;
        }

        public List<Product> GetProducts()
        {
            List<Product> products = _context.Products.ToList();
            return products;
        }
    }
}
