using System;
using Microsoft.Extensions.Caching.Memory;
using Winner.Log;

namespace Winner.Lock
{
    public class LocalLocker:ILocker
    {

        private static readonly MemoryCache CacheInstance = new MemoryCache(new MemoryCacheOptions { });
        private static object Locker = new object();


        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        public virtual bool Create(LockerInfo info)
        {
            try
            {
                lock (Locker)
                {
                    if (info == null || string.IsNullOrWhiteSpace(info.Key))
                        return false;
                    object result;
                    CacheInstance.TryGetValue(info.Key, out result);
                    if (result != null)
                        return false;
                    CacheInstance.Set(info.Key, info, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddSeconds(info.ExpireSecond)
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Creator.Get<ILog>().AddException(ex);
                if (info?.IsOptimisticLock == true)
                    return true;
            }
            return false;
        }

        public virtual bool Release(string key)
        {
            try
            {
                lock (Locker)
                {
                    if (string.IsNullOrWhiteSpace(key))
                        return false;
                    CacheInstance.Remove(key);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Creator.Get<ILog>().AddException(ex);
            }
            return false;
        }
    }
}
