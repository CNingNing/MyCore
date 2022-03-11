using Microsoft.Extensions.Caching.Memory;
using System;
using Winner.Log;

namespace Winner.Cache
{
    public class LocalCache :  ICache
    {
        private static readonly MemoryCache CacheInstance = new MemoryCache(new MemoryCacheOptions());
        #region 接口的实现
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return default(T);
            object value = null;
            if (CacheInstance.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return default(T);
        }
        /// <summary>
        /// 得到对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object Get(string key, Type type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return null;
                object value = null;
                if (CacheInstance.TryGetValue(key, out value))
                {
                    return Convert.ChangeType(value, type);
                }
            }
            catch (Exception ex)
            {
               Creator.Get<ILog>().AddException(ex);
            }
            return null;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual bool Set<T>(string key, T value, DateTime time)
        {
    
            try
            {
                if (string.IsNullOrWhiteSpace(key) || value == null)
                    return false;
                CacheInstance.Set(key, value, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = time
                });
                return true;
            }
            catch (Exception ex)
            {
                Creator.Get<ILog>().AddException(ex);
            }
            return false;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public virtual bool Set<T>(string key, T value, long timeSpan)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) || value == null)
                    return false;
                CacheInstance.Set(key, value, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(timeSpan)
                });
                return true;
            }
            catch (Exception ex)
            {
                Creator.Get<ILog>().AddException(ex);
            }
            return false;

        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool Set<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) || value == null)
                    return false;
                CacheInstance.Set(key, value, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromDays(365)
                });
                return true;
            }
            catch (Exception ex)
            {
                Creator.Get<ILog>().AddException(ex);
            }
            return false;

        }
        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public virtual bool Remove(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return false;
                CacheInstance.Remove(key);
                return true;
            }
            catch (Exception ex)
            {
                Creator.Get<ILog>().AddException(ex);
            }
            return false;
          
        }

        #endregion
    }
}
