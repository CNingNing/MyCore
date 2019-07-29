using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;



namespace Test
{
    class Program
    {
        public const string appkey= "wx9ca6cb740514ee2c";
        public const string appsecret = "fe71874b2c7082c4ae29b27649995de1";

        private static IConfiguration _configuration;
        static void Main(string[] args)
        {
            var path = AppContext.BaseDirectory;
            if (path.IndexOf("\\bin") > 0)
            {
                path = path.Substring(0, path.IndexOf("\\bin"));
            }
            var filename = $"{path}\\DBContext\\DBModels\\database.json";
            if(!File.Exists(filename))
            {

            }

            Console.WriteLine(path);

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
