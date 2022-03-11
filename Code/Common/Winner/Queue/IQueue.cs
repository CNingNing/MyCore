using System;

namespace Winner.Queue
{
    public interface IQueue
    {
        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        bool Open(string name, QueueInfo info);
        /// <summary>
        /// 保存取值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        int Push<T>(string name, T value);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        int Push<T>(string name, T value, QueueInfo info);
        /// <summary>
        /// 取值
        /// </summary>
        /// <returns></returns>
        T Pop<T>(string name);
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="name"></param>
        bool Close(string name);
        /// <summary>
        /// 取值
        /// </summary>
        /// <returns></returns>
        bool Subscribe(string name,Action<string,object> handle);
        /// <summary>
        /// 取值
        /// </summary>
        /// <returns></returns>
        bool Unsubscribe(string name, Action<string,object> handle);
    }
}
