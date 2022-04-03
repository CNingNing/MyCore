using System;

namespace Winner.Log
{

    public interface ILog
    {
        /// <summary>
        /// 记录异常信息
        /// </summary>
        /// <param name="ex"></param>
        void AddException(Exception ex);

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="content"></param>
        void WriteFile(string content);
    }
}
