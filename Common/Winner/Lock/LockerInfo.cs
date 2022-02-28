namespace Winner.Lock
{


    public class LockerInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public bool IsOptimisticLock { get; set; }
        /// <summary>
        /// 失效时间秒
        /// </summary>
        public int ExpireSecond { get; set; } = 120;


    }
}
