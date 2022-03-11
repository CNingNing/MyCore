using System;

namespace Winner.Persistence.Route
{
    [Serializable]
    public class ShardingInfo
    {
     
        /// <summary>
        /// 表索引
        /// </summary>
        public string TableIndex { get; set; }
        /// <summary>
        /// 数据库索引
        /// </summary>
        public string DatabaseIndex { get; set; }
    }
}
