using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Winner.Base;
using Winner.Cache;
using Winner.Channel;
using Winner.Cluster;
using Winner.Creation;
using Winner.Dislan;
using Winner.Filter;
using Winner.Lock;
using Winner.Log;
using Winner.Mail;
using Winner.Message;
using Winner.Persistence;
using Winner.Persistence.Compiler.Analysis;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Compiler.MySql;
using Winner.Persistence.ContextStorage;
using Winner.Persistence.Data;
using Winner.Persistence.Relation;
using Winner.Persistence.Route;
using Winner.Persistence.Translation;
using Winner.Persistence.Works;
using Winner.Queue;
using Winner.Reverse;
using Winner.Storage;
using Winner.Storage.Address;
using Winner.Storage.Distributed;
using Winner.Storage.Document;
using Winner.Storage.Image;
using Winner.Storage.Route;
using IProperty = Winner.Base.IProperty;
using Property = Winner.Base.Property;

namespace Winner
{
    public sealed class Creator
    {
        #region 声明


        #endregion

        #region 静态变量
        public static  string AppName { get; set; }
        public static  string RootPath { get; set; }
        static private string _configFile;
        static public void SetConfigFile(string config,string appName)
        {
            _configFile = config;
            AppName = appName;
            RootPath = (new FileInfo(_configFile)).Directory.Parent.FullName;
            LoadInstance();
        }

