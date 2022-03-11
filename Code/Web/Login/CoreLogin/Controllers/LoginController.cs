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
using Component.ThirdPartySdk;
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
        protected const string Name = "nprong";
        protected const string Password = "19930917";
        private readonly string _redirecturl = $"{Configuration.ConfigurationManager.GetSetting<string>("CoreReception")}/qq/reception?returnlogin={Configuration.ConfigurationManager.GetSetting<string>("CoreLogin")}/login/qqlogin";
        protected virtual IDictionary<string,string> QQ
        {
            get { return Configuration.ConfigurationManager.GetSetting<string>("QQ")?.DeserializeJson<IDictionary<string, string>>() ?? new Dictionary<string, string>(); }
        }
        protected virtual string QQAppId
        {
            get {return QQ?.Get("AppId")??""; }
        }
        protected virtual string QQAppSecret
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
            await SetLoginStatus(loginName, loginPassword);
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
            var qq = new QqSdk(QQAppId, QQAppSecret);
            return new RedirectResult(qq.QqAuthorize(_redirecturl));
        }
        /// <summary>
        /// QQ回掉方法
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> QQLogin()
        {
            var code = Request.Query["code"];
            var qq = new QqSdk(QQAppId, QQAppSecret);
            var token = qq.GetToken(code, _redirecturl);
            if(string.IsNullOrEmpty(token))
            {
                return Content("获取token失败!");
            }
            var result=qq.GetAuthorityOpendIdAndUnionId(token);
            if(result==null)
            {
                return Content("获取openId失败!");
            }
            var dic=result.DeserializeJson<Dictionary<string, object>>();
            var openId = dic.Get("openid")?.ToString() ?? "";
            var unionId = dic.Get("unionid")?.ToString() ?? "";
            await SetLoginStatus(openId, unionId);
            var userInfo = qq.GetUserInfo(token, openId);
            return RedirectToAction("Index", "Home");
        }

       
        #endregion

        protected virtual async Task SetLoginStatus(string loginName,string loginPassword)
        {
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
        }



    }
}
