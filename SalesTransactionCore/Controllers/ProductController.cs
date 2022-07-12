using jQueryAjax;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesTransaction.DataAccess;
using SalesTransaction.Model;

namespace SalesTransactionCore.Controllers
{
    public class ProductController : Controller
    {
        private readonly SalesDBContext _context;
        public ProductController(SalesDBContext context)
        {
            _context = context;
        }   
        public async Task<IActionResult> Index()
        {
            return _context.Products != null ?
                       View(await _context.Products.ToListAsync()) :
                      Problem("Entity set 'ProductsDBContext.Transactions'  is null.");
        }

        //GET
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Product());
            }
            else
            {
                var product = await _context.Customers.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEdit(int id, [Bind("ProductId, ProductName, Rate, AvailableStock")] Product product)
        {
            if (ModelState.IsValid)
            {
                if (id == 0)
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    try
                    {
                        _context.Update(product);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProductExist(product.ProductId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Products.ToList()) });
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", product) });
        }

        private bool ProductExist(int id)
        {
            return (_context.Products?.Any(c => c.ProductId == id)).GetValueOrDefault();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Bankings'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Product Deleted Succesfully";
            return RedirectToAction(nameof(Index));
            //return PartialView("_ViewAll");
        }

    }
}
