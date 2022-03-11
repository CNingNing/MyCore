using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Winner.Channel;
using Winner.Log;

namespace Winner.Queue
{

    public class DistributedQueue :  IQueue
    {
        #region 属性

        /// <summary>
        /// 服务实例
        /// </summary>
        public IChannelClient ChannelClient { get; set; }
        private ILog _log;

        /// <summary>
        /// 实例
        /// </summary>
        public ILog Log
        {
            get
            {
                if (_log == null)
                    _log = Creator.Get<ILog>();
                return _log;
            }
            set { _log = value; }
        }
        /// <summary>
        /// 消息
        /// </summary>
        public static IDictionary<string, IList<Action<string, object>>> Subscribes = new ConcurrentDictionary<string, IList<Action<string, object>>>();
        #endregion

        #region 接口的实现
        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        public virtual bool Open(string name, QueueInfo info)
        {
           var rev= Handle('o',new QueueProtocolInfo { Name = name, MaxCount = info.MaxCount,ExpireSecond= info.ExpireSecond });
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }
        /// <summary>
        /// 存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual int Push<T>(string name, T value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return -1;
            var val = value.GetType().IsValueType || value is string?value.ToString(): SerializeJson(value);
            string rev = Handle('s', new QueueProtocolInfo { Name = name, Value = val });
            if (string.IsNullOrWhiteSpace(rev))
                return -1;
            int revValue;
            int.TryParse(rev, out revValue);
            return revValue;
        }
        private static readonly MemoryCache CacheInstance = new MemoryCache(new MemoryCacheOptions());
        private static  string _cacheKey = Guid.NewGuid().ToString("N");
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public int Push<T>(string name, T value, QueueInfo info)
        {
            var key = $"{_cacheKey}{name}";
            if (info.MaxCount > 0 && !CacheInstance.TryGetValue(key, out _))
            {
                CacheInstance.Set(key, "", new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.MaxValue.ToUniversalTime()
                });
                Open(name, info);
            }
            return Push(name, value);
        }

        /// <summary>
        /// 取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual T Pop<T>(string name)
        {
            var value = Handle('p',new QueueProtocolInfo { Name = name });
            if (value == null)
                return default(T);
            if (typeof(T).IsValueType || typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }
            return DeserializeJson<T>(value);
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="name"></param>
        public virtual bool Close(string name)
        {
            var rev= Handle('c',  new QueueProtocolInfo { Name = name});
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }
        static object SubscribeLocker = new object();
        public virtual bool Subscribe(string name, Action<string,object> handle)
        {
            var rev = Handle('b', new QueueProtocolInfo { Name = name }, ReceiveHandle);
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            lock (SubscribeLocker)
            {
                if (!Subscribes.ContainsKey(name))
                    Subscribes.Add(name, new List<Action<string, object>>());
                if (!Subscribes[name].Contains(handle))
                    Subscribes[name].Add(handle);
                return true;
            }
       
        }

        public virtual bool Unsubscribe(string name, Action<string, object> handle)
        {
            lock (SubscribeLocker)
            {
                if (!Subscribes.ContainsKey(name))
                    return true;
                if (Subscribes[name].Contains(handle))
                    Subscribes[name].Remove(handle);
                if (Subscribes[name].Count == 0)
                    Subscribes.Remove(name);
                if (!Subscribes.ContainsKey(name))
                {
                    var rev = Handle('u', new QueueProtocolInfo { Name = name }, ReceiveHandle);
                    if (string.IsNullOrWhiteSpace(rev))
                        return false;
                }
                return true;
            }
      
         
        }

        public virtual void ReceiveHandle(ChannelArgsInfo args)
        {
            if(args.SendId!=EndPointInfo.UnSetArgsSendId || args.Method!='s')
                return;
            var value = args.GetResult();
            if (string.IsNullOrWhiteSpace(value))
                return;
            var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<QueueProtocolInfo>(value);
            if (protocol == null || string.IsNullOrWhiteSpace(protocol.Name) || !Subscribes.ContainsKey(protocol.Name))
                return;
            lock (SubscribeLocker)
            {
                if (Subscribes.ContainsKey(protocol.Name))
                {
                    var handles = Subscribes[protocol.Name];
                    var task = new Thread(() =>
                    {
                        for (int i = 0; i < handles.Count; i++)
                        {
                            handles[i](protocol.Name, protocol.Value);
                        }
                    });
                    task.Start();

                }
            }
        }
        #endregion

        #region 方法

        private const string ChannelName = "QueueService";

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        protected virtual string Handle(char method,QueueProtocolInfo protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol.Name))
                return null;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            var endPoint = GetEndPoint(endPoints, protocol.Name);
            var args = new ChannelArgsInfo{Method=method};
            var value = SerializeJson(protocol);
            args.SetArgs(value);
            ChannelClient.Send(endPoint, args);
            var result = args.GetResult();
            return result;
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        /// <param name="receiveHandle"></param>
        /// <returns></returns>
        protected virtual string Handle(char method, QueueProtocolInfo protocol,Action<ChannelArgsInfo> receiveHandle)
        {
            if (string.IsNullOrWhiteSpace(protocol.Name))
                return null;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            var endPoint = GetEndPoint(endPoints, protocol.Name);
            endPoint.ReceiveHandle = receiveHandle;
            var args = new ChannelArgsInfo { Method = method};
            var value = SerializeJson(protocol);
            args.SetArgs(value);
            ChannelClient.Send(endPoint, args);
            var result = args.GetResult();
            return result;
        }
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        protected virtual string SerializeJson(object input)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(input);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                return null;
            }
        }
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        protected virtual T DeserializeJson<T>(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return default(T);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                return default(T);
            }
        }

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        protected virtual object DeserializeJson(string input, Type type)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return null;
                return Newtonsoft.Json.JsonConvert.DeserializeObject(input, type);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                return null;
            }
        }
        #endregion

        #region 计算Key值
        /// <summary>
        /// 根据key得到要存放的服务器
        /// </summary>
        /// <param name="endPoints"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual EndPointInfo GetEndPoint(IList<EndPointInfo> endPoints, string key)
        {
            if (endPoints == null || endPoints.Count == 0)
                return null;
            long hash = GenerateLongId(key);
            var index = (int)(hash % endPoints.Count);
            return endPoints[index];
        }
        /// <summary>
        /// 得到缓存值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual long GenerateLongId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            byte[] buffer = Encoding.Default.GetBytes(EncryptMd5(input));
            var i = BitConverter.ToInt64(buffer, 0);
            return i;
        }
        /// <summary>
        /// 得到MD5加密
        /// </summary>
        /// <returns></returns>
        protected virtual string EncryptMd5(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var md5 = MD5.Create();
            byte[] bytValue = Encoding.UTF8.GetBytes(input);
            byte[] bytHash = md5.ComputeHash(bytValue);
            var sTemp = new StringBuilder();
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp.Append(bytHash[i].ToString("X").PadLeft(2, '0'));
            }
            return sTemp.ToString().ToLower();
        }
        #endregion

    }
}
