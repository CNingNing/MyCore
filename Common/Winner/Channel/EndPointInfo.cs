using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Winner.Channel
{
    public class EndPointInfo:ChannelInfo
    {
        
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string ConnectName { get; set; }
        /// <summary>
        /// 当前并发数
        /// </summary>
        public virtual bool IsUsed { get; set; }
        /// <summary>
        /// 读取故障转移的数据
        /// </summary>
        public virtual IList<EndPointInfo> Failovers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string Args { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public virtual bool IsStartKeepAlive { get; set; }
        ///// <summary>
        ///// 重置为非异常周期
        ///// </summary>
        //public virtual int KeepAliveTimes { get; set; } = 5000;
        /// <summary>
        /// 缓存大小
        /// </summary>
        public virtual int BufferSize { get; set; }
        /// <summary>
        /// 缓存大小
        /// </summary>
        public virtual int Timeout { get; set; } = 30000;
        /// <summary>
        /// 缓存大小
        /// </summary>
        public virtual int SleepTimes { get; set; } = 5;
        public static int UnSetArgsSendId { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public virtual Action<ChannelArgsInfo> ReceiveHandle { get; set; }
     
        /// <summary>
        /// 使用连接
        /// </summary>
        public virtual void Use()
        {
            IsUsed = true;
        }
        /// <summary>
        /// 使用连接
        /// </summary>
        public virtual void Release()
        {
            IsUsed = false;
        }
        //private static readonly MemoryCache CacheInstance = new MemoryCache(new MemoryCacheOptions());
        public class ChannelArgsDto
        {
            /// <summary>
            /// 
            /// </summary>
            public ChannelArgsInfo Args { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public DateTime ExpiredTime { get; set; }
        }
        public ConcurrentDictionary<string, ChannelArgsDto> CacheInstance = new ConcurrentDictionary<string, ChannelArgsDto>();
        public virtual string TrySetGetKey(ChannelArgsInfo args)
        {
            args.SendId = args.SendId == int.MinValue ? GetSendId() : args.SendId;
            var key = $"{ConnectName}:{args.SendId}";
            var rev = CacheInstance.TryAdd(key, null);
            return rev ? key : null;
        }
        public static object CacheInstanceLocker = new object();

        public virtual void ClearCacheInstance()
        {
            lock(CacheInstanceLocker)
            {
                CacheInstance.Clear();
            }
        }
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual ChannelArgsInfo Get(ChannelArgsInfo args,string key)
        {
            ChannelArgsDto value = null;
            var timeout = args.Timeout == 0 ? Timeout : args.Timeout;
            var start = DateTime.Now;
            while ((DateTime.Now-start).TotalMilliseconds<= timeout)
            {
                if (IsException || CacheInstance.Count==0)
                    return null;
                lock (CacheInstanceLocker)
                {
                    CacheInstance.TryGetValue(key, out value);
                    if (value != null)
                    {
                        CacheInstance.TryRemove(key, out value);
                        return value?.Args;
                    }
                }
                Thread.Sleep(SleepTimes);
            }
            lock (CacheInstanceLocker)
            {
                CacheInstance.TryRemove(key, out _);
                throw new Exception($"Get TimeOut,SendId:{args.SendId}");
            }
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual void Set(ChannelArgsInfo args)
        {
            var timeout = args.Timeout == 0 ? Timeout : args.Timeout;
            if (ReceiveHandle != null)
            {
                try
                {
                    var task=new Thread(() =>
                    {
                        ReceiveHandle(args);
                    });
                    task.Start();



                }
                catch (Exception e)
                {
                    
                }
               
            }
            if (args == null || args.SendId== UnSetArgsSendId) return ;
            var key = $"{ConnectName}:{args.SendId}";
            lock (CacheInstanceLocker)
            {
                if (CacheInstance.ContainsKey(key))
                {
                    CacheInstance.TryUpdate(key, new ChannelArgsDto { Args = args, ExpiredTime = DateTime.Now.AddSeconds(timeout)},null);
                }
            }
           
            //CacheInstance.Set(key, args, new MemoryCacheEntryOptions
            //{
            //    AbsoluteExpiration = DateTime.Now.AddSeconds(timeout)
            //});
        }

        public virtual EndPointInfo GetFailoverEndPoint()
        {
            if (IsUsed && Failovers != null && Failovers.Count > 0)
            {
                foreach (var endPoint in Failovers)
                {
                    if (!endPoint.IsUsed)
                        return endPoint;
                }
            }
            return this;
        }


    }

    public static class EndPointExtension
    {
        public static EndPointInfo GetBestEndPoint(this IList<EndPointInfo> endPoints)
        {
            foreach (var endPoint in endPoints)
            {
                if (!endPoint.IsUsed)
                    return endPoint;
            }
            return endPoints.FirstOrDefault();
            //return endPoints.OrderBy(it => it.IsException).ThenBy(it => it.IsUsed).FirstOrDefault();
        }

    }

   
}
