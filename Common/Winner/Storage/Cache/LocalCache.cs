using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Winner.Storage.Cache
{
    public class LocalCache :  ICache
    {
        private IList<CacheInfo> _caches=new List<CacheInfo>();
        /// <summary>
        /// 缓存对象
        /// </summary>
        public IList<CacheInfo> Caches
        {
            get { return _caches; }
            set { _caches = value; }
        }
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
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool Set<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                return false;
            var cacheInfo = Caches?.FirstOrDefault(it => key.StartsWith(it.Path ?? ""));
            if (cacheInfo == null)
                return false;
            CacheInstance.Set(key, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(cacheInfo.Times)
            });
            return true;
        }

        #endregion
    }
}
