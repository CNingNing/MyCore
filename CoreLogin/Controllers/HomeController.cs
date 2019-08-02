using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreLogin.Models;

namespace CoreLogin.Controllers
{
    
    public class HomeController : Controller
    {
        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }
     
    }
}
