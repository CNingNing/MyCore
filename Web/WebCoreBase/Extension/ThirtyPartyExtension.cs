//using System.Collections.Generic;
//using Beeant.Application.Services.Utility;
//using Beeant.Domain.Entities.Utility;
//using Microsoft.AspNetCore.Mvc;
//using Component.Extension;
//using Component.Sdk;
//using Configuration;
//using Dependent;
//using Microsoft.AspNetCore.Http;

//namespace WebCore.Extension
//{
//    public static class ThirtyPartyExtension
//    {
//        public static T Sdk<T>(this HttpContext httpContent,  IDictionary<string, string> forms = null, string domain = null)
//        {
//            domain = string.IsNullOrWhiteSpace(domain) ? httpContent.Request.Get("domain") : domain;
//            domain = string.IsNullOrWhiteSpace(domain) ? httpContent.Request.GetDomain() : domain;
//            var entity = new ThirdPartyEntity<T>
//                { Identity = httpContent.GetIdentity(), Forms = httpContent.Request.GetForms() };
//            entity.Forms = entity.Forms ?? new Dictionary<string, string>();
//            if (entity.Forms.ContainsKey("domain"))
//                entity.Forms.Remove("domain");
//            entity.Forms.Add("domain", domain);
//            if (forms != null)
//            {
//                foreach (var form in forms)
//                {
//                    if(!entity.Forms.ContainsKey(form.Key))
//                        entity.Forms.Add(form.Key,form.Value);
//                }
//            }
//            var result = Ioc.Resolve<IThirdPartyApplicationService>().Get(entity);
//            if (result == null)
//                return default(T);
//            return result.ThirdParty;
//        }
//        #region 微信

//        public static WechatSdk Wechat(this HttpContext httpContent,IDictionary<string,string> forms=null, string domain=null)
//        {
//            return Sdk<WechatSdk>(httpContent, forms, domain);
//        }

//        public static string WechatOauth(this HttpContext httpContent,string fileName)
//        {
           
//            fileName = httpContent.MapPath($"/WechatOauth/{fileName}");
//            if (System.IO.File.Exists(fileName))
//                return System.IO.File.ReadAllText(fileName);
//            return null;
//        }
//        #endregion

//        #region 钉钉
      

//        public static DingTalkSdk DingTalk(this HttpContext httpContent, IDictionary<string, string> forms = null, string domain = null)
//        {
//            return Sdk<DingTalkSdk>(httpContent, forms,domain);
//        }


//        #endregion
//        #region QQ

//        public static QqSdk Qq(this HttpContext httpContent, IDictionary<string, string> forms = null, string domain = null)
//        {
//            return Sdk<QqSdk>(httpContent, forms, domain);
//        }
//        #endregion
//    }
//}
