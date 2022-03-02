using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Component.Extension;

namespace Configuration
{
    static public class ConfigurationManager
    {
        #region 初始化配置文件
        public static object Locker=new object();
        public static string JsonRootPath { get; set; }


        public static void Initialize()
        {
            lock (Locker)
            {
                SetJsonRootPath();
                LoadSettings(JsonRootPath, @"redis.json");
                LoadSettings(JsonRootPath, @"thirdparty.json");
                LoadSettings(JsonRootPath, @"url.json");
                LoadSettings(JsonRootPath, @"database.json");

            }
        }

        public static void SetJsonRootPath()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while(true)
            {
                var direct =new DirectoryInfo(Path.Combine(dir.FullName, "ConfigurationFile\\Configuration\\json"));
                if (direct.Exists)
                {
                    JsonRootPath = $"{dir.FullName}\\ConfigurationFile\\Configuration\\json";
                    break;
                }
                dir = dir.Parent;
                if (dir == null)
                    break;
            }
        }

       

        /// <summary>
        /// 得到配置信息
        /// </summary>
        /// <returns></returns>
        public static void ReverseUrlKeySettings()
        {
            var temps=new Dictionary<string,string>();
            foreach (var setting in Settings)
            {
                if (setting.Key == "Domain" || !setting.Key.EndsWith("Url") || string.IsNullOrWhiteSpace(setting.Value))
                    continue;
                var key = setting.Value.ToLower().Replace("http://","").Replace("https://","");
                if (temps.ContainsKey(key))
                    temps.Remove(key);
                temps.Add(key, setting.Key);
            }

            foreach (var temp in temps)
            {
                if (Settings.ContainsKey(temp.Key))
                    Settings.Remove(temp.Key);
                Settings.Add(temp.Key, temp.Value);
            }
        }
    

        /// <summary>
        /// 得到配置文档
        /// </summary>
        /// <returns></returns>
        private static XmlDocument GetXmlDocument(string fileName)
        {
            fileName = string.IsNullOrEmpty(fileName) ? @"Config/Config.config" : fileName;
            var doc = new XmlDocument();
            fileName = Path.Combine(JsonRootPath, fileName);
            doc.Load(fileName);
            return doc;
        }

     
        /// <summary>
        /// 替换节点
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="appNode"></param>
        private static void ReplaceNodes(XmlNodeList nodes, XmlNode appNode)
        {
            if (appNode.ChildNodes.Count == 0) return;
            foreach (XmlNode node in nodes)
            {
                ReplaceNode(node, appNode);
                if (node.ChildNodes.Count > 0)
                    ReplaceNodes(node.ChildNodes, appNode);
            }
        }

        /// <summary>
        /// 替换节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="appNode"></param>
        private static void ReplaceNode(XmlNode node, XmlNode appNode)
        {
            if (node.Attributes == null || node.Attributes["Merged"] == null)
                return ;
            for (int i = 0; i < appNode.ChildNodes.Count;)
            {
                var childNode = appNode.ChildNodes[i];
                if (childNode.Attributes == null || childNode.Attributes["MergedValue"] == null
                    || childNode.Attributes["MergedValue"].Value != node.Attributes["Merged"].Value)
                {
                    i++;
                    continue;
                }
                var type = childNode.Attributes["MergedType"].Value;
                switch (type)
                {
                    case "Replace":
                        node.ParentNode.ReplaceChild(childNode, node);
                        break;
                    case "Append":
                        node.AppendChild(childNode);
                        break;
                    case "InsertBefore":
                        node.ParentNode.InsertBefore(childNode, node);
                        break;
                    case "InsertAfter":
                        node.ParentNode.InsertAfter(childNode, node);
                        break;
                    default:
                        i++;
                        break;
                }
                childNode.Attributes.Remove(childNode.Attributes["MergedType"]);
                childNode.Attributes.Remove(childNode.Attributes["MergedValue"]);
            }
        }


        #endregion

        #region 得到配置信息
        public static readonly IDictionary<string,string> Settings=new Dictionary<string, string>();

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="fileName"></param>
        private static void LoadSettings(string appName,string fileName)
        {
            if (string.IsNullOrEmpty(appName)) return;
            var file=new FileInfo(Path.Combine(appName, fileName));
            if (!file.Exists) return;
            var filecontent = File.ReadAllText(Path.Combine(appName, fileName));
            var dic = filecontent.DeserializeJson<IDictionary<string, object>>();
            foreach(var key in dic.Keys)
            {
                if (Settings.ContainsKey(key)) continue;
                Settings.Add(key, dic.Get(key).ToString());
            }
        }

        /// <summary>
        /// 加载事件
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="fileName"></param>
        private static void LoadEvents(string appName,string fileName)
        {
            var doc = GetXmlDocument(fileName);
            XmlNodeList xmlNodes = doc.SelectNodes("configuration/Settings/Event");
            if (xmlNodes == null || xmlNodes.Count == 0)
                return;
            foreach (XmlNode xmlNode in xmlNodes)
            {
                if (xmlNode.Attributes == null ||
                    xmlNode.Attributes["Name"] == null || string.IsNullOrEmpty(xmlNode.Attributes["Name"].Value)
                    || xmlNode.Attributes["Name"].Value.Split(',').Contains(appName))
                {
                    foreach (XmlNode node in xmlNode.ChildNodes)
                    {
                        if (node.Attributes == null || node.Attributes["Key"] == null
                            || node.Attributes["ClassName"] == null || node.Attributes["Method"] == null)
                            continue;
                        var isAsync = node.Attributes["IsAsync"] != null && node.Attributes["IsAsync"].Value == "true";
                        EventManager.Register(node.Attributes["Key"].Value, node.Attributes["ClassName"].Value, node.Attributes["Method"].Value, isAsync);
                    }
                }
              
            }
        }

        /// <summary>
        /// 得到配置信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetSetting<T>(string name)
        {
            if (Settings.ContainsKey(name))
            {
                var value= Settings[name];
                return ReplaceUrlSettings(name,value).Convert<T>();
            }
            return default(T);
        }

        /// <summary>
        /// 得到配置信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string ReplaceUrlSettings(string name,string value)
        {
            if (string.IsNullOrWhiteSpace(value) || name != "Domain" && !name.EndsWith("Url"))
                return value;
            var domain = Settings.ContainsKey("Domain") ? Settings["Domain"].ToLower() : "";
            var requestDomain = RequestHelper.GetRequestDomain()?.ToLower();
            if (string.IsNullOrWhiteSpace(requestDomain) || domain == requestDomain)
                return value;
            return value.ToLower().Replace(domain.ToLower(), requestDomain);
        }

        #endregion




    }
}
