using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Winner.Creation
{
    

    public class XmlFactory : Factory 
    {
        #region 属性
        private string _configFile;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public virtual string ConfigFile
        {
            get { return _configFile; }
            set 
            { 
                _configFile = value;
                LoadConfig();
            }

        }
        #endregion

        #region 加载配置
        /// <summary>
        /// 加载配置文件
        /// </summary>
        protected virtual void LoadConfig()
        {
            XmlDocument doc = GetXmlDocument();
            LoadByXml(doc);
 
        }
        /// <summary>
        /// 得到XmlDocument
        /// </summary>
        /// <returns></returns>
        protected virtual XmlDocument GetXmlDocument()
        {
            return Creator.GetXmlDocument(ConfigFile);
        }
        
        #endregion


        #region 加载IOC

        /// <summary>
        /// 加载IOC
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void LoadByXml(XmlDocument doc)
        {
            XmlNodeList xnPaths = doc.SelectNodes("/configuration/Creation/XmlFactory/Path");
            if (xnPaths == null || xnPaths.Count == 0)
                return;
            foreach (XmlNode node in xnPaths)
            {
                if (node.Attributes == null) continue;
                var nodes = GetIocXmlNodesByXmlPath(node.Attributes["Path"].Value);
                AddIocByXmlNodes(nodes);
            }
            foreach (XmlNode node in xnPaths)
            {
                XmlNodeList nodes = GetPropertyXmlNodes(node);
                SetFactoryPropertiesByXmlNodes(nodes);
            }
            foreach (var info in Factories.Values)
            {
                if(!info.IsSingle)
                    continue;
                info.Target = CreateTarget(info);
            }
            foreach (var info in Factories.Values)
            {
                if (!info.IsSingle)
                    continue;
                TrySetProperties(info.Target, info.Properties);
                TrySetAops(info.Aops);
            }
        }

        /// <summary>
        /// 得到IOC节点
        /// </summary>
        /// <returns></returns>
        protected virtual XmlNodeList GetIocXmlNodesByXmlPath(string path)
        {
            if (path == null) return null;
            XmlDocument doc = Creator.GetXmlDocument(path);
            XmlNodeList nodes = doc.SelectNodes("/configuration/Creation/XmlFactory/Ioc/Instance");
            return nodes;
        }

        /// <summary>
        /// 创建IOC实例
        /// </summary>
        /// <param name="nodes"></param>
        protected virtual void AddIocByXmlNodes(XmlNodeList nodes)
        {
            if (nodes == null || nodes.Count == 0)return;
            foreach (XmlNode node in nodes)
            {
                var info = new FactoryInfo();
                CreateByXmlNode(node, info);
            }
        }
 
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="node"></param>
        /// <param name="info"></param>
        protected virtual void CreateByXmlNode(XmlNode node,FactoryInfo info)
        {
            if (node.Attributes == null) return;
            info.ClassName = node.Attributes["ClassName"].Value;
            info.IsSingle = node.Attributes["IsSingle"] == null || Convert.ToBoolean(node.Attributes["IsSingle"].Value);
            info.Name = node.Attributes["Name"].Value;
            Set(info);
        }
        #endregion

        #region 加载Ioc属性
  
        /// <summary>
        /// 得到属性节点
        /// </summary>
        /// <returns></returns>
        protected virtual XmlNodeList GetPropertyXmlNodes(XmlNode node)
        {
            if (node.Attributes == null)
                return null;
            XmlDocument doc = Creator.GetXmlDocument(node.Attributes["Path"].Value);
            XmlNodeList nodes = doc.SelectNodes("/configuration/Creation/XmlFactory/Ioc/Instance");
            return nodes;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="alreadyFactories"></param>
        protected virtual void SetFactoryPropertiesByXmlNodes(XmlNodeList nodes)
        {
            if (nodes == null || nodes.Count == 0) return ;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes == null) continue;
                var info = Factories.ContainsKey(node.Attributes["Name"].Value) ? Factories[node.Attributes["Name"].Value] : null;
                if (info == null) continue;
                info.Properties = GetFactoryPropertiesByXmlNodes(node.SelectNodes("Property"));
                info.Aops = GetFactoryAopsByXmlNodes(node.SelectNodes("Aop"));
            }
        }



        /// <summary>
        /// 创建属性
        /// </summary>
        /// <param name="nodes"></param>
        protected virtual IList<FactoryPropertyInfo> GetFactoryPropertiesByXmlNodes(XmlNodeList nodes)
        {
            if (nodes == null || nodes.Count == 0) return null;
            return (from XmlNode node in nodes
                    let attributes = node.Attributes
                    where attributes != null
                    where attributes != null
                    select new FactoryPropertyInfo
                        {
                            Name = attributes["Name"].Value, Value = attributes["Value"].Value, 
                            Properties = GetFactoryPropertiesByXmlNodes(node.SelectNodes("Property"))
                        }).ToList();
        }
        /// <summary>
        /// 创建属性
        /// </summary>
        /// <param name="nodes"></param>
        protected virtual IList<AopInfo> GetFactoryAopsByXmlNodes(XmlNodeList nodes)
        {
            if (nodes == null || nodes.Count == 0) return null;
            return (from XmlNode node in nodes
                let attributes = node.Attributes
                where attributes != null
                where attributes != null
                select new AopInfo
                {
                    Name = attributes["Name"].Value,
                    Value = attributes["Value"].Value,
                    Method = attributes["Method"].Value,
                    IsAsync = attributes["IsAsync"] != null &&
                              Convert.ToBoolean(attributes["IsAsync"].Value),
                    Type = (AopType)Enum.Parse(typeof(AopType), attributes["Type"].Value)
                }).ToList();
        }

        #endregion

       

    }
}
