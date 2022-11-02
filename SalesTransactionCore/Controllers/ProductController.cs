using BoostChampsPlatform.Services.Services.ExcelHelper;
using ClosedXML.Excel;
using jQueryAjax;
using Microsoft.AspNetCore.Mvc;
using SalesTransaction.DataAccess;
using SalesTransaction.Interfaces;
using SalesTransaction.Model;

namespace SalesTransactionCore.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly SalesDBContext _db;
        private readonly IExcelHelper _excelHelper;
        public ProductController(IProductService productService, SalesDBContext db, IExcelHelper excelHelper)
        {
            _productService = productService;
            _db = db;
            _excelHelper = excelHelper;
        }
        public IActionResult Index()
        {
            //List<Product> products = _productService.GetProducts();
            //return View(products);
            IEnumerable<Product> objProductList = _productService.GetProducts();
            return View(objProductList);
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
        public IActionResult DeleteConfirmed(int id)
        {
            var data = _productService.DeleteConfirmed(id);
            if (data != null)
            {
                List<Product> products = new List<Product>();
                products = _productService.GetProducts();

            }
            TempData["success"] = "Product Deleted Succesfully";
            return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", data) });
            //return RedirectToAction(nameof(Index));


        }

        [HttpPost]
        public IActionResult ImportExcel(IFormFile file)
        {
            try
            {
                var fileextension = Path.GetExtension(file.FileName);
                var filename = Guid.NewGuid().ToString() + fileextension;
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedExcelFile", filename);
                using (FileStream fs = System.IO.File.Create(filepath))
                {
                    file.CopyTo(fs);
                }
                int rowno = 1;

                XLWorkbook workbook = XLWorkbook.OpenFromTemplate(filepath);
                var sheets = workbook.Worksheets.First();
                var rows = sheets.Rows().ToList();

                foreach (var row in rows)
                {
                    if (rowno != 1)
                    {
                        var productName = row.Cell(1).Value.ToString();
                        if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrEmpty(productName))
                        {
                            break;
                        }
                        Product product;
                        product = _db.Products.Where(p => p.ProductName == row.Cell(1).Value.ToString()).FirstOrDefault();
                        if (product == null)
                        {
                            product = new Product();
                        }
                        product.ProductName = row.Cell(1).Value.ToString();
                        product.Rate = Convert.ToInt32(row.Cell(2).Value);
                        product.AvailableStock = Convert.ToInt32(row.Cell(3).Value);
                        if (product.ProductId == null)
                        {
                            _db.Products.Add(product);
                            TempData["success"] = "Product Added Succesfully";


                        }
                        else
                            _db.Products.Update(product);
                        TempData["success"] = "Product Updated Succesfully";
                    }
                    else
                    {
                        rowno = 2;
                    }

                }
                _db.SaveChanges();
                System.IO.File.Delete(@filepath);

            }
            catch (Exception e)
            {
                throw e;
            }
            return View();
        }


        [HttpPost]
        public IActionResult ExportExcel(int ProductId)
        
        {
            try
            {
                var productDetails = _productService.GetProduct(ProductId);
                var productList = _productService.GetProduct(ProductId);
                var targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExportedExcelFile");
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
                var filename = productDetails.ProductName.Replace(' ', '_') + "_" + DateTime.Now.ToShortDateString().Replace('/', '_').Replace(' ', '_').Replace(':', '_') + ".xlsx";
                if (!Directory.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
                var savePath = Path.Combine(targetDirectory, filename);
                //_excelHelper.ListToExcel<>(productDetails, savePath); 
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }
    }
}
