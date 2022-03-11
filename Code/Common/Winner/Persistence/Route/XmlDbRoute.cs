using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace Winner.Persistence.Route
{
    /// <summary>
    /// 加载ORM
    /// </summary>
    public class XmlDbRoute: DbRoute
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
        public XmlDbRoute()
        { 
        }
        /// <summary>
        /// 配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlDbRoute(string configFile)
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
            LoadDbRoutes(doc);
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
        protected virtual void LoadDbRoutes(XmlDocument doc)
        {
            XmlNodeList xnPaths = doc.SelectNodes("/configuration/Persistence/XmlDbRoute/Path");
            if (xnPaths == null || xnPaths.Count == 0)
                return;
            var dbRoutes = new Dictionary<string, DbRouteInfo>();
            foreach (XmlNode node in xnPaths)
            {
                if (node.Attributes == null)
                    return ;
                XmlDocument pathDoc = Creator.GetXmlDocument(node.Attributes["Path"].Value);
                LoadDbRouteByXml(dbRoutes, pathDoc);
            }
            DbRoutes = dbRoutes;
        }
        #endregion

        #region 得到配置信息

        /// <summary>
        /// 得到数据库信息
        /// </summary>
        /// <param name="dbRoutes"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected virtual void LoadDbRouteByXml(IDictionary<string, DbRouteInfo> dbRoutes,XmlDocument doc)
        {
            XmlNodeList nodes = doc.SelectNodes("/configuration/Persistence/XmlDbRoute/Info");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    var dbroute = new DbRouteInfo { Rules = new List<RuleInfo>()};
                    if (node.Attributes == null) return;
                    dbroute.Name = node.Attributes["Name"].Value;
                    if (node.Attributes["TopCount"] != null)
                        dbroute.TopCount = Convert.ToInt32(node.Attributes["TopCount"].Value);
                    if (node.Attributes["Type"] != null)
                        dbroute.Type = (DbRouteType)Enum.Parse(typeof(DbRouteType), node.Attributes["Type"].Value);
                    dbroute.GetDataBase = node.Attributes["GetDataBase"] != null
                        ? node.Attributes["GetDataBase"].Value
                        : "";
                    dbroute.SetDataBase = node.Attributes["SetDataBase"] != null
                        ? node.Attributes["SetDataBase"].Value
                        : "";
                    dbroute.TableIndex = node.Attributes["TableIndex"] != null
                        ? int.Parse(node.Attributes["TableIndex"].Value)
                        : 0;
                    dbroute.TableCount = node.Attributes["TableCount"] != null
                        ? int.Parse(node.Attributes["TableCount"].Value)
                        : 1;
                    if (node.Attributes["DataBaseTableCount"] != null)
                        dbroute.DataBaseTableCount = Convert.ToInt32(node.Attributes["DataBaseTableCount"].Value);
                    if (node.Attributes["ClassName"] != null)
                    {
                        var obj = CreateClass(node.Attributes["ClassName"].Value);
                        if (obj != null)
                        {
                            if (node.Attributes["GetQueryShardingHandle"] != null)
                                dbroute.GetQueryShardingHandle = (Func<QueryInfo, IList<ShardingInfo>>)Delegate.CreateDelegate(typeof(Func<QueryInfo, IList<ShardingInfo>>), obj, node.Attributes["GetQueryShardingHandle"].Value);
                            if (node.Attributes["GetSaveShardingHandle"] != null)
                                dbroute.GetSaveShardingHandle = (Func<object, ShardingInfo>)Delegate.CreateDelegate(typeof(Func<object, ShardingInfo>), obj, node.Attributes["GetSaveShardingHandle"].Value);
                        }
                   }
                    LoadRulesByXmlNode(dbroute, node);
                    if (dbRoutes.ContainsKey(dbroute.Name))
                        dbRoutes.Remove(dbroute.Name);
                    dbRoutes.Add(dbroute.Name, dbroute);
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
        /// <summary>
        /// 根据节点得到OrmDataBase
        /// </summary>
        /// <param name="dbRoute"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual void LoadRulesByXmlNode(DbRouteInfo dbRoute, XmlNode node)
        {
            var nodes = node.SelectNodes("Rule");
            if(nodes==null)return;
            foreach (XmlNode nd in nodes)
            {
                var rule = new RuleInfo();
                rule.IsHash = nd.Attributes["IsHash"] != null && Convert.ToBoolean(nd.Attributes["IsHash"].Value);
                rule.PropertyName = nd.Attributes["PropertyName"] != null ? nd.Attributes["PropertyName"].Value : "";
                rule.UnRouteValue = nd.Attributes["UnRouteValue"] != null ? nd.Attributes["UnRouteValue"].Value : "";
                rule.Tag = nd.Attributes["Tag"] != null
                    ? nd.Attributes["Tag"].Value
                    : "";
                if (nd.Attributes["StartValue"] != null)
                    rule.StartValue = Convert.ToInt64(nd.Attributes["StartValue"].Value);
                if (nd.Attributes["EndValue"] != null)
                    rule.EndValue = Convert.ToInt64(nd.Attributes["EndValue"].Value);
                if (nd.Attributes["FixedValue"] != null)
                    rule.FixedValue = nd.Attributes["FixedValue"].Value;
                if (nd.Attributes["ShardingType"] != null)
                    rule.ShardingType = (ShardingType)Enum.Parse(typeof(ShardingType), nd.Attributes["ShardingType"].Value);
                if (nd.Attributes["RuleType"] != null)
                    rule.RuleType = (RuleType)Enum.Parse(typeof(RuleType), nd.Attributes["RuleType"].Value);
                dbRoute.Rules.Add(rule);
            }
        }
  
        #endregion

    }
}
