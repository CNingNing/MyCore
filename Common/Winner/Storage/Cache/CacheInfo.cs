using System;

namespace Winner.Storage.Cache
{
    [Serializable]
    public class CacheInfo
    {
        /// <summary>
        /// 缓存路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 缓存时间
        /// </summary>
        public int Times { get; set; }
    }
}
