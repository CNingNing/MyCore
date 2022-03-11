using System;

namespace Winner.Persistence.Route
{
    [Serializable]
    public class RuleInfo
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 是否Hash
        /// </summary>
        public bool IsHash { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UnRouteValue { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 开始索引
        /// </summary>
        public long StartValue { get; set; }
        /// <summary>
        /// 结束索引
        /// </summary>
        public long EndValue { get; set; }
        /// <summary>
        /// 固定值
        /// </summary>
        public string FixedValue { get; set; }
        /// <summary>
        /// 自动类型
        /// </summary>
        public ShardingType ShardingType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public RuleType RuleType { get; set; } = RuleType.All;
    }
}
