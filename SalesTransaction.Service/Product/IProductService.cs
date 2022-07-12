using SalesTransaction.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesTransaction.Interfaces
{
    public interface IProductService
    {
        List<Product> GetProducts();
        Product GetProduct(int id);
        Product AddOrEdit(Product product);
        Product DeleteConfirmed(int id);
    }
}
