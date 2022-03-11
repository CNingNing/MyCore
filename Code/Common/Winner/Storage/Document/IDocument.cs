using System;

namespace Winner.Storage.Document
{
    public interface IDocument
    {
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="info"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        string SignUrl(string url, DocumentInfo info, DateTime dateTime);
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="info"></param>
        void Set(DocumentInfo info);
        /// <summary>
        /// 设置根路径
        /// </summary>
        /// <param name="path"></param>
        void SetRootPath(string path);


    }
}
