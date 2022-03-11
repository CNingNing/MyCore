using System;
using System.Collections.Generic;
using System.Text;

namespace Winner.Persistence
{
    public class VersionCacheInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string VersionKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LocalKey { get; set; }
        /// <summary>
        /// 检查时间
        /// </summary>
        public int CheckTimeSpan { get; set; }
        /// <summary>
        /// 最后一次检查时间
        /// </summary>
        public DateTime LastCheckTime { get; set; }
        /// <summary>
        /// 移除时间
        /// </summary>
        public DateTime RemoveTime { get; set; }
    }
}
