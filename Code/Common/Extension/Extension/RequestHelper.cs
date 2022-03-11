

using System;
using System.Collections.Generic;
using System.Net;

namespace Component.Extension
{
    /// <summary>
    ///ImageHelper 的摘要说明
    /// </summary>
    public class RequestHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static string GetDomain(string host)
        {
            string[] beReplacedStrs = new string[] { ".com", ".com.cn", ".edu.cn", ".net.cn", ".org.cn", ".co.jp", ".gov.cn", ".co.uk", ".ac.cn", ".cn", ".net", ".edu", ".tv", ".info", ".ac", ".ag", ".am", ".at", ".be", ".biz", ".bz", ".cc", ".de", ".es", ".eu", ".fm", ".gs", ".hk", ".in", ".info", ".io", ".it", ".jp", ".la", ".md", ".ms", ".name", ".nl", ".nu", ".org", ".pl", ".ru", ".sc", ".se", ".sg", ".sh", ".tc", ".tk", ".tv", ".tw", ".us", ".co", ".uk", ".vc", ".vg", ".ws", ".il", ".li", ".nz", ".tech" };
            foreach (string oneBeReplacedStr in beReplacedStrs)
            {
                if (host.EndsWith(oneBeReplacedStr))
                {
                    var index = host.Replace(oneBeReplacedStr, "").LastIndexOf('.');
                    return host.Substring(index + 1);
                }
            }
            return null;
        }

        public static Func<IDictionary<string, string>> RequestFormHandle { get; set; }
        public static IDictionary<string, string> GetRequestForms()
        {
            if (RequestFormHandle == null)
                return null;
            return RequestFormHandle();
        }

        public static Func<CookieCollection> RequestCookieHandle { get; set; }
        public static CookieCollection GetRequestCookies()
        {
            if (RequestCookieHandle == null)
                return null;
            return RequestCookieHandle();
        }

        public static Func<byte[]> RequestBodyHandle { get; set; }
        public static byte[] GetRequestBodyBytes()
        {
            if (RequestBodyHandle == null)
                return null;
            return RequestBodyHandle();
        }

        public static Func<string> RequestUrlHandle { get; set; }
        public static string GetRequestUrl()
        {
            if (RequestUrlHandle == null)
                return null;
            return RequestUrlHandle();
        }

        public static Func<string> RequestLanguageHandle { get; set; }
        public static string GetLanguage()
        {
            if (RequestLanguageHandle == null)
                return null;
            return RequestLanguageHandle();
        }

     

        public static Func<string> RequestDomainHandle { get; set; }
        public static string GetRequestDomain()
        {
            if (RequestDomainHandle == null)
                return null;
            return RequestDomainHandle();
        }

      

        public static Func<string> RequestIdentityTikcetHandle { get; set; }
        public static string GetRequestIdentityTikcet()
        {
            if (RequestIdentityTikcetHandle == null)
                return null;
            return RequestIdentityTikcetHandle();
        }

        public static Func<string> RequestIpHandle { get; set; }
        public static string GetRequestIp()
        {
            if (RequestIpHandle == null)
                return null;
            return RequestIpHandle();
        }
    }
}
