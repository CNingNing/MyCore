using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Winner.Channel;
using Winner.Log;

namespace Winner.Cluster
{
    public class ClusterService 
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
        /// <summary>
        /// 节点
        /// </summary>
        public IDictionary<string,ClusterNodeInfo> Handles { get; set; } = new ConcurrentDictionary<string, ClusterNodeInfo>();

        public virtual void Handle(ChannelArgsInfo args)
        {
            try
            {
                var value = args.GetResult();
                if (string.IsNullOrWhiteSpace(value))
                    return;
                ClusterProtocolInfo protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<ClusterProtocolInfo>(value);
                if(protocol==null)
                    return;
                if(!Handles.ContainsKey(protocol.Name) || Handles[protocol.Name] == null)
                    return;
                ClusterArgsInfo clusterArgs = new ClusterArgsInfo
                {
                    Node = Handles[protocol.Name],
                    Receive = args.Receive,
                    Protocol = protocol
                };
                switch (args.Method)
                {
                    case 'e':
                    {
                        if (!Handles.ContainsKey(protocol.Name) || Handles[protocol.Name] == null ||
                            Handles[protocol.Name].Execute == null)
                        {
                            args.SetArgs(false.ToString());
                            return;
                        }
                        if (Handles[protocol.Name].IsStop)
                        {
                            Handles[protocol.Name].AddLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:Stop UnExecute");
                            args.SetArgs(false.ToString());
                            return;
                        }
                        Handles[protocol.Name].AddLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:Execute");
                            var rev=Execute(clusterArgs);
                        args.SetArgs(rev.ToString());
                    }
                        break;
                    case 'c':
                        args.SetArgs(Check(clusterArgs).ToString());
                        break;
                    case 'a':
                        args.SetArgs(Start(clusterArgs).ToString());
                        break;
                    case 't':
                        args.SetArgs(Stop(clusterArgs).ToString());
                        break;
                    case 'p':
                        args.SetArgs(Print(clusterArgs));
                        break;
                    case 'g':
                        args.SetArgs(GetStatus(clusterArgs));
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
            }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="info"></param>
        protected virtual bool Execute(ClusterArgsInfo info)
        {


            if (Handles[info.Protocol.Name].IsAsync)
            {
                AsyncExecute(info);
                return true;
            }
            else
            {
               return SyncExecute(info);
            }
        }
        /// <summary>
        /// 同步执行
        /// </summary>
        /// <param name="info"></param>
        protected virtual bool SyncExecute(ClusterArgsInfo info)
        {
            try
            {
    
                var rev=Handles[info.Protocol.Name].Execute(info);
                if (Handles[info.Protocol.Name].Response != null)
                {
                    Handles[info.Protocol.Name].Response(info);
                }
                return rev;
            }
            catch (Exception e)
            {

                Log.AddException(e);
            }
            finally
            {
    
            }
            return false;
        }
        /// <summary>
        /// 异步执行
        /// </summary>
        /// <param name="info"></param>
        protected virtual void AsyncExecute(ClusterArgsInfo info)
        {
 
            var task = new Thread(() =>
            {
                try
                {
                    Handles[info.Protocol.Name].Execute(info);
                    if (Handles[info.Protocol.Name].Response != null)
                    {
                        Handles[info.Protocol.Name].Response(info);
                    }
                }
                catch (Exception e)
                {                                                                                                                                 
                    Log.AddException(e);
                }
                finally
                {
   
                }

            });
            task.Start();
        }

        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual bool Check(ClusterArgsInfo info)
        {
            if (Handles[info.Protocol.Name].Check == null)
                return true;
            return Handles[info.Protocol.Name].Check(info);
        }
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual bool Start(ClusterArgsInfo info)
        {
            Handles[info.Protocol.Name].IsStop = false;
            if (Handles[info.Protocol.Name].Start == null)
                return true;
            return Handles[info.Protocol.Name].Start(info);
        }
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual bool Stop(ClusterArgsInfo info)
        {
            Handles[info.Protocol.Name].IsStop = true;
            if (Handles[info.Protocol.Name].Stop == null)
                return true;
            return Handles[info.Protocol.Name].Stop(info);
        }
       
        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual string Print(ClusterArgsInfo info)
        {
            if (Handles[info.Protocol.Name].Print == null)
                return Handles[info.Protocol.Name].GetLog();
            return Handles[info.Protocol.Name].Print(info);
        }

        /// <summary>
        /// 检查状态
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual string GetStatus(ClusterArgsInfo info)
        {
            if (Handles[info.Protocol.Name].GetStatus == null)
                return $"{(Handles[info.Protocol.Name].IsStop ? "Stop" : "Start")}";
            return Handles[info.Protocol.Name].GetStatus(info);
        }
    }
}
