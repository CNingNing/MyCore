using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace Winner.Persistence.Data
{
    /// <summary>
    /// 加载ORM
    /// </summary>
    public class XmlDataBase: DataBase
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
        public XmlDataBase()
        { 
        }
        /// <summary>
        /// 配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlDataBase(string configFile)
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
            LoadDataBaseByXml(doc);
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
   
        #region 得到配置信息

        /// <summary>
        /// 得到数据库信息
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected virtual void LoadDataBaseByXml(XmlDocument doc)
        {
            XmlNodeList nodes = doc.SelectNodes("/configuration/Persistence/XmlDataBase/Info");
            var dataBases = new Dictionary<string, OrmDataBaseInfo>();
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    AddOrmDataBaseByXmlNode(dataBases,node);
                }
                SetOrmDataBaseFailoverByXmlNodes(dataBases, nodes);
            }
            DataBases = dataBases;
        }

        /// <summary>
        /// 根据节点得到OrmDataBase
        /// </summary>
        /// <param name="dataBases"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual void AddOrmDataBaseByXmlNode(IDictionary<string, OrmDataBaseInfo> dataBases, XmlNode node)
        {
            if (node.Attributes == null) return;
            var db = new OrmDataBaseInfo();
            db.Name = node.Attributes["Name"].Value;
            db.ConnnectString = node.Attributes["ConnnectString"].Value;
            db.CompilerName = node.Attributes["CompilerName"] == null ? null : node.Attributes["CompilerName"].Value;
            db.CheckAlivePeriod =
                node.Attributes["CheckAlivePeriod"] == null ||
                string.IsNullOrEmpty(node.Attributes["CheckAlivePeriod"].Value)
                    ? db.CheckAlivePeriod
                    : Convert.ToInt32(node.Attributes["CheckAlivePeriod"].Value);
            if (node.Attributes["IsDefault"] != null && node.Attributes["IsDefault"].Value.Equals("true"))
                db.IsDefault = true;
            if (node.Attributes["IsGetLoadBalance"] != null && node.Attributes["IsGetLoadBalance"].Value.Equals("true"))
                db.IsGetLoadBalance = true;
            if (node.Attributes["IsSetLoadBalance"] != null && node.Attributes["IsSetLoadBalance"].Value.Equals("true"))
                db.IsSetLoadBalance = true;
            if (node.Attributes["BeginIndex"] != null && node.Attributes["EndIndex"] != null)
            {
                var beginIndex = int.Parse(node.Attributes["BeginIndex"].Value);
                var endIndex = int.Parse(node.Attributes["EndIndex"].Value);
                while (beginIndex <= endIndex)
                {
                    var dbIndex = new OrmDataBaseInfo();
                    dbIndex.Name = $"{db.Name}{beginIndex}";
                    dbIndex.ConnnectString = string.Format(db.ConnnectString, beginIndex);
                    dbIndex.CompilerName = db.CompilerName;
                    dbIndex.Index = beginIndex.ToString();
                    dbIndex.CheckAlivePeriod = db.CheckAlivePeriod;
                    dbIndex.IsDefault = db.IsDefault;
                    dbIndex.IsGetLoadBalance = db.IsGetLoadBalance;
                    dbIndex.IsSetLoadBalance = db.IsSetLoadBalance;
                    dataBases.Add(dbIndex.Name, dbIndex);
                    beginIndex++;
                }
            }

            if (node.Attributes["BeginIndex"] == null && node.Attributes["EndIndex"] == null ||
                node.Attributes["IsKeepMain"] != null && bool.Parse(node.Attributes["IsKeepMain"].Value))
            {
                db.ConnnectString = string.Format(db.ConnnectString, "");
                dataBases.Add(db.Name, db);
            }
        }

        /// <summary>
        /// 设置读写故障转移
        /// </summary>
        /// <param name="ormDataBases"></param>
        /// <param name="nodes"></param>
        protected virtual void SetOrmDataBaseFailoverByXmlNodes(IDictionary<string, OrmDataBaseInfo> ormDataBases, XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes == null) continue;
                var ormDataBase = ormDataBases.Values.FirstOrDefault(it => it.Name.Equals(node.Attributes["Name"].Value));
                if (ormDataBase == null) continue;
                SetOrmDataBaseFailoverByXmlNode(ormDataBase, ormDataBases, node);
            }
      
        }

        /// <summary>
        /// 设置读写故障转移
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <param name="ormDataBases"></param>
        /// <param name="node"></param>
        protected virtual void SetOrmDataBaseFailoverByXmlNode(OrmDataBaseInfo ormDataBase, IDictionary<string, OrmDataBaseInfo> ormDataBases, XmlNode node)
        {
            if (node.Attributes == null) return;
            if (node.Attributes["GetFailoverName"] != null)
            {
                var getFailoverName = node.Attributes["GetFailoverName"].Value.Split(',');
                ormDataBase.GetFailovers = ormDataBases.Values.Where(it => getFailoverName.Contains($"{it.Name}{it.Index}") && !ormDataBase.ConnnectString.Equals(it.ConnnectString)).ToList();
            }
            if (node.Attributes["SetFailoverName"] != null)
            {
                var setFailoverName = node.Attributes["SetFailoverName"].Value.Split(',');
                ormDataBase.SetFailovers = ormDataBases.Values.Where(it => setFailoverName.Contains($"{it.Name}{it.Index}") && !ormDataBase.ConnnectString.Equals(it.ConnnectString)).ToList();
            }
        }
        #endregion

       

    }
}
