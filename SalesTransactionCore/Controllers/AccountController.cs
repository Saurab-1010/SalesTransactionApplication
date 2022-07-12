using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesTransaction.Model;
using System.Security.Claims;

namespace SalesTransactionCore.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult AllUser()
        {          
            return View();
        }
    }
      
}
