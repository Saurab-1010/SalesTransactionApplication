using jQueryAjax;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesTransaction.DataAccess;
using SalesTransactionApplication.Model;

namespace SalesTransactionCore.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly SalesDBContext _context;
        
        public CustomerController(SalesDBContext context)
        {
            _context = context; 
        }
        public async Task<IActionResult> Index()
        {
            return _context.Customers != null ?
                View(await _context.Customers.ToListAsync()) :
                Problem("Entity set 'SalesDBContext.Customers' is null.");
        }
        //GET
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if(id == 0)
            {
                return View(new Customer());
            }
            else
            {
                var customer = await _context.Customers.FindAsync(id);
                if(customer == null)
                {
                    return NotFound();
                }
                return View(customer);
            }
        }
        [HttpPost]
        public async Task<ActionResult> AddOrEdit(int id, [Bind("CustomerId, CustomerName, InsertDate")] Customer customer)
        {
            if(ModelState.IsValid)
            {
                if(id == 0)
                {
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    try
                    {
                        _context.Update(customer);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CustomerExist(customer.CustomerId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Customers.ToList()) });
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", customer) });
        }

        private bool CustomerExist(int id)
        {
            return (_context.Customers?.Any(c => c.CustomerId == id)).GetValueOrDefault();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Customers == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Bankings'  is null.");
            }
            var bankingModel = await _context.Customers.FindAsync(id);
            if (bankingModel != null)
            {
                _context.Customers.Remove(bankingModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



    }
}
