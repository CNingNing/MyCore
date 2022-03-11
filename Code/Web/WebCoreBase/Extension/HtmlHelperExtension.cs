
//using Configuration;
//using Dependent;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Component.Extension;

//namespace WebCore.Extension
//{
//    public static class HtmlHelperExtension
//    {
//        /// <summary>
//        /// 静态页面地址
//        /// </summary>
//        /// <param name="controller"></param>
//        /// <param name="siteName"></param>
//        /// <returns></returns>
//        public static string GetTicketUrl(this IHtmlHelper htmlHelper, string url)
//        {
//            return htmlHelper.ViewContext.HttpContext.GetTicketUrl(url);
//        }
//        /// <summary>
//        /// 静态页面地址
//        /// </summary>
//        /// <param name="controller"></param>
//        /// <param name="siteName"></param>
//        /// <returns></returns>
//        public static string GetUrl(this IHtmlHelper htmlHelper, string siteName)
//        {
//            return ConfigurationManager.GetSetting<string>(siteName);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="htmlHelper"></param>
//        /// <param name="siteName"></param>
//        /// <returns></returns>
//        public static string GetLogoFileName(this IHtmlHelper htmlHelper)
//        {
//            var identity = htmlHelper.ViewContext.HttpContext.GetIdentity();
//            var agentId = identity?.Numbers?.ContainsKey("AgentId") ?? false ? identity.Numbers["AgentId"] : "";
//            var tmcId = identity?.Numbers?.ContainsKey("TmcId") ?? false ? identity.Numbers["TmcId"] : "";
//            if (!string.IsNullOrWhiteSpace(agentId))
//            {
//                var agent = htmlHelper?.ViewBag?.Agent as AgentEntity;
//                agent = agent ?? Ioc.Resolve<IApplicationService>().GetEntity<AgentEntity>(agentId.Convert<long>());
//                if (!string.IsNullOrWhiteSpace(agent?.LogoFileName))
//                {
//                    return agent.LogoFullFileName;
//                }
//            }
//            if (!string.IsNullOrWhiteSpace(tmcId))
//            {
//                var tmc = htmlHelper?.ViewBag?.Tmc as TmcEntity;
//                tmc = tmc ?? Ioc.Resolve<IApplicationService>().GetEntity<TmcEntity>(tmcId.Convert<long>());
//                if (!string.IsNullOrWhiteSpace(tmc?.LogoFileName))
//                {
//                    return tmc.LogoFullFileName;
//                }
//            }
//            var info = htmlHelper.LoadRouter();
//            if (info == null || string.IsNullOrWhiteSpace(info.LogoFileName))
//                return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/logo.png";
//            return info.LogoFullFileName;

//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="htmlHelper"></param>
//        /// <param name="siteName"></param>
//        /// <returns></returns>
//        public static string GetFaviconFileName(this IHtmlHelper htmlHelper)
//        {
//            var identity = htmlHelper.ViewContext.HttpContext.GetIdentity();
//            var agentId = identity?.Numbers?.ContainsKey("AgentId") ?? false ? identity.Numbers["AgentId"] : "";
//            var tmcId = identity?.Numbers?.ContainsKey("TmcId") ?? false ? identity.Numbers["TmcId"] : "";
//            if (!string.IsNullOrWhiteSpace(agentId))
//            {
//                var agent = htmlHelper?.ViewBag?.Agent as AgentEntity;
//                agent = agent ?? Ioc.Resolve<IApplicationService>().GetEntity<AgentEntity>(agentId.Convert<long>());
//                if (!string.IsNullOrWhiteSpace(agent?.LogoFileName))
//                {
//                    return agent.FaviconFullFileName;
//                }
//            }
//            if (!string.IsNullOrWhiteSpace(tmcId))
//            {
//                var tmc = htmlHelper?.ViewBag?.Tmc as TmcEntity;
//                tmc = tmc ?? Ioc.Resolve<IApplicationService>().GetEntity<TmcEntity>(tmcId.Convert<long>());
//                if (!string.IsNullOrWhiteSpace(tmc?.FaviconFileName))
//                {
//                    return tmc.FaviconFullFileName;
//                }
//            }
//            var info = htmlHelper.LoadRouter();
//            if (info == null || string.IsNullOrWhiteSpace(info.FaviconFileName))
//                return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/favicon.ico";
//            return info.FaviconFullFileName;

