using System;
using System.Collections.Generic;
using System.Xml;

namespace Winner.Storage.Image
{
    public class XmlImageStore : ImageStore
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
        public XmlImageStore()
        { 
            ImageThumbnails=new Dictionary<string, IList<ImageThumbnailInfo>>();
        }

        /// <summary>
        /// 存储路径，缩略图信息
        /// </summary>
        /// <param name="imageThumbnails"></param>
        ///  <param name="configFile"></param>
        public XmlImageStore(IDictionary<string, IList<ImageThumbnailInfo>> imageThumbnails, string configFile)
            : base(imageThumbnails)
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
            LoadImageThumbnailByXml(doc);
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
        protected virtual void LoadImageThumbnailByXml(XmlDocument doc)
        {
            XmlNodeList nodes = doc.SelectNodes("/configuration/Storage/XmlImageStore/Path");
            if (nodes == null || nodes.Count == 0)
                return;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                    AddImageThumbnail(node.Attributes["Path"].Value.ToLower(), GetImageThumbnailByXmlNode(node));
            }
        }
        /// <summary>
        /// 根据节点得到缩略图
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual IList<ImageThumbnailInfo> GetImageThumbnailByXmlNode(XmlNode node)
        {
            IList<ImageThumbnailInfo> images = new List<ImageThumbnailInfo>();
            XmlNodeList nodes = node.SelectNodes("Info");
            if (nodes == null || nodes.Count == 0)
                return images;
            foreach (XmlNode nd in nodes)
            {
                AddImageThumbnailByXmlNode(images, nd);
            }
            return images;
        }
        /// <summary>
        ///  根据节点添加缩略图
        /// </summary>
        /// <param name="images"></param>
        /// <param name="node"></param>
        protected virtual void AddImageThumbnailByXmlNode(IList<ImageThumbnailInfo> images, XmlNode node)
        {
            var iti = new ImageThumbnailInfo();
            if (node != null && node.Attributes != null)
            {
                iti.Flag = node.Attributes["Flag"].Value;
                iti.Width = Convert.ToInt32(node.Attributes["Width"].Value);
                iti.Height = Convert.ToInt32(node.Attributes["Height"].Value);
                iti.IsUsed =node.Attributes["IsUsed"]==null || Convert.ToBoolean(node.Attributes["Height"].Value);
            }
            images.Add(iti);
        }
        #endregion
    }
}
