using Microsoft.AspNetCore.Mvc;
using SalesTransaction.DataAccess;
using SalesTransactionCore.ViewModel;

namespace SalesTransactionCore.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly SalesDBContext _context;
        public InvoiceController(SalesDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            //List<InvoiceViewModel> ivmList = new List<InvoiceViewModel>();
            //return View(ivmList);
            //List<InvoiceViewModel> ivmList = new List<InvoiceViewModel>();
            //var data = (from s in _context.Invoices
            //            join c in _context.Customers on s.CustomerId equals c.CustomerId
            //            select new
            //            {
            //                InvoiceId = s.InvoiceId,
            //                CustomerId = s.CustomerId,
            //                CustomerName = c.CustomerName,
            //                InvoiceAmount = s.InvoiceAmount,
            //                InvoiceDate = s.InvoiceDate

            //            }).ToList();
            //foreach (var item in data)
            //{
            //    ivmList.Add(new InvoiceViewModel()
            //    {
            //        InvoiceId = item.InvoiceId,
            //        CustomerId = item.CustomerId,
            //        CustomerName = item.CustomerName,
            //        InvoiceAmount = item.InvoiceAmount,
            //        InvoiceDate = item.InvoiceDate

            //    });
            //}
            //return View(ivmList);

            List<InvoiceViewModel> ivmList = new List<InvoiceViewModel>();
            var data = (from s in _context.Invoices
                        join c in _context.Customers on s.CustomerId equals c.CustomerId
                        select new
                        {
                            InvoiceId = s.InvoiceId,
                            CustomerId = s.CustomerId,
                            CustomerName = c.CustomerName,
                            InvoiceAmount = s.InvoiceAmount,
                            InvoiceDate = s.InvoiceDate

                        }).ToList();
            foreach (var item in data)
            {
                ivmList.Add(new InvoiceViewModel()
                {
                    InvoiceId = item.InvoiceId,
                    CustomerId = item.CustomerId,
                    CustomerName = item.CustomerName,
                    InvoiceAmount = item.InvoiceAmount,
                    InvoiceDate = item.InvoiceDate

                });
            }
            return View(ivmList);

        }
        public async Task<IActionResult> GenerateInvoice()
        {
           
            return View();
        }
    }
    
}
