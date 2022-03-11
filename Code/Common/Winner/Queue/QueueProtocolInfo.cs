namespace Winner.Queue
{
    public class QueueProtocolInfo
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public int ExpireSecond { get; set; }
    }
}