using System;
using System.Collections.Generic;
using Winner.Channel;

namespace Winner.Cluster
{
    public class ClusterInfo
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
        /// 过滤
        /// </summary>
        public Action<ClusterInfo, ClusterProtocolInfo> Filter { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Action<ClusterInfo, EndPointInfo, bool> Result { get; set; }
        /// <summary>
        /// 过滤
        /// </summary>
        public Func<ClusterInfo, IList<EndPointInfo>,IList<EndPointInfo>> FilterEndPoints { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Action<ClusterInfo, IList<EndPointInfo>, int, string> Log { get; set; }
        /// <summary>
        /// 是否异步
        /// </summary>
        public bool IsAsync { get; set; }
    }
}
