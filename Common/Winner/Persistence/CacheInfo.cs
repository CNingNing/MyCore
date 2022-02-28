using System;
using System.Collections;
using System.Collections.Generic;

namespace Winner.Persistence
{
    public class CacheInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 缓存值
        /// </summary>
        public virtual string Key { get; set; }
        /// <summary>
        /// 设置查询缓存
        /// </summary>
        public virtual DateTime Time { get; set; }
        /// <summary>
        /// 设置缓存时间
        /// </summary>
        public virtual long TimeSpan { get; set; }
        /// <summary>
        /// 缓存类型
        /// </summary>
        public CacheType Type { get; set; }= CacheType.Remote;
        /// <summary>
        /// 是否订阅
        /// </summary>
        public IList<string> Dependencies { get; set; }
        /// <summary>
        /// 是否命中
        /// </summary>
        public bool IsHit { get; set; }

        /// <summary>
        /// 设置缓存时间
        /// </summary>
        public virtual int CheckTimeSpan { get; set; } = 600;


    }
}
