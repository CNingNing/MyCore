using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Component.Extension
{
    static public class WebRequestHelper
    {
        #region 发送POST

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sParaTemp"></param>
        /// <param name="isThrowException"></param>
        /// <returns></returns>
        public static string SendPostRequest(string url, IDictionary<string, string> sParaTemp,bool isThrowException=false)
        {
            
            return SendPostRequest((HttpWebRequest)WebRequest.Create(url), Encoding.UTF8, sParaTemp, isThrowException);
        }

        /// <summary>
        ///  Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="sParaTemp"></param>
        /// <param name="isThrowException"></param>
        /// <returns></returns>
        public static string SendPostRequest(string url, Encoding encoding, IDictionary<string, string> sParaTemp, bool isThrowException = false)
        {
           return SendPostRequest((HttpWebRequest) WebRequest.Create(url), encoding, sParaTemp, isThrowException);
        }

        /// <summary>
        ///  Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="isThrowException"></param>
        /// <returns></returns>
        public static string SendGetRequest(string url, Encoding encoding, bool isThrowException = false)
        {
            return SendRequest((HttpWebRequest)WebRequest.Create(url), encoding, null,"get", isThrowException);
        }

        /// <summary>
        ///  Post请求
        /// </summary>
        /// <param name="myReq"></param>
        /// <param name="encoding"></param>
        /// <param name="sParaTemp"></param>
        /// <param name="isThrowException"></param>
        /// <returns></returns>
        public static string SendPostRequest(HttpWebRequest myReq, Encoding encoding, IDictionary<string, string> sParaTemp, bool isThrowException = false)
        {
            return SendRequest(myReq, encoding, sParaTemp, "post", isThrowException);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myReq"></param>
        /// <param name="encoding"></param>
        /// <param name="sParaTemp"></param>
        /// <param name="method"></param>
        /// <param name="isThrowException"></param>
        /// <returns></returns>
        public static string SendRequest(HttpWebRequest myReq, Encoding encoding, IDictionary<string, string> sParaTemp,string method, bool isThrowException=false)
        {
            var sPara = new StringBuilder();
            if (sParaTemp != null && sParaTemp.Count > 0)
            {
                var @params = string.Join("&", sParaTemp.ToList().Select(it => $"{it.Key}={it.Value}"));
                sPara = new StringBuilder(@params);
                //foreach (var val in sParaTemp)
                //{
                //    //string.Join("&", $"{val.Key}={val.Value}");
                //    sPara.AppendFormat("{0}={1}&", val.Key, val.Value);
                //}
                //sPara.Remove(sPara.Length - 1, 1);
            }
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.Method = method;
            return SendRequest(myReq, encoding, sPara.ToString(), isThrowException);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myReq"></param>
        /// <param name="encoding"></param>
        /// <param name="content"></param>
        /// <param name="isThrowException"></param>
        /// <returns></returns>
        public static string SendRequest(HttpWebRequest myReq, Encoding encoding, string content, bool isZip, bool isThrowException = false)
        {
            if (isThrowException)
                return Request(myReq, encoding, content,false);
            try
            {
                return Request(myReq, encoding, content, false);
            }
            catch (WebException ex) // 这样我们就能捕获到异常，并且获取服务器端的输出
            {
                if (ex.Response == null)
                    throw ex;
                var wenReq = (HttpWebResponse)ex.Response;
                if (wenReq == null)
                    throw ex;
                var myStream = wenReq.GetResponseStream();
                if (myStream == null)
                    throw ex;
                if (isZip)
                {
                    myStream = new GZipStream(myStream, CompressionMode.Decompress);
                }
                using (var reader = new StreamReader(myStream, encoding))
                {
                    return reader.ReadToEnd();
                }

            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myReq"></param>
        /// <param name="encoding"></param>
        /// <param name="content"></param>
        /// <param name="isZip"></param>
        /// <returns></returns>
        public static string Request(HttpWebRequest myReq, Encoding encoding, string content,bool isZip)
        {
            ServicePointManager.Expect100Continue = true;
            if (ServicePointManager.SecurityProtocol != 0)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 |
                                                       (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            //request.ProtocolVersion = HttpVersion.Version10;
            ServicePointManager.ServerCertificateValidationCallback =
                (a, b, c, d) => true;
            if (!string.IsNullOrWhiteSpace(content))
            {
                byte[] bytesRequestData = encoding.GetBytes(content);
                myReq.ContentLength = bytesRequestData.Length;
                var requestStream = myReq.GetRequestStream();
                requestStream.Write(bytesRequestData, 0, bytesRequestData.Length);
                requestStream.Close();
            }
            var httpWResp = (HttpWebResponse)myReq.GetResponse();
            var myStream = httpWResp.GetResponseStream();
            if (myStream == null)
                return null;
            if (isZip)
            {
                myStream = new GZipStream(myStream, CompressionMode.Decompress);
            }
            var reader = new StreamReader(myStream, encoding);
            var result = reader.ReadToEnd();
            myStream.Close();
            return result;

        }


        /// <summary>
        ///  Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string SendPostRequest(string url, Encoding encoding, string content, string contentType= "application/x-www-form-urlencoded")
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType= contentType;
            return SendPostRequest(request, encoding, content);
        }
    
        /// <summary>
        ///  Post请求
        /// </summary>
        /// <param name="myReq"></param>
        /// <param name="encoding"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string SendPostRequest(HttpWebRequest myReq, Encoding encoding, string content)
        {
            myReq.Method = "POST";
            return SendRequest(myReq, encoding, content,false);
        }
     
        #endregion

        #region 证书服务
      
        /// <summary>
        /// 创建带证书设置的httpwebrequest
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="certFileName">证书名全路径</param>
        /// <param name="certPassword">证书密码</param>
        /// <returns></returns>
        public static WebRequest CreateWebRequestWithCertificate(string url, string certFileName, string certPassword)
        {

            HttpWebRequest request = null;
            var cert = CreateX509Certificate(certFileName, certPassword);
            if (cert != null)
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.ClientCertificates.Add(cert);
            }

            return request;
        }
        /// <summary>
        /// 获取指定证书
        /// </summary>
        /// <param name="certFileName">证书名全路径</param>
        /// <param name="certPassword">证书密码</param>
        /// <returns></returns>
        public static X509Certificate2 CreateX509Certificate(string certFileName, string certPassword)
        {
            X509Certificate2 cer = new X509Certificate2(certFileName, certPassword,
X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            return cer;
        }
        #endregion
    }
}
