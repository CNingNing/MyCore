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
using System.Security.Cryptography;
using EnterpriseSM;
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
            var str = "efd60cd4de9df5793f5a3124b577554d353dc4eb4ec0c21ef2719164851a0641c";
            var sm4 = Sm4Crypto.Decrypt(str);
            Console.WriteLine(sm4);


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
        

        /// <summary>
        /// 得到3DES
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt3Des(string input, string key= "cookieshare71234asdf4567")
        {
            if (string.IsNullOrEmpty(input)) return input;
            var des = TripleDES.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            var desDecrypt = des.CreateDecryptor();
            string result = "";
            try
            {
                byte[] buffer = Convert.FromBase64String(input);
                result = Encoding.UTF8.GetString(desDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch (Exception)
            {

            }
            return result;
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
