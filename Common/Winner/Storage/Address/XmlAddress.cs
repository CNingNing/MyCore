using System;
using System.Collections.Generic;
using System.Xml;

namespace Winner.Storage.Address
{
    public class XmlAddress : Address
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
        public XmlAddress()
        { 
        }

        /// <summary>
        /// WCF客户端配置文件,虚拟节点配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlAddress(string configFile)
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
            LoadAddressByXml(doc);
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
        protected virtual void LoadAddressByXml(XmlDocument doc)
        {
            XmlNode node = doc.SelectSingleNode("/configuration/Storage/XmlAddress");
            if (node == null || node.Attributes == null)
                return;
            Addresses = GetAddressesByXmlNode(node);
        }
     

        /// <summary>
        /// 根据节点得到缩略图
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual IDictionary<string, AddressInfo> GetAddressesByXmlNode(XmlNode node)
        {
            IDictionary<string, AddressInfo> domains = new Dictionary<string, AddressInfo>();
            XmlNodeList nodes = node.SelectNodes("Info");
            if (nodes == null || nodes.Count == 0)
                return domains;
            foreach (XmlNode nd in nodes)
            {
                AddAddressByXmlNode(domains, nd);
            }
            return domains;
        }
        /// <summary>
        ///  根据节点添加缩略图
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="node"></param>
        protected virtual void AddAddressByXmlNode(IDictionary<string, AddressInfo> addresses, XmlNode node)
        {
            if (node != null && node.Attributes != null)
            {
                if (node.Attributes["Count"] == null)
                {
                    AddSingleAddressByXmlNode(addresses, node);
                }
                else
                {
                    AddBatchAddressByXmlNode(addresses, node);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="node"></param>
        protected virtual void AddSingleAddressByXmlNode(IDictionary<string, AddressInfo> addresses, XmlNode node)
        {
            if (node != null && node.Attributes != null)
            {
                var address = new AddressInfo
                {
                    Url = node.Attributes["Url"] == null ? null : node.Attributes["Url"].Value,
                    Name = node.Attributes["Name"] == null ? null : node.Attributes["Name"].Value,
                    GroupName = node.Attributes["GroupName"] == null ? null : node.Attributes["GroupName"].Value
                };
                addresses.Add(address.Name, address);
               
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="node"></param>
        protected virtual void AddBatchAddressByXmlNode(IDictionary<string, AddressInfo> addresses, XmlNode node)
        {
            if (node != null && node.Attributes != null)
            {
                var startIndex = node.Attributes["StartIndex"]==null?0:int.Parse(node.Attributes["StartIndex"].Value);
                for (int i = 0; i < int.Parse(node.Attributes["Count"].Value); i++)
                {
                    var index = startIndex + i;
                    var address = new AddressInfo
                    {
                        Url = node.Attributes["Url"] == null ? null : string.Format(node.Attributes["Url"].Value, index),
                        Name = node.Attributes["Name"] == null ? null : string.Format(node.Attributes["Name"].Value, index),
                        GroupName = node.Attributes["GroupName"] == null ? null : node.Attributes["GroupName"].Value
                    };
                    addresses.Add(address.Name, address);
                }

            }
        }
        #endregion
    }
}
