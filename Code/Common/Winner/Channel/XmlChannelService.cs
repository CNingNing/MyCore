using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;
using Winner.Persistence.Compiler.Common;

namespace Winner.Channel
{

    public class XmlChannelService : ChannelService
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
        public XmlChannelService()
        {
     
        }
        /// <summary>
        /// WCF客户端配置文件,虚拟节点配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlChannelService( string configFile)
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
            var listens = new List<ListenInfo>();
            XmlDocument doc = GetXmlDocument();
            AddListenByXml(doc, listens);
            Listens = listens;
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
        /// <param name="doc"></param>
        /// <param name="listens"></param>
        protected virtual void AddListenByXml(XmlDocument doc, IList<ListenInfo> listens)
        {
            XmlNodeList nodes = doc.SelectNodes("/configuration/Channel/Listen/Info");
            if (nodes == null || nodes.Count == 0) return;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                {
                    var listen=new ListenInfo();
                    listen.Name = node.Attributes["Name"].Value;
                    listen.Ip = node.Attributes["Ip"].Value;
                    listen.Port = int.Parse(node.Attributes["Port"].Value) ;
                    if (node.Attributes["Count"]!=null)
                        listen.Count= int.Parse(node.Attributes["Count"].Value);
                    if (node.Attributes["BufferSize"] != null)
                        listen.BufferSize = int.Parse(node.Attributes["BufferSize"].Value);
                    if (node.Attributes["Timeout"] != null)
                        listen.Timeout = int.Parse(node.Attributes["Timeout"].Value);
                    if (node.Attributes["KeepAliveTimes"] != null)
                        listen.KeepAliveTimes = int.Parse(node.Attributes["KeepAliveTimes"].Value);
                    if (node.Attributes["ClassName"] != null)
                    {
                        var obj = CreateClass(node.Attributes["ClassName"].Value);
                        if (obj != null)
                        {
                            if (node.Attributes["Method"] != null)
                                listen.Handle = (Action<ChannelArgsInfo>) Delegate.CreateDelegate(typeof(Action<ChannelArgsInfo>), obj, node.Attributes["Method"].Value);
                        }
                        XmlNodeList propertyNodes = node.SelectNodes("Property");
                        if(propertyNodes!=null)
                        {
                            foreach (XmlNode propertyNode in propertyNodes)
                            {
                                if (propertyNode.Attributes != null && propertyNode.Attributes["Name"] != null && propertyNode.Attributes["Value"] != null
                                   && !string.IsNullOrWhiteSpace(propertyNode.Attributes["Name"].Value) && !string.IsNullOrWhiteSpace(propertyNode.Attributes["Value"].Value))
                                    obj.SetProperty(propertyNode.Attributes["Name"].Value, propertyNode.Attributes["Value"].Value);
                            }
                        }
                    }
                    listens.Add(listen);
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
