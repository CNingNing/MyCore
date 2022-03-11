using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Component.Extension;
using Configuration;
//using Dependent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Winner.Persistence;

namespace WebCore.Extension
{

    public static class HttpContextHelper
    {
        #region 扩展方法 
        public static IServiceProvider ServiceProvider { get; set; }

        public static IWebHostEnvironment HostingEnvironment { get; set; }
        public static void Register(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = int.MaxValue;
                options.ValueLengthLimit = int.MaxValue;
                options.KeyLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
                options.MultipartBoundaryLengthLimit = int.MaxValue;
            }).Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
                .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
       
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                            {
                               "image/svg+xml",
                               "application/atom+xml",
                               "application/x-www-form-urlencoded"
                            });
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.AddControllersWithViews().AddNewtonsoftJson().AddRazorRuntimeCompilation();
        }

        public static void Initialize(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ServiceProvider = app.ApplicationServices;
            HostingEnvironment = env;
            RequestHelper.RequestLanguageHandle = GetLanguage;
            RequestHelper.RequestFormHandle = GetRequestForm;
            RequestHelper.RequestCookieHandle = GetCookies;
            RequestHelper.RequestBodyHandle = GetRequestBody;
            RequestHelper.RequestUrlHandle = GetRequestUrl;
            RequestHelper.RequestDomainHandle = GetRequestDomain;
            RequestHelper.RequestIpHandle = GetRequestIp;
            RequestHelper.RequestIdentityTikcetHandle = GetRequestIdentityTicket;
            var jspath = GetResourceJsPath();
            if(!string.IsNullOrWhiteSpace(jspath))
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider=new PhysicalFileProvider(jspath),
                    RequestPath="/js"
                });
            }
            app.UseResponseCompression();
            //if(Configuration.ConfigurationManager.GetSetting<bool>("IsPathBase") && !string.IsNullOrWhiteSpace(StartupHelper.SubSystemUrl))
            //{
            //    var path = StartupHelper.SubSystemUrl.Replace("Presentation","");
            //    if(path.Length>3)
            //    {
            //        PathBase = $"/{path.Substring(0, path.Length - 3).ToLower()}";
            //        app.UsePathBase(PathBase);

            //    }
            //}
        }
        private static string GetResourceJsPath()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (true)
            {
                var temp = new DirectoryInfo(Path.Combine(dir.FullName, "js"));
                if (temp.Exists)
                {
                    return temp.FullName;
                }
                dir = dir.Parent;
                if (dir == null)
                    break;
            }
            if(ConfigurationManager.GetSetting<bool>("IsDebug"))
            {
                dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (true)
                {
                    var temp = new DirectoryInfo(Path.Combine(dir.FullName, "Infrastructure/Configuration/js"));
                    if (temp.Exists)
                    {
                        return temp.FullName;
                    }
                    dir = dir.Parent;
                    if (dir == null)
                        break;
                }
            }
            return null;
        }
        static string PathBase { get; set; }

        public static string GetPathBase()
        {
            return PathBase;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetRequestUrl()
        {
            return Current()?.Request?.Url();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetRequestIdentityTicket()
        {
            return Current().GetIdentityTicket();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetRequestIp()
        {
            return Current()?.Request?.GetClientIp();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetThirdPartyLoginName(string appId,string thirtyPatyId)
        {
           return Winner.Creator.Get<Winner.Base.ISecurity>().EncryptMd5($"{appId}:{thirtyPatyId}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string,string> GetRequestForm()
        {
            return Current()?.Request.GetForms();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRequestBody()
        {
            return Current()?.Request.GetBodyBytes();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static CookieCollection GetCookies()
        {
            return Current()?.Request.GetCookies();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetLanguage()
        {
            string lang = Current()?.Request?.Get("language");
            if (string.IsNullOrWhiteSpace(lang))
            {
                var cookies = Current()?.Request?.Cookies;
                if (cookies == null)
                    return "";
                cookies.TryGetValue("language", out lang);
            }
            
            return lang == "cn" ? "" : lang;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetRequestDomain()
        {
            return Current()?.Request?.GetDomain();
        }
       
        public static HttpContext Current()
        {
            object factory = ServiceProvider.GetService(typeof(IHttpContextAccessor));
            return ((IHttpContextAccessor)factory)?.HttpContext;
        }
        #region request
        //获取当前页面Url
        public static string Url(this HttpRequest request)
        {
            var content = Current();
            var scheme = content.Request.Scheme;
            var host = request?.Host.Host?.ToLower();
            if (string.IsNullOrWhiteSpace(host))
                return null;
            var key = ConfigurationManager.GetSetting<string>(host);
            var configUrl = string.IsNullOrWhiteSpace(key) ? null : ConfigurationManager.GetSetting<string>(key);
            if (!string.IsNullOrWhiteSpace(configUrl) && configUrl.Contains("https://"))
                scheme = "https";
            var port = !content.Request.Host.Port.HasValue || content.Request.Host.Port == 80 || content.Request.Host.Port == 443 ? "" : $":{content.Request.Host.Port }";
            return $"{scheme}://{content.Request.Host.Host}{ port}{Current().Request.Path}{content.Request.QueryString}";
        }
        public static string UserAgent(this HttpRequest request)
        {

            return Current().Request.Headers["User-Agent"].Count > 0 ? Current().Request.Headers["User-Agent"][0] : null;
        }

        //获取当前页面Url
        public static string Get(this HttpRequest request, string key)
        {
            if (!string.IsNullOrWhiteSpace(request.Query[key]))
                return request.Query[key];
            if (Current().Request.HasFormContentType)
            {
                Current().Request.ReadFormAsync().Wait();
                return request.Form[key];
            }
            return null;
        }
        public static T Get<T>(this HttpRequest request, string key)
        {
            return request.Get(key).Convert<T>();
        }
        public static ICollection<string> AllKeys(this HttpRequest request)
        {
            var keys = new List<string>();
            foreach (var key in Current().Request.Query.Keys)
            {
                keys.Add(key);
            }
            if (Current().Request.HasFormContentType)
            {
                Current().Request.ReadFormAsync().Wait();
                foreach (var key in Current().Request.Form.Keys)
                {
                    keys.Add(key);
                }
            }
            return keys;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request.Get<string>("x-requested-with") == "XMLHttpRequest")
                return true;
            if (request.Headers == null || !request.Headers.ContainsKey("x-requested-with"))
                return false;
            return request.Headers["x-requested-with"] == "XMLHttpRequest";

        }
        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, string> GetForms(this HttpRequest request)
        {
            if (request == null)
                return null;
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in request.Query.Keys)
            {
                if (result.ContainsKey(key))
                {
                    result[key] = $"{result[key]},{request.Query[key]}";
                }
                else
                {
                    result.Add(key, request.Query[key]);
                }
            }

            if (request.HasFormContentType)
            {
                request.ReadFormAsync().Wait();
                foreach (var key in request.Form.Keys)
                {
                    if (result.ContainsKey(key))
                    {
                        result[key] = $"{result[key]},{request.Form[key]}";
                    }
                    else
                    {
                        result.Add(key, request.Form[key]);
                    }
                }
            }

            return result;

        }
        private static readonly Regex MobileBrowserMatch1 = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static readonly Regex MobileBrowserMatch2 = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static bool IsMobileBrowser(this HttpRequest request)
        {
            var userAgent = request.UserAgent();
            if (!string.IsNullOrWhiteSpace(userAgent) && (MobileBrowserMatch1.IsMatch(userAgent) || MobileBrowserMatch2.IsMatch(userAgent.Substring(0, 4))))
            {
                return true;
            }

            return false;
        }

        public static bool CheckWechatBrower(this HttpRequest request)
        {
            var userAgent = request.Headers["User-Agent"].Count > 0 ? Current().Request.Headers["User-Agent"][0] : null;
            if (userAgent == null || userAgent.ToLower().Contains("micromessenger"))
                return true;
            return false;
        }

        public static bool CheckDingTalkBrower(this HttpRequest request)
        {
            var userAgent = request.Headers["User-Agent"].Count > 0 ? Current().Request.Headers["User-Agent"][0] : null;
            if (userAgent == null || userAgent.ToLower().Contains("dingtalk"))
                return true;
            return false;
        }
        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns>
        public static string GetClientIp(this HttpRequest request)
        {
            if (request == null)
                return null;
            var ip= request.Headers["X-Forwarded-For"].Count > 0 ? Current().Request.Headers["X-Forwarded-For"][0] : null;
            return string.IsNullOrWhiteSpace(ip)? request.HttpContext.Connection.RemoteIpAddress.ToString():ip;
        }
        #endregion

        public static string MapPath(this HttpContext context, string path)
        {
            return $"{Path.GetDirectoryName(HostingEnvironment.WebRootPath)}{path}";
        }


        public const string TicketKey = "ticket";
        public const string TicketName = "ticketName";

        /// <summary>
        /// 加密至
        /// </summary>
        private static string Domain
        {
            get { return ConfigurationManager.GetSetting<string>("Domain"); }
        }

        public static string GetTicketName(this HttpContext context)
        {
            if (!string.IsNullOrWhiteSpace(context?.Request?.Get(TicketName)))
                return context.Request.Get(TicketName);
            return Current().Request.Cookies.ContainsKey(TicketName) ? Current().Request.Cookies[TicketName] : TicketKey;
        }
        public static string CreateTicketName(this HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(context?.Request?.Get("TicketName")))
                return TicketKey;
            return context.Request.Get("TicketName");
        }


        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns>
        //public static string CreateIdentityTicket(this HttpContext context, IdentityEntity identity,string loginType)
        //{
        //    if (identity == null || identity.IsShareTicket)
        //        return null;
        //    var key = Winner.Creator.Get<Winner.Base.ISecurity>().EncryptMd5($"IdentityForceQuit{loginType}{identity.Id}");
        //    identity.TicketId = Guid.NewGuid().ToString("N");
        //    //Ioc.Resolve<ICacheApplicationService>().Set(key, identity.TicketId, 60);
        //    SendIdentityWebSocketMessage(context, identity, new {TicketId= identity.TicketId, Type="CheckForceLogout" }.SerializeJson());
        //    return identity.TicketId;
        //}

        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns>
        //public static bool CheckQuit(this HttpContext context, string loginType)
        //{
        //    var identity = context.GetIdentity();
        //    if (identity == null || identity.IsShareTicket)
        //        return false;
        //    var key=Winner.Creator.Get<Winner.Base.ISecurity>().EncryptMd5($"IdentityForceQuit{loginType}{identity.Id}");
        //    var value= Ioc.Resolve<ICacheApplicationService>().Get<string>(key);
        //    if(string.IsNullOrWhiteSpace(value))
        //        return false;
        //    var rev= value!= identity.TicketId;
        //    if (rev)
        //        context.RemoveIdentity();
        //    return rev;
        //}

        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns>
        public static string GetIdentityTicket(this HttpContext context,string ticketName=null)
        {
            if (Current() == null)
                return null;
            ticketName = ticketName?? GetTicketName(context);
            if (!string.IsNullOrWhiteSpace(Current().Request.Query[ticketName]))
                return Current().Request.Query[ticketName];
            if (Current().Request.HasFormContentType)
            {
                Current().Request.ReadFormAsync().Wait();
                if (!string.IsNullOrWhiteSpace(Current().Request.Form[ticketName]))
                    return Current().Request.Form[ticketName];
            }
            return Current().Request.Cookies.ContainsKey(ticketName) ? Current().Request.Cookies[ticketName] : null;

        }

        /// <summary>
        /// 设置token
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public static bool SetIdentityTicket(this HttpContext context, string ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket) || Current() == null)
                return false;
            var request = Current()?.Request;
            var domain = request?.GetDomain();
            var ticketName = CreateTicketName(context);
            if(ticketName!=TicketKey)
            {
                Current().Response.Cookies.Append(TicketName, HttpUtility.UrlEncode(ticketName), new CookieOptions { Domain = domain, Path = "/", Secure = false, HttpOnly = false, SameSite = SameSiteMode.Unspecified });
            }
            else
            {
                Current().Response.Cookies.Delete(TicketName);
            }
            Current().Response.Cookies.Append(ticketName, HttpUtility.UrlEncode(ticket), new CookieOptions { Domain = domain, Path = "/", Secure = false, HttpOnly = false, SameSite = SameSiteMode.Unspecified });
            return true;
        }
       

        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns>
        //public static TokenEntity SetIdentity(this HttpContext context, IdentityEntity identity)
        //{
        //    var token = Ioc.Resolve<IIdentityApplicationService>().Set(identity);
        //    SetIdentityTicket(context, identity.TicketId);
        //    return token;
        //}

        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns>
        //public static IdentityEntity GetIdentity(this HttpContext context,string ticketName=null)
        //{
        //    var ticket = GetIdentityTicket(context, ticketName);
        //    if (string.IsNullOrWhiteSpace(ticket))
        //        return null;
        //    var identity= Ioc.Resolve<IIdentityApplicationService>().Get<IdentityEntity>(ticket);
        //    if (identity != null)
        //        identity.TicketId = ticket;
        //    return identity;
        //}
        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns> 
        //public static bool RemoveIdentity(this HttpContext context, string ticketName = null)
        //{
        //    var ticket = GetIdentityTicket(context, ticketName);
        //    if (string.IsNullOrWhiteSpace(ticket))
        //        return true;
        //    var rev= Ioc.Resolve<IIdentityApplicationService>().Remove(ticket);
        //    var domain = Current().Request?.GetDomain();
        //    ticketName = ticketName ?? GetTicketName(context);
        //    Current().Response.Cookies.Delete(TicketName);
        //    Current().Response.Cookies.Delete(ticketName);
        //    Current().Response.Cookies.Append(TicketName, HttpUtility.UrlEncode(ticket), new CookieOptions { Domain = domain, Path = "/", Expires = DateTime.Now.AddDays(-1), Secure = false, HttpOnly = false, SameSite = SameSiteMode.Unspecified });
        //    Current().Response.Cookies.Append(ticketName, HttpUtility.UrlEncode(ticket), new CookieOptions { Domain = domain, Path = "/",Expires=DateTime.Now.AddDays(-1), Secure = false, HttpOnly = false, SameSite = SameSiteMode.Unspecified });
        //    return rev;
        //}

        /// <summary>
        /// 设置token
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetCookie(this HttpContext context,string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || Current() == null)
                return false;
            var domain = Current()?.Request?.GetDomain();
            Current().Response.Cookies.Append(name, HttpUtility.UrlEncode(value), new CookieOptions { Domain = domain, Path = "/", Secure = false, HttpOnly = false, SameSite = SameSiteMode.Unspecified });
            return true;
        }

        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns>
        public static string GetCookie(this HttpContext context,string name)
        {
            return Current().Request.Cookies.ContainsKey(name) ? Current().Request.Cookies[name] : null;

        }
        #endregion

        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns> 
        public static byte[] GetBodyBytes(this HttpRequest request)
        {
            if (request.Body == null)
                return null;
            request.EnableBuffering();
            request.Body.Position = 0;
            using (MemoryStream memstream = new MemoryStream())
            {
                int bytesRead = 0;
                byte[] buffer = new byte[65530];
                while ((bytesRead = request.Body.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memstream.Write(buffer, 0, bytesRead);
                }
                return memstream.ToArray();
            }
        }
        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns> 
        public static CookieCollection GetCookies(this HttpRequest request)
        {
            if (request.Cookies == null)
                return null;
            var cookies = new CookieCollection();
            foreach (var cookie in request.Cookies)
            {
                cookies.Add(new Cookie(cookie.Key, cookie.Value));
            }
            return cookies;
        }
        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns> 
        public static void RemoveCookie(this HttpContext httpContent,string name)
        {
            var domain = httpContent.Request?.GetDomain();
            httpContent.Response.Cookies.Append(name, "", new CookieOptions { Domain = domain, Path = "/", Secure = false, HttpOnly = false, SameSite = SameSiteMode.Unspecified, Expires = new DateTimeOffset(DateTime.Now.AddDays(-1)) });
        }
        /// <summary>
        /// 得到客户端地址
        /// </summary>
        /// <returns></returns> 
        public static string GetDomain(this HttpRequest request)
        {
            var domain = ConfigurationManager.Settings.Get("Domain");
            var host = request.Host.Host;
            if (!string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(host) && !host.Contains(domain.ToLower()))
            {
                var newDomain = Component.Extension.RequestHelper.GetDomain(host);
                if (string.IsNullOrWhiteSpace(newDomain))
                {
                    return domain;
                }
                return newDomain;
            }
            return domain;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public static RouterEntity GetRouter(this HttpContext httpContent,string newDomain=null)
        //{
        //    var domain = ConfigurationManager.GetSetting<string>("Domain");
        //    var host = httpContent.Request.Host.Host;
        //    newDomain =string.IsNullOrWhiteSpace(newDomain)? httpContent.Request?.GetDomain(): $"{newDomain}.com";
        //    if (domain == newDomain)
        //        return null;
        //    var identity = httpContent.GetIdentity();
        //    var outNumber = $"Hr{identity?.GetNumber<string>("HrId")}";
        //    var query = new QueryInfo<RouterEntity>();
        //    query.AppendCacheDependency<RouterEntity>().SetCacheType(CacheType.LocalAndRemote).Query()
        //        .Where(it => (it.Domain == newDomain || it.OutNumber==outNumber) && it.IsUsed).Select(it => it);
        //    var info = EntityControllerExtension.GetEntities<RouterEntity>(null, query)?.FirstOrDefault();
        //    return info;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContent"></param>
        /// <param name="url"></param>
        /// <param name="checkHttps"></param>
        /// <returns></returns>
        public static string GetTicketUrl(this HttpContext httpContent, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;
            if (url.StartsWith("Presentation"))
            {
                var values = url.Split('/');
                values[0] = Configuration.ConfigurationManager.GetSetting<string>(values[0]);
                url = string.Join("/", values);
            }
            var lowerUrl = url.ToLower();
            //if (checkHttps && url.StartsWith("https"))
            //    return url;
            var ticketName = httpContent.GetTicketName().ToLower();
            var result = new StringBuilder();
            bool isAppendParam = false;
            if (lowerUrl.Contains(ticketName) || lowerUrl.Contains(TicketName.ToLower()))
            {
                var vals = url.Split('?');
                result.Append(vals[0]);
                if (vals.Length > 1)
                {
                    var paramters = vals[1].Split('&');
                    foreach (var paramter in paramters)
                    {
                        var pvalues = paramter.Split('=');
                        if (pvalues.Length != 2 || pvalues[0].ToLower() == ticketName || pvalues[0].ToLower() == TicketName.ToLower())
                            continue;
                        if (isAppendParam == false)
                        {
                            result.Append("?");
                            isAppendParam = true;
                        }
                        else {
                            result.Append("&");
                        }
                        result.Append(paramter);
                    }

                }
            }
            else
            {
                isAppendParam = url.Contains("?");
                result.Append(url);
            }
            var ticket = httpContent.GetIdentityTicket(ticketName);
            result.Append(isAppendParam ? "&" : "?");
            result.Append($"{ticketName}={ticket}");
            if (ticketName != HttpContextHelper.TicketKey)
                result.Append($"&ticketName={ticketName}");
            return result.ToString();
        }
        //public static void SendIdentityWebSocketMessage(this HttpContext httpContent,IdentityEntity identity, string content)
        //{
        //    var url = GetIdentityWebSocketUrl(httpContent, identity.Id);
        //    if (string.IsNullOrWhiteSpace(url))
        //        return;
        //    Task.Run(() => {
        //        url = $"{url}/Home/SendIdentityMessage";
        //        WebRequestHelper.SendPostRequest(url, new Dictionary<string, string> { { "ticket", identity.TicketId }, { "content", System.Web.HttpUtility.UrlEncode(content) } });
        //    });
        //}

        public static string GetIdentityWebSocketUrl(this HttpContext httpContent, long accountId)
        {
            var urls = new List<string>();
            foreach (var setting in ConfigurationManager.Settings)
            {
                if (!Regex.IsMatch(setting.Key, "^PresentationServiceWebSocket\\w*Url$") || string.IsNullOrWhiteSpace(setting.Value))
                    continue;
                urls.Add(setting.Value);
            }
            if (urls.Count == 0)
                return null;
            return urls[(int) (accountId % urls.Count)];

        }
    }
}