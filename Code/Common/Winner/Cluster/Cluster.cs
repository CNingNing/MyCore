using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Winner.Channel;
using Winner.Log;

namespace Winner.Cluster
{
    public class Cluster : ICluster
    {
        private ILog _log;

        /// <summary>
        /// 实例
        /// </summary>
        public ILog Log
        {
            get
            {
                if (_log == null)
                    _log = Creator.Get<ILog>();
                return _log;
            }
            set { _log = value; }
        }
        public IChannelClient ChannelClient { get; set; }

        private const string ChannelName = "ClusterService";
        /// <summary>
        /// 节点
        /// </summary>
        public IDictionary<string, ClusterNodeInfo> Handles { get; set; } = new ConcurrentDictionary<string, ClusterNodeInfo>();
        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual bool Start(ClusterInfo info)
        {
            if (info == null || ChannelClient == null)
                return false;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            if (endPoints == null)
                return false;
            var orgPoints = endPoints;
            if (Handles.ContainsKey(info.Name) && Handles[info.Name].EndPoints != null &&
                Handles[info.Name].EndPoints.Count > 0)
            {
                endPoints = endPoints.Where(endPoint =>
                    Handles[info.Name].EndPoints.ContainsKey(endPoint.Ip) &&
                    Handles[info.Name].EndPoints[endPoint.Ip].Contains(endPoint.Port)).ToList();
            }

            if (info.FilterEndPoints != null)
                endPoints = info.FilterEndPoints(info,endPoints);
            if (endPoints == null || endPoints.Count==0)
                return false;
            for (int i = 0; i < endPoints.Count; i++)
            {
                SelectExecute(info, endPoints, i, orgPoints.IndexOf(endPoints[i]), endPoints.Count);
            }
            return true;
        }

        protected virtual void SelectExecute(ClusterInfo info, IList<EndPointInfo> endPoints, int index, int endPointIndex, int count)
        {
            if (info.IsAsync)
            {
                var task = new Thread(() => {
                    var rev = Execute(info, endPoints, index, endPointIndex,count);
                    if (info.Result != null)
                    {
                        info.Result(info, endPoints[index], rev);
                    }
                });
                task.Start();
            }
            else
            {
                var rev = Execute(info, endPoints, index, endPointIndex, count);
                if (info.Result != null)
                {
                    info.Result(info, endPoints[index], rev);
                }
            }
        }
     
        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="info"></param>
        /// <param name="endPoints"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual bool Execute(ClusterInfo info, IList<EndPointInfo> endPoints, int index,int endPointIndex, int count)
        {

            try
            {
                var rev = Handle(endPoints[index], 'c', new ClusterProtocolInfo { Name = info.Name });
                if (rev == null || bool.Parse(rev) == false)
                {
                    if (info.Log != null)
                    {
                        info.Log(info, endPoints, index, "Execute-Check-Failure");
                    }
                    return false;
                }
                return ExecuteNode(info, endPoints, index, endPointIndex, count);
            }
            catch (Exception ex)
            {
                Winner.Creator.Get<ILog>().AddException(ex);

            }
            finally
            {

            }
            return false;
        }
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual IList<EndPointInfo> GetNodes(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || ChannelClient == null)
                return null;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            if (endPoints == null)
                return null;
            if (Handles.ContainsKey(name) && Handles[name].EndPoints != null &&
                Handles[name].EndPoints.Count > 0)
            {
                endPoints = endPoints.Where(endPoint =>
                    Handles[name].EndPoints.ContainsKey(endPoint.Ip) &&
                    Handles[name].EndPoints[endPoint.Ip].Contains(endPoint.Port)).ToList();
            }
            return endPoints;
        }

        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual bool Check(string name,int index)
        {
            if (string.IsNullOrWhiteSpace(name) || ChannelClient == null)
                return false;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            if (endPoints == null || index>=endPoints.Count)
                return false;
            var rev = Handle(endPoints[index], 'c', new ClusterProtocolInfo { Name = name });
            if (rev == null || bool.Parse(rev) == false)
                return false;
            return true;
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual bool Start(string name, int index)
        {
            if (string.IsNullOrWhiteSpace(name) || ChannelClient == null)
                return false;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            if (endPoints == null || index >= endPoints.Count)
                return false;
            var rev = Handle(endPoints[index], 'a', new ClusterProtocolInfo { Name = name });
            if (rev == null || bool.Parse(rev) == false)
                return false;
            return true;
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual bool Stop(string name, int index)
        {
            if (string.IsNullOrWhiteSpace(name) || ChannelClient == null)
                return false;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            if (endPoints == null || index >= endPoints.Count)
                return false;
            var rev = Handle(endPoints[index], 't', new ClusterProtocolInfo { Name = name });
            if (rev == null || bool.Parse(rev) == false)
                return false;
            return true;
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string GetStatus(string name, int index)
        {
            if (string.IsNullOrWhiteSpace(name) || ChannelClient == null)
                return null;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            if (endPoints == null || index >= endPoints.Count)
                return null;
            var rev = Handle(endPoints[index], 'g', new ClusterProtocolInfo { Name = name });
            return rev;
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string Print(string name, int index)
        {
            if (string.IsNullOrWhiteSpace(name) || ChannelClient == null)
                return null;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            if (endPoints == null || index >= endPoints.Count)
                return null;
            var rev = Handle(endPoints[index], 'p', new ClusterProtocolInfo { Name = name });
            return rev;
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="info"></param>
        /// <param name="endPoints"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual bool ExecuteNode(ClusterInfo info, IList<EndPointInfo> endPoints, int index,int endPointIndex, int count)
        {
            var args = new ClusterProtocolInfo
            {
                Name = info.Name,
                Count = count,
                Index = index,
                EndPointIndex = endPointIndex,
                Value = info.Value
            };
            if (info.Filter != null)
            {
                info.Filter(info, args);
            }
            var result = Handle(endPoints[index], 'e', args);
            var rev = result==null?false: bool.Parse(result);
            if (!rev)
            {
                if (info.Log != null)
                {
                    info.Log(info, endPoints, index, "Execute-ExecuteNode-Failure");
                }
            }
            return rev;
        }


        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="endPoint"></param>
        /// <param name="clusterArgs"></param>
        /// <returns></returns>
        protected virtual string Handle(EndPointInfo endPoint,char method, ClusterProtocolInfo clusterArgs)
        {
            var args = new ChannelArgsInfo {Method= method };
            var value = SerializeJson(clusterArgs);
            args.SetArgs(value);
            ChannelClient.Send(endPoint, args);
            var result = args.GetResult();
            return result;
        }

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        protected virtual string SerializeJson(object input)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(input);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                return null;
            }
        }
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        protected virtual T DeserializeJson<T>(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return default(T);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                return default(T);
            }
        }
    }
}
