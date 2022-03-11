using Newtonsoft.Json;
using Winner.Channel;

namespace Winner.Cluster
{
    public class ClusterArgsInfo
    {
       /// <summary>
       /// 
       /// </summary>
        public ClusterProtocolInfo Protocol { get; set; }
       /// <summary>
       /// 
       /// </summary>
       public ClusterNodeInfo Node { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReceiveInfo Receive { get; set; }

    }
}
