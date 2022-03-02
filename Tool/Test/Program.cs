using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Component.Extension;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace Test
{
    class Program
    {
        public const string appkey= "wx9ca6cb740514ee2c";
        public const string appsecret = "fe71874b2c7082c4ae29b27649995de1";

        private static IConfiguration _configuration;

        public enum Color
        {
            red=1,
            black=2
        }

        static void Main(string[] args)
        {
           var path=AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine(path);

        }

        public delegate void Play(object e);
        public class Employee
        {
            public int Money = 2000;
            public event Play Play;
            public void PlayGame()
            {

            }
        }
        public class Manager
        {
            public void RemoveMoney()
            {
                
            }
        }
            


        public static byte[] Decompress(byte[] data)
        {
            try
            {
                MemoryStream ms = new MemoryStream(data);
                GZipStream zip = new GZipStream(ms, CompressionMode.Decompress, true);
                MemoryStream msreader = new MemoryStream();
                byte[] buffer = new byte[0x1000];
                while (true)
                {
                    int reader = zip.Read(buffer, 0, buffer.Length);
                    if (reader <= 0)
                    {
                        break;
                    }
                    msreader.Write(buffer, 0, reader);
                }
                zip.Close();
                ms.Close();
                msreader.Position = 0;
                buffer = msreader.ToArray();
                msreader.Close();
                return buffer;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    


        public  class Flight
        {
           public  string Code { get; set; }
            public int Count { get; set; }
        }





        /// <summary>
        /// 返回流
        /// </summary>
        /// <param name="request"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        protected static  string GetResponse(WebRequest request, string encoding)
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