        /// <summary>
        /// 替换
        /// </summary>
        /// <param name="appName"></param>
        public static XmlDocument ReplaceConfigFile(XmlDocument doc)
        {
            if (string.IsNullOrEmpty(AppName)) 
                return doc;
            XmlNodeList xmlNodes = doc.SelectNodes("configuration/Merged/App");
            foreach (XmlNode xmlNode in xmlNodes)
            {
                if (xmlNode.Attributes == null || xmlNode.Attributes["Name"] == null)
                    continue;
                var names = xmlNode.Attributes["Name"].Value.Split(',');
                if (names.Count(it => AppName.StartsWith(it)) == 0)
                    continue;
                ReplaceNodes(doc.ChildNodes, xmlNode);
            }
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
                return;
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
        /// <summary>
        /// 得到配置文档
        /// </summary>
        /// <returns></returns>
        public static XmlDocument GetXmlDocument(string fileName)
        {
            var doc = new XmlDocument();
            fileName = Path.Combine(RootPath, fileName);
            doc.Load(fileName);
            return ReplaceConfigFile(doc);
        }

        static private  IDictionary<string, object> _instances = new Dictionary<string, object>();

        #endregion

        #region 得到和创建实例
        /// <summary>
        /// 得到实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        static public T Get<T>(string name=null)
        {
            if (name == null)
            {
                name = typeof(T).ToString();
            }
            if (!_instances.ContainsKey(name))
               return  default(T);
            return (T)_instances[name];
        }
        /// <summary>
        /// 添加实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        static public bool Set(string name, object obj)
        {
            if (_instances.ContainsKey(name))
            {
                return false;
            }
            _instances.Add(name, obj);
            return true;
        }
    

        #endregion

        #region 默认配置
        /// <summary>
        /// 加载实例
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        private static void LoadInstatnce(IDictionary<string, object> instances, string name, object obj)
        {
            if (!instances.ContainsKey(name))
            {
                instances.Add(name, obj);
            }
          
        }
        /// <summary>
        /// 加载Base模块
        /// </summary>
        private static void LoadBaseInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances,typeof(IComponent).FullName, new Component());
            LoadInstatnce(instances, typeof(ISecurity).FullName, new Security());
            LoadInstatnce(instances, typeof(IProperty).FullName, new Property());
        }
        /// <summary>
        /// 加载Queue模块
        /// </summary>
        private static void LoadQueueInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(IQueue).FullName, new DistributedQueue());
           
        }
        /// <summary>
        /// 加载Mail模块
        /// </summary>
        private static void LoadMailInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(IMail).FullName, new Mail.Mail());
        }
        /// <summary>
        /// 加载Cache模块
        /// </summary>
        private static void LoadCacheInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(ICache).FullName, new DistributedCache());
        }
        /// <summary>
        /// 加载Base模块
        /// </summary>
        private static void LoadClusterInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(ICluster).FullName, new Cluster.Cluster());
        }
        /// <summary>
        /// 加载Creation模块
        /// </summary>
        private static void LoadCreationInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(IFactory).FullName, new XmlFactory());
        }
        /// <summary>
        /// 加载Dislan模块
        /// </summary>
        private static void LoadDislanInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(ILanguage).FullName, new XmlLanguage());
        }
        /// <summary>
        /// 加载Dislan模块
        /// </summary>
        private static void LoadFilterInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(IValidation).FullName, new XmlValidation());
        }
        /// <summary>
        /// 加载Log模块
        /// </summary>
        private static void LoadLogInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(ILog).FullName, new FileLog());
        }

        /// <summary>
        /// 加载Log模块
        /// </summary>
        private static void LoadMessageInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(IMessage).FullName, new DistributedMessage());
        }

        /// <summary>
        /// 加载Log模块
        /// </summary>
        private static void LoadLockInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(ILocker).FullName, new DistributedLocker());
        }
        /// <summary>
        /// 加载Persistence模块
        /// </summary>
        private static void LoadPersistenceInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(IContext).FullName, new Context());
            LoadInstatnce(instances, typeof(ITransaction).FullName, new Transaction());
            LoadInstatnce(instances, typeof(IExecutor).FullName, new Executor());
            LoadInstatnce(instances, typeof(IOrm).FullName, new XmlOrm());
            LoadInstatnce(instances, typeof(IDataBase).FullName, new XmlDataBase());
            LoadInstatnce(instances, typeof(IDbRoute).FullName, new XmlDbRoute());
            LoadInstatnce(instances, typeof(IContextStorage).FullName, new ContextStorage());
            LoadInstatnce(instances, typeof(ICompiler).FullName, new MySqlCompiler());
            LoadInstatnce(instances, typeof(IAnalyzer).FullName, new XmlBinaryAnalyzer());
        }
   
        /// <summary>
        /// 加载Reverse模块
        /// </summary>
        private static void LoadReverseInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(IMapper).FullName, new Mapper());
        }
        /// <summary>
        /// 加载Search模块
        /// </summary>
        //private static void LoadSearchInstance(IDictionary<string, object> instances)
        //{
        //    LoadInstatnce(instances, typeof(IAnalyzer).FullName, new StandardAnalyzer());
        //    LoadInstatnce(instances, typeof(IDocumentor).FullName, new Documentor());
        //    LoadInstatnce(instances, typeof(IWorder).FullName, new Worder());
        //    LoadInstatnce(instances, typeof(IStorer).FullName, new FileStorer());
        //    LoadInstatnce(instances, typeof(IIndexer).FullName, new Indexer());
        //}
        /// <summary>
        /// 加载Storage模块
        /// </summary>
        private static void LoadStorageInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(Storage.Cache.ICache).FullName, new Storage.Cache.LocalCache());
            LoadInstatnce(instances, typeof(IFile).FullName, new FileStore());
            LoadInstatnce(instances, typeof(IThumbnail).FullName, new Thumbnail());
            LoadInstatnce(instances, typeof(IDocument).FullName, new Document());
            LoadInstatnce(instances, typeof(IMaster).FullName, new Master());
            LoadInstatnce(instances, typeof(IAddress).FullName, new Address());
            LoadInstatnce(instances, typeof(IFileRoute).FullName, new FileRoute());
        }
        /// <summary>
        /// 加载Wcf模块
        /// </summary>
        private static void LoadChannelInstance(IDictionary<string, object> instances)
        {
            LoadInstatnce(instances, typeof(IChannelService).FullName, new ChannelService());
            LoadInstatnce(instances, typeof(IChannelClient).FullName, new ChannelClient());
        }

        /// <summary>
        /// 加载默认实例
        /// </summary>
        public static void LoadDefaultInstance(IDictionary<string, object> instances)
        {
            LoadBaseInstance(instances);
            LoadQueueInstance(instances);
            LoadMailInstance(instances);
            LoadCacheInstance(instances);
            LoadClusterInstance(instances);
            LoadCreationInstance(instances);
            LoadDislanInstance(instances);
            LoadFilterInstance(instances);
            LoadLogInstance(instances);
            LoadPersistenceInstance(instances);
            LoadReverseInstance(instances);
            LoadMessageInstance(instances);
            LoadLockInstance(instances);
            LoadStorageInstance(instances);
            LoadChannelInstance(instances);
            SetDefaultInstanceProperty(instances);
        }

        /// <summary>
        /// 设置默认实例
        /// </summary>
        public static void SetDefaultInstanceProperty(IDictionary<string, object> instances)
        {
            foreach (var instance in instances)
            {
                var properties = instance.Value.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (instances.ContainsKey(property.PropertyType.FullName))
                    {
                        property.SetValue(instance.Value, instances[property.PropertyType.FullName], null);
                    }

                }
            }
          
        }

        #endregion

        #region 加载实例
        static private object _locker=new object();
        /// <summary>
        /// 加载实例
        /// </summary>
        static private void LoadInstance()
        {
            lock (_locker)
            {
                var directs = new List<string>();
                LoadDirectory(directs, Path.GetDirectoryName(_configFile));
                var instances = new Dictionary<string, object>();
                LoadDefaultInstance(instances);

                var doc = new XmlDocument();
                doc.Load(_configFile);
                var nodes = doc.SelectNodes("/configuration/Winner/Instance");
                FillInstance(instances, nodes);
                _instances = instances;

            }
        }
        /// <summary>
        /// 加载目录
        /// </summary>
        /// <param name="directs"></param>
        /// <param name="path"></param>
        private static void LoadDirectory(IList<string> directs, string path)
        {
            directs.Add(path);
            var directory=new DirectoryInfo(path);
            foreach (var dir in directory.GetDirectories())
            {
                LoadDirectory(directs, dir.FullName);
            }
            
        }
 
        /// <summary>
        /// 填充实例
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="nodes"></param>
        private static void FillInstance(IDictionary<string, object> instances, XmlNodeList nodes)
        {
            if (nodes == null || nodes.Count <= 0) return;
            foreach (XmlNode node in nodes)
                TryCreateInstance(instances,node);
            SetDefaultInstanceProperty(instances);
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes == null || !instances.ContainsKey(node.Attributes["Name"].Value)) continue;
                var target = instances[node.Attributes["Name"].Value];
                TrySetProperty(instances,target, node);
            }
                
           
        }

        ///  <summary>
        /// 尝试创建实例
        ///  </summary>
        /// <param name="instances"></param>
        /// <param name="node"></param>
        private static void TryCreateInstance(IDictionary<string, object> instances, XmlNode node)
        {
            try
            {
                CreateInstance(instances,node);
            }
            catch (Exception ex)
            {
                throw new Exception(node.OuterXml, ex);
            }
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static void CreateInstance(IDictionary<string, object> instances, XmlNode node)
        {
            if (node.Attributes == null || node.Attributes["ClassName"] == null || string.IsNullOrEmpty(node.Attributes["ClassName"].Value)) return;
            var type = Type.GetType(node.Attributes["ClassName"].Value);
            if (type == null) return;
            var obj = Activator.CreateInstance(type);
            if (instances.ContainsKey(node.Attributes["Name"].Value))
                instances.Remove(node.Attributes["Name"].Value);
            instances.Add(node.Attributes["Name"].Value, obj);
        }

        ///  <summary>
        /// 尝试创建实例
        ///  </summary>
        /// <param name="instances"></param>
        /// <param name="target"></param>
        /// <param name="node"></param>
        private static void TrySetProperty(IDictionary<string, object> instances, object target, XmlNode node)
        {
            try
            {
                SetProperty(instances,target, node);
            }
            catch (Exception ex)
            {
                throw new Exception(node.OuterXml, ex);
            }
        }


        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="target"></param>
        /// <param name="node"></param>
        static private void SetProperty(IDictionary<string, object> instances, object target, XmlNode node)
        {
            var nodes = node.SelectNodes("Property");
            if (nodes == null) return;
            foreach (XmlNode nd in nodes)
            {
                if (nd.Attributes == null) continue;
                var property =
                    target.GetType().GetProperties().FirstOrDefault(it => it.Name.Equals(nd.Attributes["Name"].Value));
                if (property == null) continue;
                var value = GetPropertyValue(instances,nd.Attributes["Value"].Value, property);
                if (value == null) continue;
                property.SetValue(target, value, null);
                TrySetProperty(instances,value, nd);
            }
        }

        /// <summary>
        /// 得到属性值
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="propertyValue"></param>
        /// <param name="property"></param>
        static private object GetPropertyValue(IDictionary<string, object> instances, string propertyValue, PropertyInfo property)
        {
            object value;
            if (property.PropertyType.IsInterface)
            {
                value = !instances.ContainsKey(propertyValue) ? CreateClass(propertyValue) : instances[propertyValue];
            }
            else
                value = TryConvertValue(propertyValue, property.PropertyType);
            return value;
        }
         /// <summary>
        /// 转换值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static private object TryConvertValue(object value, Type type)
        {
            if (value == null) return null;
            if (type == typeof(object)) return value;
            try
            {
                if (type.IsEnum)
                {
                    var charValue = Convert.ChangeType(value, typeof(char));
                    if (charValue == null) return Enum.Parse(type, value.ToString());
                    var intValue = Convert.ChangeType(charValue, typeof(int));
                    if (intValue == null) return null;
                    return Enum.Parse(type, intValue.ToString());
                }
                return Convert.ChangeType(value, type);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 创建类
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        static private object CreateClass(string className)
        {
            var t = Type.GetType(className);
            if (t == null) return null;
            return Activator.CreateInstance(t);
        }
        #endregion

    }
}
