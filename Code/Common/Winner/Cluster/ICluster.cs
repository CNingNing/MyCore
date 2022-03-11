using System.Collections.Generic;
using Winner.Channel;

namespace Winner.Cluster
{
    public interface ICluster
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        bool Start(ClusterInfo info);
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IList<EndPointInfo> GetNodes(string name);
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        bool Check(string name, int index);

        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        bool Start(string name, int index);
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        bool Stop(string name,int index);
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        string GetStatus(string name, int index);
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        string Print(string name, int index);

    }
}
