using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Component.Extension;

namespace Test
{
    class Program
    {
        public const string appkey= "wx9ca6cb740514ee2c";
        public const string appsecret = "fe71874b2c7082c4ae29b27649995de1";
       
        static void Main(string[] args)
        {

            var redis = new CSRedis.CSRedisClient("127.0.0.1:6379");
            redis.Set("myning","2227");
            Console.WriteLine(redis.Get("myning"));
            

            //var url =
            // string.Format(
            //     "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appkey,
            //     appsecret);
            //HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //var json = GetResponse(request, "utf-8");
            ////if (string.IsNullOrEmpty(json))
            ////    Console.WriteLine("");
            //var dis = ConvertExtension.DeserializeJson<IDictionary<string, string>>(json);

            //var com = WebRequestHelper.SendPostRequest(url, null);

            //var  cn= WebRequestHelper.SendPostRequest(url, Encoding.UTF8, "");


            //Console.WriteLine(json);
            //Console.WriteLine(com);
            //Console.WriteLine(cn);
        }
        /// <summary>
        /// 返回流
        /// </summary>
        /// <param name="request"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        protected  static  string GetResponse(WebRequest request, string encoding)
        {
            using (WebResponse response = request.GetResponse())
            {
                var stream = response.GetResponseStream();
                if (stream == null) return null;
                using (var reader = new StreamReader(stream, Encoding.GetEncoding(encoding)))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
