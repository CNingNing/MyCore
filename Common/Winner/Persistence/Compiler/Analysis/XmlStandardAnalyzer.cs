using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

namespace Winner.Persistence.Compiler.Analysis
{
    public class XmlStandardAnalyzer : StandardAnalyzer
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
        public XmlStandardAnalyzer()
        { 
        }
        public XmlStandardAnalyzer(string configFile)
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
            if(doc==null)
                return;
            LoadDictionariesByXml(doc);
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
        /// 得到词库信息
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected virtual void LoadDictionariesByXml(XmlDocument doc)
        {
            var files = new List<string>();
            XmlNodeList nodes = doc.SelectNodes("/configuration/Search/XmlStandardAnalyzer/File");
            var mainDictionaries = new List<string>();
            var stopDictionaries = new List<string>();
            var transformDictionaries = new List<KeyValuePair<string, string>>();
            var splitDictionaries = new List<string>();
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, node.Attributes["Path"].Value);
                    if (!File.Exists(fileName))
                        continue;
                    switch (node.Attributes["Name"].Value)
                    {
                        case "Main":
                            mainDictionaries.AddRange(GetDictionariesByXmlNode(fileName));  
                            break;
                        case "Stop":
                            stopDictionaries.AddRange(GetDictionariesByXmlNode(fileName));
                            break;
                        case "Split":
                            splitDictionaries.AddRange(GetDictionariesByXmlNode(fileName));
                            break;
                        case "Transform":
                            var values = GetDictionariesByXmlNode(fileName);
                            if (values != null)
                            {
                                foreach (var value in values)
                                {
                                    var vs = value.Split(' ');
                                    if(vs.Length!=2)
                                        continue;
                                    transformDictionaries.Add(new KeyValuePair<string, string>(vs[0].Trim(),vs[1].Trim()) );
                                }
                            }
                           
                            break;
                    }
                    files.Add(fileName);
                }
            }
            MainDictionaries = mainDictionaries.OrderBy(it => it).Distinct().Select(it=>new WordInfo{Name=it}).ToArray();
            StopDictionaries = stopDictionaries.OrderBy(it => it).Distinct().Select(it => new WordInfo { Name = it }).ToArray();
            TransformDictionaries = transformDictionaries.OrderBy(it => it.Key).Distinct().ToArray();
            SplitDictionaries = splitDictionaries.OrderBy(it => it).Distinct().ToArray();
        
        }

        /// <summary>
        /// 得到字典
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected virtual IList<string> GetDictionariesByXmlNode(string fileName)
        {
            var text = File.ReadAllText(fileName);
            return text.Split(new[] {"\r\n"}, StringSplitOptions.None);
        }
   
 

        #endregion
    }
}
