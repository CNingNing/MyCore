using Microsoft.AspNetCore.Mvc;
using WebCore.Extension;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace WebCore.Base
{

    public class BaseController : Controller
    {
        //public override void OnActionExecuting(ActionExecutingContext context)
        //{
        //    if (ViewBag.Identity == null)
        //        ViewBag.Identity = Identity;
        //    base.OnActionExecuting(context);
        //}

        /// <summary>
        /// 没有找到视图
        /// </summary>
        /// <param name="message"></param>
        /// <param name="viewPath"></param>
        /// <returns></returns>
        public virtual IActionResult None(string message, string viewPath = "/Views/Shared/none.cshtml")
        {
            ViewBag.Message = message;
            return View(viewPath);            
        }

        //private bool _isGetIdentity;
        //private IdentityEntity _identity;
        ///// <summary>
        ///// 身份验证
        ///// </summary>
        //public virtual IdentityEntity Identity
        //{
        //    get
        //    {
        //        if (!_isGetIdentity)
        //        {
        //            _identity = HttpContextHelper.Current().GetIdentity();
        //            _isGetIdentity = true;
        //        }
        //        return _identity;
        //    }
        //}

      

        /// <summary>
        /// 得到文件名
        /// </summary>
        /// <param name="path"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        protected virtual string GetViewPath(string path, string style)
        {
            if (string.IsNullOrEmpty(style))
                return path;
            var viewName = $"{style}/{path}";
            var services = ControllerContext.HttpContext.RequestServices;
            var executor = services.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var isSuccess = executor?.FindView(ControllerContext, viewName, true).Success;
            return isSuccess==true ? viewName : path;
        }

        /// <summary>
        /// 得到语言
        /// </summary>
        /// <returns></returns>
        protected virtual string GetLanguage()
        {
            string lang;
            HttpContext.Request.Cookies.TryGetValue("language", out lang);
            return lang == "cn" ? "" : lang;
        }
    }
}
