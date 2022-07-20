using jQueryAjax;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalesTransaction.DataAccess;
using SalesTransaction.Interfaces;
using SalesTransaction.Model;
using SalesTransactionCore.ViewModel;

namespace SalesTransactionCore.Controllers
{
    //[Authorize]
    public class SalesController : Controller
    {
        private readonly ISalesService _salesService;
        public SalesController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        public IActionResult Index()
        {
            List<Sales> sales = _salesService.GetSales();
            return View(sales);            
        }

        public IActionResult AddOrEdit(int id = 0)
        {
            SalesViewModel model = new SalesViewModel();
            var salesData = _salesService.GetCustomers();
            foreach (var data in salesData)
            {
                model.CustomerList.Add(new SelectListItem
                {
                    Value = data.CustomerId.ToString(),
                    Text = data.CustomerName
                });
            }
            var ProductData = _salesService.GetProducts();
            foreach (var data in ProductData)
            {
                model.ProductList.Add(new SelectListItem
                {
                    Value = data.ProductId.ToString(),
                    Text = data.ProductName,
                   
                }) ;

                    model.ProductRate.Add(new SelectListItem
                    {
                        Value = data.ProductId.ToString(),
                        Text = data.Rate.ToString()

                    });


               

                //model.ProductList.Add(new ProductSelectItem
                //{
                //    ProductId = data.ProductId,
                //    ProductName = data.ProductName,
                //    Rate = data.Rate
                //});
            }
       
            if(id == 0)
            return View(model);
            else
            {
                var salesModel = _salesService.GetSale(id);
                if(salesModel != null)
                {
                    model.SalesId = salesModel.SalesId;
                    model.CustomerId = salesModel.CustomerId;
                    //model.CustomerName = salesModel.CustomerName;
                    model.ProductId = salesModel.ProductId;
                    //model.ProductName = salesModel.ProductName;
                    model.Rate = salesModel.Rate;
                    model.Quantity = salesModel.Quantity;
                    model.InserDate = salesModel.InserDate;
                }
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrEdit(SalesViewModel model)
        {
            if (ModelState.IsValid)
            {
                Sales sales = new Sales();
             

                sales.SalesId = Convert.ToInt32(model.SalesId);

                var productData = _salesService.GetProducts().Find(x => x.ProductId == model.ProductId);
                sales.ProductId = Convert.ToInt32(model.ProductId);
                sales.ProductName = "";
                if(productData != null)
                {
                    sales.ProductName = productData.ProductName != null ? productData.ProductName : " ";
                }
                
                var customerData = _salesService.GetCustomers().Find(x => x.CustomerId == model.CustomerId);
                sales.CustomerId = Convert.ToInt32(model.CustomerId);
                sales.CustomerName = "";
                if(customerData != null)
                {
                    sales.CustomerName = customerData.CustomerName != null ? customerData.CustomerName : "";
                }
                sales.Rate = model.Rate;
                sales.Quantity = model.Quantity;
                sales.InserDate = model.InserDate;
    

                bool isEdit = sales.SalesId > 0 ? true : false;
                var data = _salesService.AddOrEdit(sales);

                if (isEdit == true)
                {
                    TempData["success"] = "Sales Edited Succesfully";
                    return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "AddOrEdit", data) });
                }
                else
                {
                    List<Sales> sales1 = new List<Sales>();
                    sales1 = _salesService.GetSales();
                    TempData["success"] = "Sales Created Succesfully";
                    return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", _salesService.GetSales().ToList()) });
                }
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", model) });
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var data = _salesService.DeleteConfirmed(id);
            if(data != null)
            {
                List<Sales> sales = new List<Sales>();
                sales = _salesService.GetSales();
              
            }
            TempData["success"] = "Sales Deleted Succesfully";
            return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", data) });
            //return RedirectToAction(nameof(Index));


        }

        public IActionResult Invoice(int? id)
        {
            TempData["idFromView"] = id;
            return View(_salesService.GetSales().Where(x => x.SalesId == id).ToList());

        }
    }
}
