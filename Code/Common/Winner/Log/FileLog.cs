using System;
using System.Text;
using System.IO;

namespace Winner.Log
{
    public class FileLog :  ILog
    {
        #region 静态变量
        /// <summary>
        /// 记录异常信息的锁
        /// </summary>
        static protected object ExceptionLocker = new object();
        #endregion

        #region 属性
        /// <summary>
        /// 记录异常信息路径
        /// </summary>
        public string ExceptionPath{get;set;}
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public FileLog()
        { 
        }
        /// <summary>
        /// 错误日志存储位置
        /// </summary>
        /// <param name="exceptionPath"></param>
        public FileLog(string exceptionPath)
        {
            ExceptionPath = exceptionPath;
        }
        #endregion

        #region 接口的实现
        /// <summary>
        /// 记录异常信息
        /// </summary>
        /// <param name="ex"></param>
        public virtual void AddException(Exception ex)
        {
            lock (ExceptionLocker)
            {
                WriteFile(GetContentByException(ex));
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param></param>
        /// <param name="content"></param>
        public virtual void WriteFile(string content)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExceptionPath);
            CreateDirectory(dir);
            var fileName = string.Format("{0}{1}.txt", dir, DateTime.Now.ToString("yyyy-MM-dd"));
            FileStream stream = new FileInfo(fileName).Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            stream.Seek(0, SeekOrigin.End);
            byte[] buffer1 = Encoding.GetEncoding("gb2312").GetBytes(content);
            stream.Write(buffer1, 0, buffer1.Length);
            var buffer2 = new[] { Convert.ToByte('\r'), Convert.ToByte('\n') };
            stream.Write(buffer2, 0, 2);
            stream.Flush();
            stream.Close();
        }
        #endregion

        #region 方法
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path"></param>
        protected virtual void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        /// <summary>
        /// 得到录入信息
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected virtual string GetContentByException(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("DateTime:{0}", DateTime.Now);
            sb.AppendFormat(",Message:{0}", ex.Message);
            sb.AppendFormat(",StackTrace:{0}", ex.StackTrace);
            sb.Append("\r\n---------------------------------------------------------\r\n");
            return sb.ToString();
        }
        #endregion

    }
}
