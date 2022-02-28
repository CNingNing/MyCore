
using System.Net;

namespace Winner.Base
{

    public interface IComponent
    {
        

        /// <summary>
        /// 得到页面内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        string GetPageContent(string url, string encoding);
        /// <summary>
        /// 得到文件内容
        /// </summary>
        /// <param name="request"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        string GetPageContent(WebRequest request, string encoding);
        /// <summary>
        /// 得到验证码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        byte[] CreateCodeImage(string code);
    }
}
