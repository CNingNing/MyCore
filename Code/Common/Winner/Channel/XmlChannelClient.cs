using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;

namespace Winner.Channel
{

    public class XmlChannelClient : ChannelClient
    {
        #region 属性
  
        private string _configFile;
        public string ConfigFile
        {
            get { return _configFile; }
            set
            {
                _configFile = value;
                LoadConfig();
            }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public XmlChannelClient()
        {
     
        }
        /// <summary>
        /// WCF客户端配置文件,虚拟节点配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlChannelClient( string configFile)
        {
            ConfigFile = configFile;
            
        }
        #endregion

        #region 加载配置文件
        /// <summary>
        /// 根据配置文件加载
        /// </summary>
        protected virtual void LoadConfig()
        {
            var endPoints = new ConcurrentDictionary<string, IList<EndPointInfo>>();
            XmlDocument doc = GetXmlDocument();
            XmlNodeList nodes = doc.SelectNodes("/configuration/Channel/EndPoint/Info");
            AddEndPointByXml(nodes, endPoints);
            SetEndPointsFailoverByXmlNodes(nodes, endPoints);
            EndPoints = endPoints;
        }
        /// <summary>
        /// 得到XmlDocument
        /// </summary>
        /// <returns></returns>
        protected virtual XmlDocument GetXmlDocument()
        {
            return Creator.GetXmlDocument(ConfigFile);
        }

        /// <summary>
        /// 根据XML加载Node属性
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="endPoints"></param>
        protected virtual void AddEndPointByXml(XmlNodeList nodes, IDictionary<string, IList<EndPointInfo>> endPoints)
        {
            if (nodes == null || nodes.Count == 0) return;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                {
                    if (!endPoints.ContainsKey(node.Attributes["Name"].Value))
                        endPoints.Add(node.Attributes["Name"].Value, new List<EndPointInfo>());
                    var beginPort = node.Attributes["Port"]!=null ? int.Parse(node.Attributes["Port"].Value): int.Parse(node.Attributes["BeginPort"].Value);
                    var endPort = node.Attributes["Port"] != null ? int.Parse(node.Attributes["Port"].Value): int.Parse(node.Attributes["EndPort"].Value);
                    var count = node.Attributes["Count"] == null ? 1 : int.Parse(node.Attributes["Count"].Value);
                    for (int port = beginPort; port<= endPort; port++)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var endPoint = new EndPointInfo();
                            endPoint.Name = node.Attributes["Name"].Value;
                            endPoint.Ip = node.Attributes["Ip"].Value;
                            endPoint.Port = port;
                            endPoint.Index = i;
                            endPoint.ConnectName = $"{endPoint.Ip}:{port}:{i}";
                            if (node.Attributes["BufferSize"] != null)
                                endPoint.BufferSize = int.Parse(node.Attributes["BufferSize"].Value);
                            if (node.Attributes["Timeout"] != null)
                                endPoint.Timeout = int.Parse(node.Attributes["Timeout"].Value);
                            //if (node.Attributes["KeepAliveTimes"] != null)
                            //    endPoint.KeepAliveTimes = int.Parse(node.Attributes["KeepAliveTimes"].Value);
                            if (node.Attributes["SleepTimes"] != null)
                                endPoint.SleepTimes = int.Parse(node.Attributes["SleepTimes"].Value);
                            if (node.Attributes["GroupName"] != null)
                                endPoint.GroupName = node.Attributes["GroupName"].Value;
                            if (node.Attributes["Args"] != null)
                                endPoint.Args = node.Attributes["Args"].Value;
                            endPoints[node.Attributes["Name"].Value].Add(endPoint);
                        }
                       
                    }
                }
            }
        }
        /// <summary>
        /// 设置故障转移
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="endPoints"></param>
        protected virtual void SetEndPointsFailoverByXmlNodes(XmlNodeList nodes, IDictionary<string, IList<EndPointInfo>> endPoints)
        {
            foreach (var endPoint in endPoints)
            {
                foreach (var point in endPoint.Value)
                { 
                    point.Failovers = new List<EndPointInfo>();
                    foreach (var failoverPoint in endPoint.Value)
                    {
                        if (point== failoverPoint)
                            continue;
                        if (!string.IsNullOrWhiteSpace(point.GroupName) &&
                            string.IsNullOrWhiteSpace(failoverPoint.GroupName) &&
                            point.GroupName.Equals(failoverPoint.GroupName)
                            || point.Name.Equals(failoverPoint.Name) && point.Ip.Equals(failoverPoint.Ip) && point.Port==failoverPoint.Port)
                        {
                            point.Failovers.Add(failoverPoint);
                        }
                    }
                }
            }

        }
        #endregion

    }
}
