using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using WebCore.Base;

namespace CoreLogin.Controllers
{
    public class HomeController : BaseController
    {
        
        public IActionResult Index()
        {
            
            return View();
        }
    }
}
