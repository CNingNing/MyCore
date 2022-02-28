using System.Collections.Generic;
using System.IO;

namespace Winner.Storage.Document
{
    public class DocumentInfo
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Sign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Timestamp { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// 字符集
        /// </summary>
        public string OutputName { get; set; }
        /// <summary>
        /// 输出流
        /// </summary>
        public byte[] Output { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string ContentType= "application/octet-stream";
        /// <summary>
        /// 头文件
        /// </summary>
        public IDictionary<string, string> Headers;
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

    }
}
