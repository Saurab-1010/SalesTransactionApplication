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
    public class InvoiceService : IInvoiceService
    {
        private readonly SalesDBContext _context;
        public InvoiceService(SalesDBContext context)
        {
            _context = context;
        }
 
        public Invoice GetInvoice(int id)
        {
            Invoice invoice = _context.Invoices.Where(s => s.InvoiceId == id).FirstOrDefault();
            return invoice;
        }

        public List<Invoice> GetInvoices()
        {
            List<Invoice> invoices = _context.Invoices.ToList();
            return invoices;
        }

        public List<Sales> GetSales()
        {
            List<Sales> sales= _context.Sailing.ToList();
            return sales;
        }
        public List<Customer> GetCustomers()
        {
            List<Customer> customers = _context.Customers.ToList();
            return customers;
        }


        public Invoice Create(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            _context.SaveChanges();
            return invoice;
        }

        public List<Product> GetProducts()
        {
            List<Product> products = _context.Products.ToList();
            return products;
        }
    }
}
