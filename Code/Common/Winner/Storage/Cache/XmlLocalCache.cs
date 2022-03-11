using System;
using System.Collections.Generic;
using System.Xml;

namespace Winner.Storage.Cache
{
    public class XmlLocalCache : LocalCache
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
        public XmlLocalCache()
        { 
        }

        /// <summary>
        /// WCF客户端配置文件,虚拟节点配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlLocalCache(string configFile)
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
            LoadCacheByXml(doc);
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
        protected virtual void LoadCacheByXml(XmlDocument doc)
        {
            XmlNode node = doc.SelectSingleNode("/configuration/Storage/XmlLocalCache");
            if (node == null || node.Attributes == null)
                return;
            Caches = GetCachesByXmlNode(node);
        }
     

        /// <summary>
        /// 根据节点得到缩略图
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual IList<CacheInfo> GetCachesByXmlNode(XmlNode node)
        {
            IList<CacheInfo> caches = new List<CacheInfo>();
            XmlNodeList nodes = node.SelectNodes("Info");
            if (nodes == null || nodes.Count == 0)
                return caches;
            foreach (XmlNode nd in nodes)
            {
                AddCacheByXmlNode(caches, nd);
            }
            return caches;
        }
        /// <summary>
        ///  根据节点添加缩略图
        /// </summary>
        /// <param name="caches"></param>
        /// <param name="node"></param>
        protected virtual void AddCacheByXmlNode(IList<CacheInfo> caches, XmlNode node)
        {
            var ds = new CacheInfo();
            if (node != null && node.Attributes != null)
            {
                ds.Path = node.Attributes["Path"].Value;
                ds.Times = Convert.ToInt32(node.Attributes["Times"].Value);
            }
            caches.Add(ds);
        }
        #endregion
    }
}
