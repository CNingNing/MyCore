using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Winner.Queue
{
    public class LocalQueue : IQueue
    {

        private static ConcurrentDictionary<string, ConcurrentQueue<object>> _message = new ConcurrentDictionary<string, ConcurrentQueue<object>>();
        /// <summary>
        /// 消息
        /// </summary>
        public static ConcurrentDictionary<string, ConcurrentQueue<object>> Message
        {
            get { return _message; }
            set { _message = value; }
        }
        private static ConcurrentDictionary<string, QueueInfo> _expirations = new ConcurrentDictionary<string, QueueInfo>();
        /// <summary>
        /// 消息
        /// </summary>
        public static ConcurrentDictionary<string, QueueInfo> Expirations
        {
            get { return _expirations; }
            set { _expirations = value; }
        }

        /// <summary>
        /// 消息
        /// </summary>
        public static IDictionary<string, IList<Action<string,object>>> Subscribes = new ConcurrentDictionary<string, IList<Action<string,object>>>();
   
        /// <summary>
        /// 打开消息队列
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        public virtual bool Open(string name, QueueInfo info)
        {
            try
            {
                if (info.MaxCount == 0)
                    return true;
                if (!Message.ContainsKey(name))
                {
                    Message.TryAdd(name, new ConcurrentQueue<object>());
                    Expirations.TryAdd(name, info);
                }

                return true;
            }
            finally
            {
            }
          
        }
        /// <summary>
        /// 保存取值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public virtual int Push<T>(string name, T value)
        {
            try
            {
                lock (SubscribeLocker)
                {
                    if (Subscribes.ContainsKey(name))
                    {
                        var handles = Subscribes[name];
                        var task = new Thread(() =>
                        {
                            for (int i = 0; i < handles.Count; i++)
                            {
                                handles[i](name, value);
                            }
                        });
                        task.Start();
                    }
                }
                if (!Message.ContainsKey(name))
                    return 0;
                Message[name].Enqueue(value);
                if (Message[name].Count > Expirations[name].MaxCount)
                    Pop<T>(name);
                return Message[name].Count;
            }
            finally
            {
                ScanClear();
            }
           
        }

        /// <summary>
        /// 保存取值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="info"></param>
        public virtual int Push<T>(string name, T value, QueueInfo info)
        {
            try
            {
                Open(name, info);
                return Push(name, value);
            }
            finally
            {
                ScanClear();
            }

        }
        /// <summary>
        /// 取值
        /// </summary>
        /// <returns></returns>
        public virtual T Pop<T>(string name)
        {
            try
            {
                if (!Message.ContainsKey(name) || !Expirations.ContainsKey(name))
                    return default(T);
                if (!Expirations[name].Validate())
                    return default(T);
                object result;
                Message[name].TryDequeue(out result);
                Expirations[name].SetLastAccessTime();
                return (T)result;
            }
            finally
            {
                ScanClear();
            }
          
        }
        /// <summary>
        /// 关闭消息队列
        /// </summary>
        /// <param name="name"></param>
        public virtual bool Close(string name)
        {
            try
            {
                if (Message.ContainsKey(name))
                    Message.TryRemove(name, out _);
                if (Expirations.ContainsKey(name))
                    Expirations.TryRemove(name, out _);
                return true;
            }
            finally
            {
                ScanClear();
            }
          
        }
        static object SubscribeLocker=new object();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual bool Subscribe(string name, Action<string, object> handle)
        {
            try
            {
                lock (SubscribeLocker)
                {
                    if (!Subscribes.ContainsKey(name))
                        Subscribes.Add(name, new List<Action<string, object>>());
                    if (!Subscribes[name].Contains(handle))
                        Subscribes[name].Add(handle);
                    return true;
                }
            }
            finally
            {
                ScanClear();
            }


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual bool Unsubscribe(string name, Action<string, object> handle)
        {
            try
            {
                lock (SubscribeLocker)
                {
                    if (!Subscribes.ContainsKey(name))
                        return true;
                    if (Subscribes[name].Contains(handle))
                        Subscribes[name].Remove(handle);
                    if (Subscribes[name].Count == 0)
                        Subscribes.Remove(name);
                    return true;
                }
            }
            finally
            {
                ScanClear();
            }
         
           
        }

      
        /// <summary>
        /// 检查
        /// </summary>
        public static void ScanClear()
        {
            var task=new Thread(() =>
            {
                var keys = Expirations.Keys.ToArray();
                foreach (var key in keys)
                {
                    QueueInfo expiration;
                    Expirations.TryGetValue(key, out expiration);
                    if (expiration.ExpireSecond <= 0 || (DateTime.Now - expiration.GetLastAccessTime()).TotalSeconds <= expiration.ExpireSecond)
                        continue;
                    if (Message.ContainsKey(key))
                        Message.TryRemove(key, out _);
                    if (Expirations.ContainsKey(key))
                        Expirations.TryRemove(key, out _);
                }
            });
            task.Start();
            
        }
    }
}
