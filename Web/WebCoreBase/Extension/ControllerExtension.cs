using System;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Configuration;
using Newtonsoft.Json;
using Winner.Persistence;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace WebCore.Extension
{
    static public class ControllerExtension
    {

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="controller"></param>
        public static IActionResult GetDefaultActionResult(this Controller controller)
        {
            return new RedirectResult(string.IsNullOrEmpty(controller.HttpContext.Request.Get("url"))
                                          ? ConfigurationManager.GetSetting<string>("DefaultUrl")
                                          : HttpUtility.UrlDecode(controller.HttpContext.Request.Get("url")));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="data"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static JsonResult Jsonp(this Controller controller, object data, JsonSerializerSettings settings = null)
        {
            if (settings == null)
                return new JsonResult(data, new Newtonsoft.Json.JsonSerializerSettings());
            JsonResult result = new JsonResult(data, settings);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static RedirectResult RedirectUri(this Controller controller, string url)
        {
            if (url == null)
                return new RedirectResult(url);
            url = Uri.EscapeDataString(url);
            RedirectResult result = new(url);
            return result;
        }


        /// <summary>
        /// 静态页面地址
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static string GetUrl(this Controller controller, string siteName)
        {
            return ConfigurationManager.GetSetting<string>(siteName);
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static string GetLogoFileName(this Controller controller)
        //{
        //    var info = controller.LoadRouter();
        //    if (info == null || string.IsNullOrWhiteSpace(info.LogoFileName))
        //        return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/logo.png";
        //    return info.LogoFullFileName;

        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static string GetFaviconFileName(this Controller controller)
        //{
        //    var info = controller.LoadRouter();
        //    if (info == null || string.IsNullOrWhiteSpace(info.FaviconFileName))
        //        return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/favicon.ico";
        //    return info.FaviconFullFileName;

        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static string GetDefaultFileName(this Controller controller)
        //{
        //    var info = controller.LoadRouter();
        //    if (info == null || string.IsNullOrWhiteSpace(info.DefaultFileName))
        //        return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/default.png";
        //    return info.DefaultFullFileName;

        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <param name="fullFileName"></param>
        ///// <returns></returns>
        //public static string GetDefaultFileName(this Controller controller, string fullFileName)
        //{
        //    if (!string.IsNullOrWhiteSpace(fullFileName))
        //        return fullFileName;
        //    var info = controller.LoadRouter();
        //    if (info == null || string.IsNullOrWhiteSpace(info.DefaultFileName))
        //        return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/default.png";
        //    return info.DefaultFullFileName;

        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static string GetPrerenderFileName(this Controller controller)
        //{
        //    var info = controller.LoadRouter();
        //    if (info == null || string.IsNullOrWhiteSpace(info.DefaultFileName))
        //        return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/prerender.gif";
        //    return info.PrerenderFullFileName;

        //}
        ///// <summary>
        ///// 静态页面地址
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static string GetSiteName(this Controller controller)
        //{
        //    var info = controller.LoadRouter();
        //    if (info == null || string.IsNullOrWhiteSpace(info.Name))
        //        return ConfigurationManager.GetSetting<string>("DomainName");
        //    return info.Name;
        //}
        ///// <summary>
        ///// 静态页面地址
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static string GetIcpName(this Controller controller)
        //{
        //    var info = controller.LoadRouter();
        //    if (info == null || string.IsNullOrWhiteSpace(info.Name))
        //        return ConfigurationManager.GetSetting<string>("DomainIcp");
        //    return info.Name;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public static RouterEntity LoadRouter(this Controller controller, string domain = null)
        //{
        //    var info = controller.ViewBag.Router as RouterEntity;
        //    if (info != null)
        //        return info;
        //    controller.ViewBag.Router = controller.HttpContext.GetRouter(domain);
        //    return info;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static string GetLanguage(this Controller controller)
        {
            return controller.HttpContext.Request.Cookies.ContainsKey("language") ? controller.HttpContext.Request.Cookies["language"] : null;
        }
    }
}
