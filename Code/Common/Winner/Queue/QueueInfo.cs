using System;

namespace Winner.Queue
{
    public class QueueInfo
    {
        /// <summary>
        /// 最大数量数量
        /// </summary>
        public int MaxCount { get; set; } = 10000;

        /// <summary>
        /// 失效时间秒
        /// </summary>
        public int ExpireSecond { get; set; } = 3600;
        /// <summary>
        /// 最近访问时间
        /// </summary>
        protected DateTime LastAccessTime { get; set; }=DateTime.Now;

        public virtual void SetLastAccessTime()
        {
            LastAccessTime = DateTime.Now;
        }

        public virtual DateTime GetLastAccessTime()
        {
            return LastAccessTime;
        }

        public virtual bool Validate()
        {
            return (DateTime.Now - LastAccessTime).TotalSeconds <= ExpireSecond;
        }
    }
}
