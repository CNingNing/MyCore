using Microsoft.AspNetCore.Mvc;
using WebCore.Extension;
using WebCore.Base;
using System.Web;

namespace CoreReception.Controllers
{
    public class QqController : ApiBaseController
    {
        public virtual IActionResult Reception(string returnlogin, string code)
        {
            if(string.IsNullOrEmpty(returnlogin))return Content("Error");
            returnlogin = HttpUtility.UrlDecode(returnlogin);
            returnlogin = string.Format("{0}{1}code={2}", returnlogin, returnlogin.Contains('?') ? "&" : "?", code);
            return new RedirectResult(returnlogin);
        }
    }
}
