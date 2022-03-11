using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Winner.Channel;

namespace Winner.Storage.Distributed
{
    public class XmlDistributedStore : DistributedStore
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
        public XmlDistributedStore()
        { 
        }

        /// <summary>
        /// WCF客户端配置文件,虚拟节点配置文件路径
        /// </summary>
        /// <param name="configFile"></param>
        public XmlDistributedStore(string configFile)
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

            LoadStoreByXml(doc);
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
        protected virtual void LoadStoreByXml(XmlDocument doc)
        {
            XmlNode node = doc.SelectSingleNode("/configuration/Storage/XmlDistributedStore");
            if (node == null || node.Attributes == null)
                return;
            DataServiceGroups = GetDataServiceGroupsByXmlNode(node);
        }
  

        /// <summary>
        /// 根据节点得到缩略图
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual IList<DataServiceGroupInfo> GetDataServiceGroupsByXmlNode(XmlNode node)
        {
            IList<DataServiceGroupInfo> dataServiceGroups = new List<DataServiceGroupInfo>();
            XmlNodeList nodes = node.SelectNodes("DataServiceGroup");
            if (nodes == null || nodes.Count == 0)
                return dataServiceGroups;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            foreach (XmlNode nd in nodes)
            {
                AddDataServiceByXmlNode(dataServiceGroups, endPoints, nd);
            }
            return dataServiceGroups;
        }

        /// <summary>
        ///  根据节点添加缩略图
        /// </summary>
        /// <param name="dataServiceGroups"></param>
        /// <param name="endPoints"></param>
        /// <param name="node"></param>
        protected virtual void AddDataServiceByXmlNode(IList<DataServiceGroupInfo> dataServiceGroups, IList<EndPointInfo> endPoints, XmlNode node)
        {

            if (node != null && node.Attributes != null)
            {
                if (node.Attributes["Count"] == null)
                    AddSingleDataServiceByXmlNode(dataServiceGroups, endPoints, node);
                else
                    AddBatchDataServiceByXmlNode(dataServiceGroups, endPoints, node);
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="dataServiceGroups"></param>
        /// <param name="endPoints"></param>
        /// <param name="node"></param>
        protected virtual void AddSingleDataServiceByXmlNode(IList<DataServiceGroupInfo> dataServiceGroups, IList<EndPointInfo> endPoints, XmlNode node)
        {
         
            if (node != null && node.Attributes != null)
            {
                var dsp = new DataServiceGroupInfo { DataServices = new List<DataServiceInfo>() };
                dsp.Path = node.Attributes["Path"] == null ? null : node.Attributes["Path"].Value;
                dsp.Name = node.Attributes["Name"] == null ? null : node.Attributes["Name"].Value;
                dsp.IsClose = node.Attributes["IsClose"] != null && Convert.ToBoolean(node.Attributes["IsClose"].Value);
                FillDataServiceGroup(dsp, endPoints, node);
                FillAddresses(dsp, endPoints, node);
                dataServiceGroups.Add(dsp);
            }
           
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="dataServiceGroups"></param>
        /// <param name="endPoints"></param>
        /// <param name="node"></param>
        protected virtual void AddBatchDataServiceByXmlNode(IList<DataServiceGroupInfo> dataServiceGroups, IList<EndPointInfo> endPoints, XmlNode node)
        {
            if (node == null || node.Attributes == null) return;
            var startIndex = node.Attributes["StartIndex"] == null ? 0 : int.Parse(node.Attributes["StartIndex"].Value);
            var count = node.Attributes["Count"] == null ? 1 : int.Parse(node.Attributes["Count"].Value);
            for (int i = 0; i < count; i++)
            {
                var dsp = new DataServiceGroupInfo { DataServices = new List<DataServiceInfo>() };
                dsp.Path = node.Attributes["Path"] == null ? null : node.Attributes["Path"].Value;
                dsp.Name = node.Attributes["Name"] == null ? null :string.Format(node.Attributes["Name"].Value,startIndex+i);
                dsp.IsClose = node.Attributes["IsClose"] != null && Convert.ToBoolean(node.Attributes["IsClose"].Value);
                dataServiceGroups.Add(dsp);
                FillDataServiceGroup(dsp, endPoints, node);
                FillAddresses(dsp, endPoints, node);
            }
          
        }

        protected virtual void FillDataServiceGroup(DataServiceGroupInfo dsp, IList<EndPointInfo> endPoints, XmlNode node)
        {
            XmlNodeList dsNodes = node.SelectNodes("DataService");
            if (dsNodes != null && dsNodes.Count > 0)
            {
                foreach (XmlNode dsNode in dsNodes)
                {
                    if (dsNode.Attributes == null)
                        continue;
                    var ip = dsNode.Attributes["Ip"] == null ? "" : dsNode.Attributes["Ip"].Value;
                    var ports=new List<int>();
                    var indexs = new List<int>();
                    if (dsNode.Attributes["BeginPort"] != null && dsNode.Attributes["EndPort"] != null)
                    {
                        var beginPort = int.Parse(dsNode.Attributes["BeginPort"].Value);
                        var endPort = int.Parse(dsNode.Attributes["EndPort"].Value);
                        for (int i = beginPort; i <= endPort; i++)
                        {
                            ports.Add(i);
                        }
                    }
                    if (dsNode.Attributes["StartIndex"] != null && dsNode.Attributes["Count"] != null)
                    {
                        var startIndex = int.Parse(dsNode.Attributes["StartIndex"].Value);
                        var count = int.Parse(dsNode.Attributes["Count"].Value);
                        for (int i = startIndex; i < count; i++)
                        {
                            indexs.Add(i);
                        }
                    }

                    foreach (var endPoint in endPoints)
                    {
                        if(!string.IsNullOrWhiteSpace(ip) && endPoint.Ip!=ip)
                            continue;
                        if(ports.Count>0 && !ports.Contains(endPoint.Port))
                            continue;
                        if (indexs.Count > 0 && !indexs.Contains(endPoint.Index))
                            continue;
                        var ds = new DataServiceInfo
                        {
                            Type = dsNode.Attributes["Type"] == null
                                ? DataServiceType.Master
                                : (DataServiceType)
                                Enum.Parse(typeof(DataServiceType), dsNode.Attributes["Type"].Value),
                            EndPoint = endPoint
                        };
                        dsp.DataServices.Add(ds);
                    }
                }
            }
        }

        protected virtual void FillAddresses(DataServiceGroupInfo dsp, IList<EndPointInfo> endPoints, XmlNode node)
        {
            var addresses = new List<string>();
            XmlNodeList dsNodes = node.SelectNodes("Address");
            if (dsNodes != null && dsNodes.Count > 0)
            {
                foreach (XmlNode dsNode in dsNodes)
                {
                    if (dsNode.Attributes == null)
                        continue;
                    var dsStartIndex = dsNode.Attributes["StartIndex"] == null ? 0 : int.Parse(dsNode.Attributes["StartIndex"].Value);
                    var dsCount = dsNode.Attributes["Count"] == null ? 1 : int.Parse(dsNode.Attributes["Count"].Value);
                    for (int j = 0; j < dsCount; j++)
                    {
                        var name = dsNode.Attributes["StartIndex"] == null
                            ? dsNode.Attributes["Name"].Value
                            : string.Format(dsNode.Attributes["Name"].Value, j + dsStartIndex);
                        addresses.Add(name);
                    }
                }
            }
            dsp.Addresses = addresses.ToArray();
        }
        #endregion
    }
}
