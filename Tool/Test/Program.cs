using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Microsoft.Extensions.Configuration;
using DBModels.Hr;
using DBModels;
namespace Test
{
    class Program
    {
        public const string appkey= "wx9ca6cb740514ee2c";
        public const string appsecret = "fe71874b2c7082c4ae29b27649995de1";

        private static IConfiguration _configuration;
        static void Main(string[] args)
        {
            
            var a=   DbConnnectString.GetDatabase("User");
            Console.WriteLine(a);

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
