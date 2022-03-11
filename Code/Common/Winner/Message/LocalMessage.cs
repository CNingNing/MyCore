using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Winner.Log;

namespace Winner.Message
{
    public class LocalMessage : IMessage
    {
        private static readonly MemoryCache CacheInstance = new MemoryCache(new MemoryCacheOptions { });
        private static readonly IDictionary<string,object> Lockers = new ConcurrentDictionary<string, object>();
        private static object Locker = new object();
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
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual object GetLocker(string key)
        {
            if (Lockers.ContainsKey(key))
                return Lockers[key];
            lock (Locker)
            {
                if (Lockers.ContainsKey(key))
                    return Lockers[key];
                Lockers.Add(key,new object());
                return Lockers[key];
            }
        }
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        public virtual bool SetCount(MessageInfo info)
        {
            try
            {
                if (info == null || string.IsNullOrWhiteSpace(info.Key))
                    return false;
                lock (GetLocker(info.Key))
                {
                    CacheInstance.Set(info.Key, info.Count, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(info.ExpireSecond)
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                Creator.Get<ILog>().AddException(ex);
            }
            return false;
        }
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        public virtual int AddCount(MessageInfo info)
        {
            try
            {
                if (info == null || string.IsNullOrWhiteSpace(info.Key))
                    return 0;
                lock (GetLocker(info.Key))
                {
                    object value;
                    CacheInstance.TryGetValue(info.Key, out value);
                    if (value == null)
                        return 0;
                    var count = int.Parse(value.ToString()) + info.Count;
                    CacheInstance.Set(info.Key, count);
                    return count;
                }
            }
            catch (Exception ex)
            {
                Creator.Get<ILog>().AddException(ex);
            }
            return 0;
        }
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        public virtual int RemoveCount(MessageInfo info)
        {
            try
            {
                if (info == null || string.IsNullOrWhiteSpace(info.Key))
                    return 0;
                lock (GetLocker(info.Key))
                {
                    object value;
                    CacheInstance.TryGetValue(info.Key, out value);
                    if (value == null)
                        return 0;
                    var count = int.Parse(value.ToString()) - info.Count;
                    if(count<=0)
                    {
                        CacheInstance.Remove(info.Key);
                        Lockers.Remove(info.Key);
                        return 0;
                    }
                    CacheInstance.Set(info.Key, count);
                    return count;
                }
            }
            catch (Exception ex)
            {
                Log.AddException(ex);

            }
            return 0;
        }
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        public virtual int GetCount(string key)
        {
            try
            {
                lock (GetLocker(key))
                {
                    object value;
                    CacheInstance.TryGetValue(key, out value);
                    if (value == null)
                        return -1;
                    var count = int.Parse(value.ToString());
                    return count;
                }
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
             
            }
            return 0;
           
        }
    }
}
