using System;

namespace Winner.Persistence.Route
{
    [Serializable]
    public enum RuleType
    {
        /// <summary>
        /// 所有
        /// </summary>
        All = 1,
        /// <summary>
        /// 读
        /// </summary>
        Read = 2,
        /// <summary>
        /// 写
        /// </summary>
        Write = 3
    }
}
