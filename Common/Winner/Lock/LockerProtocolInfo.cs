namespace Winner.Lock
{
    public class LockerProtocolInfo
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 失效时间秒
        /// </summary>
        public int ExpireSecond { get; set; }
        /// <summary>
        /// 失效时间秒
        /// </summary>
        public bool IsOptimisticLock { get; set; }
    }

}
