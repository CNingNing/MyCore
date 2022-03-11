using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Winner.Cluster
{
    public class ClusterNodeInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否运行
        /// </summary>
        public bool IsStop { get; set; }
        /// <summary>
        /// 是否异步
        /// </summary>
        public bool IsAsync { get; set; } = true;
        /// <summary>
        /// 执行
        /// </summary>
        public Func<ClusterArgsInfo, string> Print { get; set; }
        /// <summary>
        /// 执行
        /// </summary>
        public Func<ClusterArgsInfo, bool> Stop { get; set; }
        /// <summary>
        /// 执行
        /// </summary>
        public Func<ClusterArgsInfo, bool> Start { get; set; }
        /// <summary>
        /// 执行
        /// </summary>
        public Func<ClusterArgsInfo,bool> Check { get; set; }
        /// <summary>
        /// 执行
        /// </summary>
        public Func<ClusterArgsInfo, string> GetStatus { get; set; }
        /// <summary>
        /// 执行
        /// </summary>
        public Func<ClusterArgsInfo,bool> Execute { get; set; }
        /// <summary>
        /// 响应
        /// </summary>
        public Action<ClusterArgsInfo> Response { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string,IList<int>> EndPoints { get; set; }

        public virtual int LogMaxCount { get; set; } = 100;

        public virtual ConcurrentQueue<string> Logs { get; set; } = new ConcurrentQueue<string>();


        private object PrintLocker { get; set; } = new object();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetLog()
        {
            lock (PrintLocker)
            {
                var sb=new StringBuilder();
                for (int i = 0; i < 100; i++)
                {
                    string result;
                    if(!Logs.TryDequeue(out result))
                        break;
                    sb.AppendLine(result);
                }
                return sb.ToString();
            }
        }
        private object AddPrintLocker { get; set; } = new object();

        public virtual void AddLog(string message)
        {
            lock (AddPrintLocker)
            {
                if(Logs.Count> LogMaxCount)
                    Logs.TryDequeue(out _);
                Logs.Enqueue(message);
            }
        }
    }
}
