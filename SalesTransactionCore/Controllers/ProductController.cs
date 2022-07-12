using jQueryAjax;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesTransaction.DataAccess;
using SalesTransaction.Interfaces;
using SalesTransaction.Model;

namespace SalesTransactionCore.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }   
        public IActionResult Index()
        {
            List<Product> products = _productService.GetProducts();
            return View(products);
        }

        //GET
        public IActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Product());
            }
            else
            {
                var product = _productService.GetProduct(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
        }
        [HttpPost]
        public ActionResult AddOrEdit(Product product)
        {
            if (ModelState.IsValid)
            {
                bool isEdit = product.ProductId > 0 ? true : false;
                var data = _productService.AddOrEdit(product);

                if (isEdit == true)
                {
                    TempData["success"] = "Product Edited Succesfully";
                    return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "AddOrEdit", data) });
                }
                else
                {
                    List<Product> products = new List<Product>();
                    products = _productService.GetProducts();
                    TempData["success"] = "Product Created Succesfully";
                    return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", data) });
                }
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", product) });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var data = _productService.DeleteConfirmed(id);
            if( data != null)
            {
                List<Product> products = new List<Product>();
                products = _productService.GetProducts();

            }
            TempData["success"] = "Product Deleted Succesfully";
            return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", data) });
            //return RedirectToAction(nameof(Index));


        }

    }
}
