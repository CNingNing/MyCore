using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebCore.Base;
using WebCore.Base.Auth;
using System.Net;
using Component.Extension;
using System.Text.RegularExpressions;
using WebCore.Extension;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace CoreLogin.Controllers
{
    
    public class LoginController : BaseController
    {
        public const string Name = "nprong";
        public const string Password = "19930917";
        public const string Redirecturl = "";
        public virtual IDictionary<string,string> QQ
        {
            get { return Configuration.ConfigurationManager.GetSetting<string>("QQ")?.DeserializeJson<IDictionary<string, string>>() ?? new Dictionary<string, string>(); }
        }
        public virtual string QQAppId
        {
            get {return QQ?.Get("AppId")??""; }
        }
        public virtual string QQAppSecret
        {
            get { return QQ?.Get("AppSecret")??""; }
        }


        [AllowAnonymous]
        public virtual async Task<IActionResult> Index()
        {
            var identityPrincipal = HttpContext.User;
            if(identityPrincipal == null  || identityPrincipal.Claims==null || !identityPrincipal.Claims.Any())
            {
                return View();
            }
            if (identityPrincipal.Identity == null || !identityPrincipal.Identity.IsAuthenticated)
            {
                return View();
            }
            if(identityPrincipal.Identity.AuthenticationType!="Ticket")
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return View();
            }
            var loginName = identityPrincipal.Claims.FirstOrDefault(it => it.Type == ClaimTypes.Name)?.Value;
            var loginPassword = identityPrincipal.Claims.FirstOrDefault(it => it.Type == ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrWhiteSpace(loginName) || string.IsNullOrWhiteSpace(loginPassword))
            {
                return View();
            }
            if (loginName != Name || loginPassword != Password)
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [AllowAnonymous]
        public virtual async Task<IActionResult> CheckLogin()
        {
            var dic=new Dictionary<string, object>();
            var loginName = Request.Get("LoginName");
            var loginPassword = Request.Get("LoginPassword");
            if(string.IsNullOrEmpty(loginName) || string .IsNullOrWhiteSpace(loginPassword))
            {
                dic.Add("Status", false);
                dic.Add("Message", "账号或密码不可为空！");
                return this.Jsonp(dic);
            }
            if (loginName != Name || loginPassword != Password)
            {
                dic.Add("Status", false);
                dic.Add("Message", "账号或密码错误，请重新输入！");
                return RedirectToAction("Index", "Login");
            }
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.Name,loginName),
                new Claim(ClaimTypes.NameIdentifier,loginPassword)
            };
            var identity = new ClaimsIdentity(claim, "Ticket");
            var identityPrincipal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, identityPrincipal, new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(10),
                IsPersistent = false,
                AllowRefresh = false
            });
            return RedirectToAction("Index", "Home");

        }
        [AllowAnonymous]
        public  virtual async Task<IActionResult> CheckLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
           

        #region QQ登陆
        /// <summary>
        /// 打开qq授权页面
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult QqAuthorize()
        {
            var url = string.Format(
                     "https://graph.qq.com/oauth2.0/authorize?response_type=code&client_id={0}&redirect_uri={1}&state=State",
                     QQAppId, WebUtility.UrlEncode(Redirecturl));
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
            return RedirectToAction("Index", "Home");
        }


        public virtual string? GetAuthorityAccessToken(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;
            var url =
                string.Format(
                    "https://graph.qq.com/oauth2.0/token?client_id={0}&client_secret={1}&code={2}&grant_type=authorization_code&redirect_uri={3}",
                    QQAppId, QQAppSecret, code, Redirecturl);
            HttpWebRequest? request = WebRequest.Create(url) as HttpWebRequest;
            var json = WebRequestHelper.GetResponse(request, "utf-8");
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
            var json = WebRequestHelper.GetResponse(request, "utf-8");
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
            var url = $"https://graph.qq.com/user/get_user_info?access_token={token}&openid={openId}&oauth_consumer_key={QQAppId}";
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            var json = WebRequestHelper.GetResponse(request, "utf-8");
            var dis = json.DeserializeJson<Dictionary<string, string>>();
            if (dis.ContainsKey("ret") && dis["ret"] != "0")
                return null;
            return dis;
        }
        #endregion

    }
}
