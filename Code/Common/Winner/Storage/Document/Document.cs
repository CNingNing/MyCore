using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Winner.Storage.Document
{
    public class Document : IDocument
    {
        protected virtual string RootPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public virtual void SetRootPath(string path)
        {
            RootPath = path;
        }

        /// <summary>
        /// 签名地址
        /// </summary>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public virtual string SignUrl(string url, DocumentInfo info, DateTime dateTime)
        {
            try
            {
                if (string.IsNullOrEmpty(info.FileName))
                    return url;
                info.Timestamp = (dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds.ToString();
                info.Sign= Creator.Get<Base.ISecurity>().EncryptSign($"{info.FileName}{info.Timestamp}{info.ContentType}{info.Key}");
                url = string.Format("{0}{1}timestamp={2}&contenttype={3}&sign={4}", url, url.Contains("?") ? "&" : "?", info.Timestamp, info.ContentType, info.Sign);
                return url;
            }
            catch (Exception e)
            {
                return null;
            }
      
        }
        /// <summary>
        /// 检查
        /// </summary>
        /// <returns></returns>
        public virtual void Set(DocumentInfo info)
        {
            if (info==null || string.IsNullOrEmpty(info.FileName))
            {
                return;
            }
            if (!Check(info))
            {
                return;
            }

            info.Result = true;
            string fileName = Path.Combine(RootPath, info.FileName);
            var exe = Path.GetExtension(fileName);
            info.FileName = fileName;
            if (File.Exists(fileName))
            {
                if(info.ContentType== "application/octet-stream")
                {
                    info.ContentType = "application/octet-stream";
                    info.Headers = new Dictionary<string, string>();
                    info.Headers.Add("charset", "UTF-8");
                    info.Headers.Add("Content-Disposition", string.Format("attachment;filename={0}{1}", string.IsNullOrEmpty(info.OutputName) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(info.OutputName) , exe));

                }
                using (var fileStream = new FileStream(info.FileName, FileMode.Open))
                {
                    info.Output = new byte[fileStream.Length];
                    fileStream.Read(info.Output, 0, info.Output.Length);
                }
            }
        }
        /// <summary>
        /// 检查
        /// </summary>
        /// <returns></returns>
        public virtual bool Check(DocumentInfo info)
        {
            try
            {
                if (string.IsNullOrEmpty(info.Sign))
                {
                    info.Message = "must provider sign";
                    return false;
                }
                if (double.Parse(info.Timestamp) < (DateTime.Now - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds)
                {
                    info.Message = "timestamp over";
                    return false;
                }
                var sign = Creator.Get<Base.ISecurity>().EncryptSign($"{info.FileName}{info.Timestamp}{info.ContentType}{info.Key}");
                if (info.Sign!= sign)
                {
                    info.Message = "sign error";
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                info.Message = $"check exception:{e.Message}";
                return false;
            }

        }
      
    }
}
