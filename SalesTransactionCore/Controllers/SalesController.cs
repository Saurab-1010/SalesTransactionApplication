using jQueryAjax;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalesTransaction.DataAccess;
using SalesTransaction.Model;
using SalesTransactionCore.ViewModel;

namespace SalesTransactionCore.Controllers
{
    public class SalesController : Controller
    {
        private readonly SalesDBContext _context;
        public SalesController(SalesDBContext context)
        {
            _context = context; 
        }
        public async Task<IActionResult> Index()
        {
            return _context.Sailing != null ?
                View(await _context.Sailing.ToListAsync()) :
                Problem("Entity set 'SalesDbContext.Sailing' is null");

            //List<SalesViewModel> svmList = new List<SalesViewModel>();
            //var data = (from s in _context.Sailing
            //            join p in _context.Products on s.ProductId equals p.ProductId
            //            join c in _context.Customers on s.CustomerId equals c.CustomerId
            //            select new
            //            {
            //                SalesId = s.SalesId,
            //                CustomerId = s.CustomerId,
            //                CustomerName = c.CustomerName,
            //                ProductId = s.ProductId,
            //                ProductName = p.ProductName,
            //                Quantity = s.Quantity,
            //                Rate = s.Rate,
            //                InserDate = s.InserDate,
            //            }).ToList();
            //foreach (var item in data)
            //{
            //    svmList.Add(new SalesViewModel()
            //    {
            //        SalesId = item.SalesId,
            //        CustomerId = item.CustomerId,
            //        CustomerName = item.CustomerName,
            //        ProductId = item.ProductId,
            //        ProductName = item.ProductName,
            //        Quantity = item.Quantity,
            //        Rate = item.Rate,
            //        InserDate = item.InserDate,
            //    });
            //}
            //return View(svmList);

        }
        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            SalesViewModel model = new SalesViewModel();
            var salesData = _context.Customers;
            foreach (var data in salesData)
            {
                model.CustomerList.Add(new SelectListItem
                {
                    Value = data.CustomerId.ToString(),
                    Text = data.CustomerName
                });
            }
            var ProductData = _context.Products;
            foreach (var data in ProductData)
            {
                model.ProductList.Add(new SelectListItem
                {
                    Value = data.ProductId.ToString(),
                    Text = data.ProductName
                });
            }
            if(id == 0)
            return View(model);
            else
            {
                var salesModel = await _context.Sailing.FindAsync(id);
                if(salesModel != null)
                {
                    //model.SalesId = salesModel.SalesId;
                    model.CustomerId = salesModel.CustomerId;
                    model.ProductId = salesModel.ProductId;
                    model.Rate = salesModel.Rate;
                    model.Quantity = salesModel.Quantity;
                    //model.Total = salesModel.Total;
                    model.InserDate = salesModel.InserDate;
                    //model.ProductName = salesModel.ProductName;
                    //model.CustomerName = salesModel.CustomerName;
                }
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(SalesViewModel model)
        {          
            if(ModelState.IsValid)
            {
                Sales sales = new Sales();
                if (model.SalesId == 0)
                {
                    sales.CustomerId = Convert.ToInt32(model.CustomerId);
                    sales.CustomerName = _context.Customers.Find(model.CustomerId).CustomerName;
                    sales.ProductId = Convert.ToInt32(model.ProductId);
                    sales.ProductName = _context.Products.Find(model.ProductId).ProductName;
                    //model.SalesId = sales.SalesId;
                    sales.Rate = model.Rate;
                    sales.Quantity = model.Quantity;
                    //model.Total = sales.Total;
                    sales.InserDate = model.InserDate;

                    _context.Add(sales);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    try
                    {
                        _context.Update(model);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SalesModelExists(model.SalesId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Sailing.ToList()) });
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", model) });
            //return View(model);

        }
        private bool SalesModelExists(int id)
        {
            return (_context.Sailing?.Any(e => e.SalesId == id)).GetValueOrDefault();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Sailing == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Bankings'  is null.");
            }
            var sales = await _context.Sailing.FindAsync(id);
            if (sales != null)
            {
                _context.Sailing.Remove(sales);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Sales Deleted Succesfully";
            return RedirectToAction(nameof(Index));
            //return PartialView("_ViewAll");
        }
        public async Task<IActionResult> Invoice(int? id)
        {
            TempData["idFromView"] = id;
            return View(_context.Sailing.Where(x => x.SalesId == id).ToList());           

        }
    }
}
