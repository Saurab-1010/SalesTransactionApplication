using jQueryAjax;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesTransaction.DataAccess;
using SalesTransaction.Interfaces;
using SalesTransaction.Model;
using SalesTransactionCore.ViewModel;

namespace SalesTransactionCore.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        public IActionResult Index()
        {

            List<Invoice> invoices= _invoiceService.GetInvoices();
            return View(invoices);

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

        }
        public IActionResult Create(int id = 0)
        {
            InvoiceViewModel model = new InvoiceViewModel();
            var salesData = _invoiceService.GetCustomers();
            foreach (var data in salesData)
            {
                model.CustomerList.Add(new SelectListItem
                {
                    Value = data.CustomerId.ToString(),
                    Text = data.CustomerName
                });
            }
            var ProductData = _invoiceService.GetProducts();
            foreach (var data in ProductData)
            {
                model.ProductList.Add(new SelectListItem
                {
                    Value = data.ProductId.ToString(),
                    Text = data.ProductName

                });
            }
            //var SalesData = _invoiceService.GetSales();
            //foreach (var data in SalesData)
            //{
            //    model.SalesList.Add(new SelectListItem
            //    {
            //        Value = data.SalesId.ToString()
                    

            //    });
            //}
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(InvoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                Invoice invoice = new Invoice();

                invoice.InvoiceId = model.InvoiceId;
                invoice.SalesId = model.SalesId;

                //var SalesData = _invoiceService.GetSales().Find(x => x.SalesId == model.SalesId);
                //invoice.SalesId = model.SalesId;
                //invoice.SalesId = " ";
                //if (SalesData != null)
                //{
                //    invoice.SalesId = SalesData.SalesId != null ? SalesData.SalesId : " ";
                //}

                var customerData = _invoiceService.GetCustomers().Find(x => x.CustomerId == model.CustomerId);
                invoice.CustomerId = model.CustomerId;
                invoice.CustomerName = " ";
                if (customerData != null)
                {
                    invoice.CustomerName = customerData.CustomerName != null ? customerData.CustomerName : " ";
                }

                var productData = _invoiceService.GetProducts().Find(x => x.ProductId == model.ProductId);
                invoice.ProductId = Convert.ToInt32(model.ProductId);
                invoice.ProductName = "";
                if (productData != null)
                {
                    invoice.ProductName = productData.ProductName != null ? productData.ProductName : " ";
                }

                invoice.InvoiceAmount = model.InvoiceAmount;
                invoice.InvoiceDate = model.InvoiceDate;

                var data = _invoiceService.Create(invoice);

                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "Create", data) });
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "Create", model) });
        }
        public IActionResult GenerateInvoice(int? id)
        {

            TempData["idFromView"] = id;
            return View(_invoiceService.GetInvoices().Where(x => x.InvoiceId == id).ToList());
        }


    }

}
