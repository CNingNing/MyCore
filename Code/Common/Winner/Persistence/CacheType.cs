namespace Winner.Persistence
{
    public enum CacheType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,
        /// <summary>
        /// 本地
        /// </summary>
        Local,
        /// <summary>
        /// 本地和远程
        /// </summary>
        LocalAndRemote,
        /// <summary>
        /// 本地和远程
        /// </summary>
        LocalAndRemoteDelayCheck,
        /// <summary>
        /// 远程
        /// </summary>
        Remote

    }
}