//        }
//        public static string GetHotelLogoFileName(this IHtmlHelper htmlHelper)
//        {
//            var identity = htmlHelper.ViewContext.HttpContext.GetIdentity();
//            var agentId = identity?.Numbers?.ContainsKey("AgentId") ?? false ? identity.Numbers["AgentId"] : "";
//            var tmcId = identity?.Numbers?.ContainsKey("TmcId") ?? false ? identity.Numbers["TmcId"] : "";
//            if (!string.IsNullOrWhiteSpace(agentId))
//            {
//                var agent = htmlHelper?.ViewBag?.Agent as AgentEntity;
//                agent = agent ?? Ioc.Resolve<IApplicationService>().GetEntity<AgentEntity>(agentId.Convert<long>());
//                if (!string.IsNullOrWhiteSpace(agent?.LogoFileName))
//                {
//                    return agent.HotelLogoFullFileName;
//                }
//            }
//            if (!string.IsNullOrWhiteSpace(tmcId))
//            {
//                var tmc = htmlHelper?.ViewBag?.Tmc as TmcEntity;
//                tmc = tmc ?? Ioc.Resolve<IApplicationService>().GetEntity<TmcEntity>(agentId.Convert<long>());
//                if (!string.IsNullOrWhiteSpace(tmc?.LogoFileName))
//                {
//                    return tmc.HotelLogoFullFileName;
//                }
//            }
//            var info = htmlHelper.LoadRouter();
//            if (info == null || string.IsNullOrWhiteSpace(info.LogoFileName))
//                return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/logo.png";
//            return info.LogoFullFileName;

//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="htmlHelper"></param>
//        /// <returns></returns>
//        public static string GetDefaultFileName(this IHtmlHelper htmlHelper)
//        {
//            var info = htmlHelper.LoadRouter();
//            if (info == null || string.IsNullOrWhiteSpace(info.DefaultFileName))
//                return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/default.png";
//            return info.DefaultFullFileName;

//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="htmlHelper"></param>
//        /// <param name="fullFileName"></param>
//        /// <returns></returns>
//        public static string GetDefaultFileName(this IHtmlHelper htmlHelper,string fullFileName)
//        {
//            if (!string.IsNullOrWhiteSpace(fullFileName))
//                return fullFileName;
//            var info = htmlHelper.LoadRouter();
//            if (info == null || string.IsNullOrWhiteSpace(info.DefaultFileName))
//                return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/default.png";
//            return info.DefaultFullFileName;

//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="htmlHelper"></param>
//        /// <param name="siteName"></param>
//        /// <returns></returns>
//        public static string GetPrerenderFileName(this IHtmlHelper htmlHelper)
//        {
//            var info = htmlHelper.LoadRouter();
//            if (info == null || string.IsNullOrWhiteSpace(info.DefaultFileName))
//                return $"{ConfigurationManager.GetSetting<string>("PresentationServiceSharedUrl")}/img/prerender.gif";
//            return info.PrerenderFullFileName;

//        }
//        /// <summary>
//        /// 静态页面地址
//        /// </summary>
//        /// <param name="htmlHelper"></param>
//        /// <param name="siteName"></param>
//        /// <returns></returns>
//        public static string GetSiteName(this IHtmlHelper htmlHelper)
//        {
//            var info = htmlHelper.LoadRouter();
//            if (info == null || string.IsNullOrWhiteSpace(info.Name))
//                return ConfigurationManager.GetSetting<string>("DomainName");
//            return info.Name;
//        }
//        /// <summary>
//        /// 静态页面地址
//        /// </summary>
//        /// <param name="htmlHelper"></param>
//        /// <param name="siteName"></param>
//        /// <returns></returns>
//        public static string GetIcpName(this IHtmlHelper htmlHelper)
//        {
//            var info = htmlHelper.LoadRouter();
//            if (info == null || string.IsNullOrWhiteSpace(info.Name))
//                return ConfigurationManager.GetSetting<string>("DomainIcp");
//            return info.Name;
//        }
     
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        public static RouterEntity LoadRouter(this IHtmlHelper htmlHelper,string domain=null)
//        {
//            var info = htmlHelper.ViewBag.Router as RouterEntity;
//            if (info != null)
//                return info;
//            htmlHelper.ViewContext.ViewBag.Router = htmlHelper.ViewContext.HttpContext.GetRouter(domain);
//            return info;
//        }
   
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="lang"></param>
//        /// <returns></returns>
//        public static string GetLanguage(this IHtmlHelper htmlHelper)
//        {
//            return htmlHelper.ViewContext.HttpContext.Request.Cookies.ContainsKey("language") ? htmlHelper.ViewContext.HttpContext.Request.Cookies["language"] : null;
//        }
//    }
//}
