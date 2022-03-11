using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Component.Extension
{
    static public class WebRequestHelper
    {
        static readonly HttpClient client = new();
        #region 发送Get
        /// <summary>
        /// 直接请求Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> SendGetRequestAsync(string url)
        {
            try
            {
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                responseMessage.EnsureSuccessStatusCode();
                if(responseMessage.StatusCode==  HttpStatusCode.OK)
                {
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    return response;
                }
                return null;
               
            }
            catch(Exception ex)
            {
                return ex.Message;

            }
        }
        /// <summary>
        /// 带参数请求url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> SendGetRequestAsync(Uri url,string data)
        {
            try
            {
                client.BaseAddress = url;
                var response = await client.GetStringAsync(data);
                return response;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
           

        }
        /// <summary>
        /// 请求Url返回字节，并处理
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string>SendGetRequestByteAsync(string url)
        {
            try
            {
                var responseMessage = await client.GetByteArrayAsync(url);
                var response = Encoding.GetEncoding(Encoding.UTF8.ToString()).GetString(responseMessage);
                return response;
            }catch(Exception ex)
            {
                return ex.Message;
            }
            
        }
        /// <summary>
        /// 请求Url返回流，并处理
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string>SendGetRequstStreamAsync(string url)
        {
            try
            {
                var responseMessage = await client.GetStreamAsync(url);
                using MemoryStream stream = new();
                int bytesRead = 0;
                byte[] buffer = new byte[65530];
                while ((bytesRead = responseMessage.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
                var response = Encoding.GetEncoding(Encoding.UTF8.ToString()).GetString(stream.ToArray());
                return response;
            }catch (Exception ex)
            {
                return ex.Message;
            }
            
        }
        #endregion
        #region 发送POST
        /// <summary>
        /// application/json形式发送
        /// </summary>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<string> SendPostAysnc(string url,string request)
        {
            try
            {
                HttpContent content = new StringContent(request, Encoding.UTF8);
                var responseMessage = await client.PostAsync(url, content);
                if(responseMessage.StatusCode==HttpStatusCode.OK)
                {
                    if (responseMessage == null)
                        return null;
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    return response;
                }
                return null;
               
            }catch(Exception ex)
            {
                return ex.Message;
            }
           
        }
        /// <summary>
        /// multipart/form-data形式发送
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static async Task<string> SendPostAysnc(string url, IDictionary<string,object>request)
        {
            try
            {
                HttpContent content = new MultipartFormDataContent(request.SerializeJson());
                var responseMessage = await client.PostAsync(url, content);
                if(responseMessage.StatusCode== HttpStatusCode.OK)
                {
                    if (responseMessage == null)
                        return null;
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    return response;
                }
                return null;
                
            }catch(Exception ex)
            {
                return ex.Message;
            }
            
        }
        /// <summary>
        /// 带授权头的application/json请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="request">请求体</param>
        /// <param name="useranme">账号</param>
        /// <param name="userpassword">密码</param>
        /// <returns></returns>
        public static async Task<string>SendPostAsync(string url,string request,string useranme,string userpassword)
        {
            try
            {
                HttpContent content = new StringContent(request, Encoding.UTF8);
                var value = $"{useranme}:{userpassword}";
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
                var responseMessage = await client.PostAsync(url, content);
                if(responseMessage.StatusCode== HttpStatusCode.OK)
                {
                    if (responseMessage == null)
                        return null;
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    return response;
                }
                return null;
            }catch (Exception ex)
            {
                return ex.Message;
            }
           
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
        #endregion
        public static string GetResponse(WebRequest request, string encoding)
        {
            using WebResponse response = request.GetResponse();
            var stream = response.GetResponseStream();
            if (stream == null) return null;
            using var reader = new StreamReader(stream, Encoding.GetEncoding(encoding));
            return reader.ReadToEnd();
        }
    }
}
