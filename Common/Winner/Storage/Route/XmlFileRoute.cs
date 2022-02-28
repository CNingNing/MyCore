using System;
using System.Collections.Generic;
using System.Xml;

namespace Winner.Storage.Route
{
    public class XmlFileRoute : FileRoute
    {

        #region 属性

        public string ClientFile { get; set; }
        public string NodeName { get; set; }
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
        public XmlFileRoute()
        { 
        }

        /// <summary>
        /// WCF客户端配置文件,虚拟节点配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlFileRoute(string configFile)
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
            LoadFileRoutesByXml(doc);
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
        /// 加载缩略图信息
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void LoadFileRoutesByXml(XmlDocument doc)
        {
            XmlNode node = doc.SelectSingleNode("/configuration/Storage/XmlFileRoute");
            if (node == null || node.Attributes == null)
                return;
            FileRoutes = GetFileRoutesByXmlNode(node);
        }
     

        /// <summary>
        /// 根据节点得到缩略图
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual IList<FileRouteInfo> GetFileRoutesByXmlNode(XmlNode node)
        {
            IList<FileRouteInfo> fileRoutes = new List<FileRouteInfo>();
            XmlNodeList nodes = node.SelectNodes("Info");
            if (nodes == null || nodes.Count == 0)
                return fileRoutes;
            foreach (XmlNode nd in nodes)
            {
                AddFileRouteByXmlNode(fileRoutes, nd);
            }
            return fileRoutes;
        }
        /// <summary>
        ///  根据节点添加缩略图
        /// </summary>
        /// <param name="fileRoutes"></param>
        /// <param name="node"></param>
        protected virtual void AddFileRouteByXmlNode(IList<FileRouteInfo> fileRoutes, XmlNode node)
        {
            if (node != null && node.Attributes != null)
            {
                var fileRoute = new FileRouteInfo
                {
                    Path = node.Attributes["Path"] == null ? null : node.Attributes["Path"].Value
                };
                fileRoutes.Add(fileRoute);
            }
        }
        #endregion
    }
}
