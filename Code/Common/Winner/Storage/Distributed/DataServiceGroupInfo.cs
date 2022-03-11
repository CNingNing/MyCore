using System.Collections.Generic;

namespace Winner.Storage.Distributed
{
    public class DataServiceGroupInfo
    {
        /// <summary>
        /// 针对目录
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 是否打开
        /// </summary>
        public bool IsClose { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 数据节点
        /// </summary>
        public IList<DataServiceInfo> DataServices { get; set; }
        /// <summary>
        /// 拥有地址
        /// </summary>
        public string[] Addresses { get; set; }
    }
}
