using System;

namespace Winner.Persistence.Route
{
    [Serializable]
    public enum DbRouteType
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default = 1,
        /// <summary>
        /// 全部分片
        /// </summary>
        All = 2
    }
}
