using jQueryAjax;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesTransaction.DataAccess;
using SalesTransaction.Interfaces;
using SalesTransactionApplication.Model;

namespace SalesTransactionCore.Controllers
{
    public class CustomerController : Controller
    {
        //private readonly SalesDBContext _context;
        private readonly ICustomerService _customerService;
        
        public CustomerController(/*SalesDBContext context,*/ ICustomerService customerService)
        {
            //_context = context; 
            _customerService = customerService;
        }
        public IActionResult Index()
        {
            List<Customer> customers = _customerService.GetCustomers();
            return View(customers);
        }
       // GET
        public IActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Customer());
            }
            else
            {
                var customer = _customerService.GetCustomer(id);
                if (customer == null)
                {
                    return NotFound();
                }
                return View(customer);
            }
        }
        [HttpPost]
        public IActionResult AddOrEdit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                //bool isEdit = false;
                //if (customer.CustomerId > 0)
                //{
                //    isEdit = true; 
                //}

                bool isEdit = customer.CustomerId > 0 ? true : false;
                  
                var data = _customerService.AddOrEdit(customer);

                if (isEdit == true)
                {
                    TempData["success"] = "Customer Edited Succesfully";
                    return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "AddOrEdit", data) });
                }
                else
                {


                    List<Customer> customers = new List<Customer>();
                    customers = _customerService.GetCustomers();
                    TempData["success"] = "Customer Created Succesfully";
                    return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", _customerService.GetCustomers().ToList()) });
                }

            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", customer) });
        }




        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {                          
            var data = _customerService.DeleteConfirmed(id);
           
            if (data != null)
            {
            
                List<Customer> customers = new List<Customer>();
                customers = _customerService.GetCustomers();

           
            }
            TempData["success"] = "Customer Deleted Succesfully";
            return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", data) });
            //return RedirectToAction(nameof(Index));
        }
    }
    
    
}
