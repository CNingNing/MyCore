using Winner.Channel;

namespace Winner.Cluster
{
    public class ClusterProtocolInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 节点索引
        /// </summary>
        public int EndPointIndex { get; set; }
  
    }
}
