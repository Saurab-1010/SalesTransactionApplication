using SalesTransaction.DataAccess;
using SalesTransaction.Interfaces;
using SalesTransaction.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesTransaction.Repositories
{
    public class ProductService : IProductService
    {
        private readonly SalesDBContext _context;
        public ProductService(SalesDBContext context)
        {
            _context = context;
        }

        public Product AddOrEdit(Product product)
        {
            Product ProductDetails = _context.Products.Find(product.ProductId);
            if(product.ProductId > 0)
            {
                if (ProductDetails != null)
                {
                    ProductDetails.ProductName = product.ProductName;
                    ProductDetails.Rate = product.Rate;
                    ProductDetails.AvailableStock = product.AvailableStock;
                    _context.Update(ProductDetails);
                    _context.SaveChanges();
                    return product;
                }
            }
            else
            {
                _context.Add(product);
                _context.SaveChanges();
                return product;
            }
            return product;
        }

        public Product DeleteConfirmed(int id)
        {
            var productModel = _context.Products.Find(id);
            if(productModel != null)
            {
                _context.Products.Remove(productModel);
                _context.SaveChanges();
            }
            return productModel;
        }

        public Product GetProduct(int id)
        {
            Product product = _context.Products.Where(p => p.ProductId == id).FirstOrDefault();
            return product;
        }

        public List<Product> GetProducts()
        {
            List<Product> products = _context.Products.ToList();
            return products;
        }

        //public bool IsProductAlreadyExists( string productName)
        //{
        //    bool result = false; 
        //    try
        //    {
        //        var productList = GetProducts(productName);

        //        var product = _context.Products.Where(x => x.ProductName == productName).FirstOrDefault();
        //        if(product != null)
        //        {
        //            foreach(var data in productList)
        //            {
        //                if(data.ProductName == productName)
        //                {
        //                    result = true;
        //                }
        //            }
        //        }
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}
    }
}
