using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreLogin.Models;
using System.Net;
using Component.Extension;
using System.Text.RegularExpressions;

namespace CoreLogin.Controllers
{
   
    
    public class HomeController : Controller
    {
        public const string appId = "";
        public const string appSecret = "";
        public const string redirecturl = "";
        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }
        #region QQ登陆
        /// <summary>
        /// 打开qq授权页面
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult QqAuthorize()
        {
          var url=string.Format(
                   "https://graph.qq.com/oauth2.0/authorize?response_type=code&client_id={0}&redirect_uri={1}&state=State",
                   appId, WebUtility.UrlEncode(redirecturl));
            return new RedirectResult(url);
        }
        /// <summary>
        /// QQ回掉方法
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult QQLogin()
        {
            var code = Request.Query["code"];
            var token = GetAuthorityAccessToken(code);
            var dis = GetAuthorityOpendIdAndUnionId(token);
            var userInfo = GetUserInfo(token, dis["openid"]);
            return null;
        }


        public virtual string? GetAuthorityAccessToken(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;
            var url =
                string.Format(
                    "https://graph.qq.com/oauth2.0/token?client_id={0}&client_secret={1}&code={2}&grant_type=authorization_code&redirect_uri={3}",
                    appId, appSecret, code, redirecturl);
            HttpWebRequest? request = WebRequest.Create(url) as HttpWebRequest;
            var json =WebRequestHelper.GetResponse(request, "utf-8");
            if (string.IsNullOrEmpty(json))
                return null;
            if (!json.Contains("access_token"))
            {
                return null;
            }
            var dis = json.Split('&').Where(it => it.Contains("access_token")).FirstOrDefault();
            var accessToken = dis?.Split('=')[1];
            return accessToken;
        }
        /// <summary>
        /// 获取OpenId和UnionId
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Dictionary<string, string> GetAuthorityOpendIdAndUnionId(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            var url = $"https://graph.qq.com/oauth2.0/me?access_token={token}&unionid=1";
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            var json =WebRequestHelper.GetResponse(request, "utf-8");
            if (string.IsNullOrEmpty(json) || json.Contains("error") || !json.Contains("callback"))
                return null;
            Regex reg = new Regex(@"\(([^)]*)\)");
            Match m = reg.Match(json);
            var dis = m.Result("$1").DeserializeJson<Dictionary<string, string>>();
            return dis;
        }
        /// <summary>
        /// 获取用户的基本信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        public virtual Dictionary<string, string> GetUserInfo(string token, string openId)
        {
            if (string.IsNullOrEmpty(token)) return null;
            var url = $"https://graph.qq.com/user/get_user_info?access_token={token}&openid={openId}&oauth_consumer_key={appId}";
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            var json =WebRequestHelper.GetResponse(request, "utf-8");
            var dis = json.DeserializeJson<Dictionary<string, string>>();
            if (dis.ContainsKey("ret") && dis["ret"] != "0")
                return null;
            return dis;
        }
        #endregion

    }
}
