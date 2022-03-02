using Microsoft.AspNetCore.Mvc;
using WebCore.Extension;
using WebCore.Base;
using System.Web;

namespace CoreReception.Controllers
{
    public class QqController : ApiBaseController
    {
        public virtual IActionResult Reception(string url,string code)
        {
            if(string.IsNullOrEmpty(url))return Content("Error");
            url = HttpUtility.UrlDecode(url);
            url = string.Format("{0}{1}code={2}", url, url.Contains('?') ? "&" : "?", code);
            return this.RedirectUri(url);
        }
    }
}
