using System;

namespace Winner.Persistence.Route
{
    [Serializable]
    public enum ShardingType
    {
        /// <summary>
        /// 固定
        /// </summary>
        Fixed = 1,
        /// <summary>
        /// 余数
        /// </summary>
        Remainder = 2,
        /// <summary>
        /// 值
        /// </summary>
        Value = 3,
        /// <summary>
        /// 随机
        /// </summary>
        Random=4
    }
}
