using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Winner.Cluster
{
    /// <summary>
    /// 加载ORM
    /// </summary>
    public class XmlCluster : Cluster
    {
  
        #region 属性
        private string _configFile;
        /// <summary>
        /// 配置文件路径
        /// </summary>
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
        public XmlCluster()
        { 
        }
        /// <summary>
        /// 配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlCluster(string configFile)
        {
            ConfigFile = configFile;
        }
        #endregion

        #region 加载配置文件

        /// <summary>
        /// 加载配置文件
        /// </summary>
        protected virtual void LoadConfig()
        {
            XmlDocument doc = GetXmlDocument();
            LoadHandles(doc);
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
        /// 加载DbRoutes
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void LoadHandles(XmlDocument doc)
        {
            XmlNodeList xnPaths = doc.SelectNodes("/configuration/Cluster/XmlClusterService/Path");
            if (xnPaths == null || xnPaths.Count == 0)
                return;
            var handles = new ConcurrentDictionary<string, ClusterNodeInfo>();
            foreach (XmlNode node in xnPaths)
            {
                if (node.Attributes == null)
                    return ;
                XmlDocument pathDoc = Creator.GetXmlDocument(node.Attributes["Path"].Value);
                LoadHandlesByXml(handles, pathDoc);
            }
            Handles = handles;
        }
        #endregion

        #region 得到配置信息

        /// <summary>
        /// 得到数据库信息
        /// </summary>
        /// <param name="handles"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected virtual void LoadHandlesByXml(IDictionary<string, ClusterNodeInfo> handles, XmlDocument doc)
        {
            XmlNodeList nodes = doc.SelectNodes("/configuration/Cluster/XmlClusterService/Info");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null) return;
                    if (handles.ContainsKey(node.Attributes["Name"].Value))
                        continue;
                    if (node.Attributes["ClassName"] == null) continue;
                    var obj = CreateClass(node.Attributes["ClassName"].Value);
                    if (obj == null) continue;
                    if (node.Attributes["Execute"] == null) continue;
                    var info=new ClusterNodeInfo{Name= node.Attributes["Name"].Value };
                    info.Execute =
                        (Func<ClusterArgsInfo,bool>)
                            Delegate.CreateDelegate(typeof (Func<ClusterArgsInfo, bool>), obj,
                                node.Attributes["Execute"].Value);
                    handles.Add(info.Name, info);
                    if (node.Attributes["Response"] != null)
                    {
                        info.Response =
                            (Action<ClusterArgsInfo>)
                            Delegate.CreateDelegate(typeof(Action<ClusterArgsInfo>), obj,
                                node.Attributes["Response"].Value);
                    }
                    if (node.Attributes["Check"] != null)
                    {
                        info.Check =
                       (Func<ClusterArgsInfo, bool>)
                       Delegate.CreateDelegate(typeof(Func<ClusterArgsInfo, bool>), obj,
                           node.Attributes["Check"].Value);
                    }
                    if (node.Attributes["Start"] != null)
                    {
                        info.Start =
                            (Func<ClusterArgsInfo, bool>)
                            Delegate.CreateDelegate(typeof(Func<ClusterArgsInfo, bool>), obj,
                                node.Attributes["Start"].Value);
                    }
                    if (node.Attributes["Stop"] != null)
                    {
                        info.Stop =
                            (Func<ClusterArgsInfo, bool>)
                            Delegate.CreateDelegate(typeof(Func<ClusterArgsInfo, bool>), obj,
                                node.Attributes["Stop"].Value);
                    }
                    if (node.Attributes["Print"] != null)
                    {
                        info.Print =
                            (Func<ClusterArgsInfo, string>)
                            Delegate.CreateDelegate(typeof(Func<ClusterArgsInfo, string>), obj,
                                node.Attributes["Print"].Value);
                    }
                    if (node.Attributes["GetStatus"] != null)
                    {
                        info.GetStatus =
                            (Func<ClusterArgsInfo, string>)
                            Delegate.CreateDelegate(typeof(Func<ClusterArgsInfo, string>), obj,
                                node.Attributes["GetStatus"].Value);
                    }
                    if (node.Attributes["IsAsync"] != null &&
                        !string.IsNullOrWhiteSpace(node.Attributes["IsAsync"].Value))
                    {
                        info.IsAsync = bool.Parse(node.Attributes["IsAsync"].Value);
                    }
                    if (node.Attributes["LogMaxCount"] != null &&
                        !string.IsNullOrWhiteSpace(node.Attributes["LogMaxCount"].Value))
                    {
                        info.LogMaxCount = int.Parse(node.Attributes["LogMaxCount"].Value);
                    }
                    if (node.Attributes["EndPoints"] != null &&
                        !string.IsNullOrWhiteSpace(node.Attributes["EndPoints"].Value))
                    {
                        var vals = node.Attributes["EndPoints"].Value.Split(',');
                        info.EndPoints=new ConcurrentDictionary<string, IList<int>>();
                        foreach (var val in vals)
                        {
                            var points = val.Split(':');
                            if(!info.EndPoints.ContainsKey(points[0]))
                                info.EndPoints.Add(points[0],new List<int>());
                            var ports = points[1].Split('-');
                            var startPort = int.Parse(ports[0]);
                            var endPort = ports.Length > 1 ? int.Parse(ports[1]) : startPort;
                            for (int i = startPort; i <= endPort; i++)
                            {
                                info.EndPoints[points[0]].Add(i);
                            }

                        }
                    }
                }
            }
        
        }
        /// <summary>
        /// 创建类
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        protected virtual object CreateClass(string className)
        {
            var t = Type.GetType(className);
            if (t == null) return null;
            return Activator.CreateInstance(t);
        }
       
        #endregion

    }
}
