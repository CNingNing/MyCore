using System;
using System.Text;
using System.Web;
using Component.Extension;
using Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Winner.Base;

namespace WebCore.Extension
{
    static public class CodeHelper
    {
   

        #region 客户端验证码
        /// <summary>
        /// 加密至
        /// </summary>
        private static string GetCodeKey()
        {
            var key =Winner.Creator.Get<ISecurity>().EncryptMd5(HttpContextHelper.Current().Request.GetClientIp());
            return key.Left(24);
        }

        /// <summary>
        /// 得到验证码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string EncryptCode(string code)
        {
            return Winner.Creator.Get<ISecurity>().Encrypt3Des(code, GetCodeKey());
        }

            /// <summary>
            /// 得到验证码
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
        public static IActionResult CreateCode(string name)
        {
            var code = new StringBuilder();
            var random = new Random();
            for (int i = 0; i < 4; i++)
            {
                code.Append(random.Next(0, 9));
            }
            var bytes = Winner.Creator.Get<IComponent>().CreateCodeImage(code.ToString());
            HttpContextHelper.Current().Response.Cookies.Append(name, EncryptCode(code.ToString()));
            return new FileContentResult(bytes, "image/jpeg");
        }


        /// <summary>
        /// 验证码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool ValidateCode(string code, string name)
        {
            if (!HttpContextHelper.Current().Request.Cookies.ContainsKey(name))
                return false;
            var cookie = HttpContextHelper.Current().Request.Cookies[name];
            if (cookie == null) return false;
            var value = Winner.Creator.Get<ISecurity>().Decrypt3Des(cookie, GetCodeKey());
            HttpContextHelper.Current().Response.Cookies.Append(name, "", new CookieOptions {  Path = "/", Secure = false, HttpOnly = false, SameSite = SameSiteMode.Unspecified, Expires = new DateTimeOffset(DateTime.Now.AddDays(-1)) });
            HttpContextHelper.Current().Response.Cookies.Delete(name);
            return value == code;
        }
        const int ErrorCount = 1000;
        private const string CodeErrorName = "CodeError";
        /// <summary>
        /// 初始化验证码
        /// </summary>
        /// <param name="name"></param>
        public static int InitilzeCodeErrorCount(string name=null)
        {
            name = string.IsNullOrWhiteSpace(name) ? CodeErrorName : name;
            if (!HttpContextHelper.Current().Request.Cookies.ContainsKey(name))
            {
                HttpContextHelper.Current().Response.Cookies.Append(name, Winner.Creator.Get<ISecurity>().Encrypt3Des(ErrorCount.ToString(), GetCodeKey()));
                return ErrorCount;
            }
            return GetCodeErrorCount();
        }

        /// <summary>
        /// 初始化验证码
        /// </summary>
        /// <param name="name"></param>
        public static int AddCodeErrorCount(string name = null)
        {
            name = string.IsNullOrWhiteSpace(name) ? CodeErrorName : name;
            var value = GetCodeErrorCount();
            value = value>= ErrorCount ? ErrorCount : value+1;
            var val = Winner.Creator.Get<ISecurity>().Encrypt3Des(value.ToString(), GetCodeKey());
            HttpContextHelper.Current().Response.Cookies.Append(name, val,new CookieOptions{Expires = new DateTimeOffset(DateTime.Now.AddYears(10))});
            return value;
        }

        /// <summary>
        /// 初始化验证码
        /// </summary>
        /// <param name="name"></param>
        public static void ResetCodeErrorCount(string name = null)
        {
            name = string.IsNullOrWhiteSpace(name) ? CodeErrorName : name;
            var val = Winner.Creator.Get<ISecurity>().Encrypt3Des("0", GetCodeKey());
            HttpContextHelper.Current().Response.Cookies.Append(name, val, new CookieOptions { Expires = new DateTimeOffset(DateTime.Now.AddYears(10)) });
        }

        /// <summary>
        /// 得到错误数量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetCodeErrorCount(string name = null)
        {
            name = string.IsNullOrWhiteSpace(name) ? CodeErrorName : name;
            if (!HttpContextHelper.Current().Request.Cookies.ContainsKey(name))
                return ErrorCount;
            var cookie = HttpContextHelper.Current().Request.Cookies[name];
            if (cookie == null)
                return ErrorCount;
            var value= Winner.Creator.Get<ISecurity>().Decrypt3Des(cookie, GetCodeKey());
            int i;
            if (int.TryParse(value, out i))
                return i;
            return ErrorCount;
        }
        #endregion

    }
}
