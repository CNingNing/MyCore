using System;
using System.Linq;
using System.Xml;

namespace Winner.Dislan
{
    public class XmlLanguage : Language
    {
        #region 配置文件
        private string _configFile;
        /// <summary>
        /// 配置文件
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

        #region 加载配置
        /// <summary>
        /// 加载配置
        /// </summary>
        protected virtual void LoadConfig()
        {
            XmlDocument doc = GetXmlDocument();
            LoadLanguageByXml(doc);
        }
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        protected virtual XmlDocument GetXmlDocument()
        {
            return Creator.GetXmlDocument(ConfigFile);
        }
        
        #endregion

        #region 加载配置

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void LoadLanguageByXml(XmlDocument doc)
        {
            XmlNodeList xnPaths = doc.SelectNodes("/configuration/Dislan/XmlLanguage/LanguagePath");
            if (xnPaths == null || xnPaths.Count == 0)
                return ;
            foreach (XmlNode node in xnPaths)
            {
                XmlNodeList nodes = GetLanguageXmlNodes(node);
                AddLanguageByXmlNodes(nodes);
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual XmlNodeList GetLanguageXmlNodes(XmlNode node)
        {
            if (node.Attributes == null)
                return null;
            XmlDocument doc = Creator.GetXmlDocument(node.Attributes["Path"].Value);
            XmlNodeList nodes = doc.SelectNodes("/configuration/Dislan/XmlLanguage/Language");
            return nodes;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="nodes"></param>
        protected virtual void AddLanguageByXmlNodes(XmlNodeList nodes)
        {
            if (nodes == null || nodes.Count == 0)return;
            foreach (XmlNode node in nodes)
            {
                FillNamesByXmlNode(node);
            }
        }
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="node"></param>
        protected virtual void FillNamesByXmlNode(XmlNode node)
        {
            var nodes = node.SelectNodes("Message");
            if (nodes == null || nodes.Count == 0) return;
            if (node.Attributes == null) return;
            var languages = (from XmlNode nd in nodes select GetLanguageInfoByXmlNode(nd)).ToList();
            AddLangs(node.Attributes["Name"].Value, languages);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual LanguageInfo GetLanguageInfoByXmlNode( XmlNode node)
        {
            var info = new LanguageInfo();
            if (node.Attributes != null)
            {
                info.Name = node.Attributes["Name"].Value;
                info.Message = node.Attributes["Value"].Value;
            }
            return info;
        }

        #endregion
    }
}
