using System;
using System.Collections.Generic;
using System.Xml;

namespace Winner.Mail
{
    public class XmlMail : Mail
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
        public XmlMail()
        { 
           Servers=new List<ServerInfo>();
        }

        /// <summary>
        /// 存储路径，缩略图信息
        /// </summary>
        /// <param name="servers"></param>
        ///  <param name="configFile"></param>
        public XmlMail(IList<ServerInfo> servers, string configFile)
        {
            Servers = servers;
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
            LoadServersByXml(doc);
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
        /// 加载服务器信息
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void LoadServersByXml(XmlDocument doc)
        {
            XmlNodeList nodes = doc.SelectNodes("/configuration/Mail/XmlMail/Server");
            if (nodes == null || nodes.Count == 0)
                return;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                    AddServerByXmlNode(node);
            }
        }

        /// <summary>
        ///  根据节点添加服务器
        /// </summary>
        /// <param name="node"></param>
        protected virtual void AddServerByXmlNode(XmlNode node)
        {
            var server = new ServerInfo();
            if (node != null && node.Attributes != null)
            {
                server.FromMail = node.Attributes["FromMail"].Value;
                server.Password = node.Attributes["Password"].Value;
                server.SmtpHost = node.Attributes["SmtpHost"].Value;
                server.UserName = node.Attributes["UserName"].Value;
                server.DisplayName = node.Attributes["DisplayName"] != null ? node.Attributes["DisplayName"].Value : "";
                server.Port = node.Attributes["Port"]!=null? int.Parse(node.Attributes["Port"].Value):25;
                server.EnableSsl = node.Attributes["EnableSsl"] != null ? bool.Parse(node.Attributes["EnableSsl"].Value) : false;
            }
            Servers.Add(server);
        }
        #endregion
    }
}
