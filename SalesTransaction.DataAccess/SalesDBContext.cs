using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesTransaction.Model;
using SalesTransactionApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesTransaction.DataAccess
{
    public class SalesDBContext : IdentityDbContext<ApplicationUser>
    {
        public SalesDBContext()
        {

        }
        public SalesDBContext(DbContextOptions options) : base(options)
        {
             
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sales> Sailing { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
    }
}
