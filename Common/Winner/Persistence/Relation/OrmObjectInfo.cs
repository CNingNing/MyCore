using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Winner.Persistence.Relation
{


    [Serializable]
    public class OrmObjectInfo
    {
        /// <summary>
        /// 对象名称
        /// </summary>
        public string ObjectName { get; set; }
        

        /// <summary>
        /// 操作表名称
        /// </summary>
        public string SetTableName { get; set; }
        /// <summary>
        /// 查询表名称
        /// </summary>
        public string GetTableName { get; set; }

        private IDictionary<string,OrmPropertyInfo> _properties = new Dictionary<string, OrmPropertyInfo>();
        /// <summary>
        /// 属性
        /// </summary>
        public ICollection<OrmPropertyInfo> Properties
        {
            get { return _properties.Values; }
        }
        /// <summary>
        /// 主键
        /// </summary>
        public OrmPropertyInfo PrimaryProperty { get; set; }
        /// <summary>
        /// 主键
        /// </summary>
        public OrmPropertyInfo VersionProperty { get; set; }
        /// <summary>
        /// 数据库写信息
        /// </summary>
        public string SetDataBase { get; set; }
        /// <summary>
        /// 数据库读信息
        /// </summary>
        public string GetDataBase { get; set; }

        /// <summary>
        /// 标记删除
        /// </summary>
        public string Mark { get; set; }
        /// <summary>
        /// 标记删除
        /// </summary>
        public string MarkAddTime { get; set; }
        /// <summary>
        /// 标记删除
        /// </summary>
        public string MarkModifyTime { get; set; }
        /// <summary>
        /// 标记删除
        /// </summary>
        public string MarkRemoveTime { get; set; }
        /// <summary>
        /// 标记删除
        /// </summary>
        public string MarkRestoreTime { get; set; }
        /// <summary>
        /// 是否实体缓存
        /// </summary>
        public CacheType CacheType { get; set; } = CacheType.None;

        /// <summary>
        /// 缓存时间
        /// </summary>
        public long CacheTime { get; set; } = 1800;
        /// <summary>
        /// 缓存时间
        /// </summary>
        public int CheckCacheTimeSpan { get; set; } = 600;
        /// <summary>
        /// 缓存版本控制名
        /// </summary>
        public bool IsCacheDependency{ get; set; }
        /// <summary>
        /// 路由名称
        /// </summary>
        public string RouteName { get; set; }
        /// <summary>
        /// 分词器
        /// </summary>
        public string AnalyzerName { get; set; }
        /// <summary>
        /// 得到属性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual OrmPropertyInfo GetPropertyInfo(string name)
        {
            if (_properties.ContainsKey(name))
                return _properties[name];
            return null;
        }
        protected bool HasMultiMapProperty { get; set; }
        /// <summary>
        /// 得到属性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual void AddProperty(OrmPropertyInfo property)
        {
            _properties = _properties ?? new Dictionary<string, OrmPropertyInfo>();
            if (!_properties.ContainsKey(property.PropertyName))
                _properties.Add(property.PropertyName, property);
            if (property.PropertyName.Count(s=>s=='.') > 1)
                HasMultiMapProperty = true;
        }
        #region 得到属性连

        /// <summary>
        /// 得到属性连
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="isContainRemote"></param>
        /// <returns></returns>
        public virtual IList<OrmPropertyInfo> GetChainProperties( string propertyName,bool isContainRemote=false)
        {
            var orm = this;
            var name = propertyName;
            var chainProperties = new List<OrmPropertyInfo>();
            while (orm!=null)
            {
                name = AddNearProperty(chainProperties, orm, name);
                if(string.IsNullOrEmpty(name))break;
                var p = chainProperties.LastOrDefault();
                if (p == null || p.Map == null) break;
                if (!isContainRemote && p.Map.CheckRemote())
                    break;
                orm = p.Map.GetMapObject();
            }
            return chainProperties;
        }

        /// <summary>
        /// 得到最近的一个属性
        /// </summary>
        /// <param name="chainProperties"></param>
        /// <param name="orm"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected virtual string AddNearProperty(IList<OrmPropertyInfo> chainProperties, OrmObjectInfo orm, string propertyName)
        {
            var p = orm.GetPropertyInfo(propertyName);
            if (p != null)
            {
                chainProperties.Add(p);
                return null;
            }
            if(HasMultiMapProperty && propertyName.Count(s=>s=='.')>1)
            {
                var key = _properties.Keys.LastOrDefault(it => propertyName.StartsWith(it));
                if(key!=null)
                {
                    p = GetPropertyInfo(key);
                    if (p != null) chainProperties.Add(p);
                    return propertyName.Substring(key.Length + 1, propertyName.Length - 1 - key.Length);
                }
            }
            var index = propertyName.IndexOf('.');
            if (index == -1) return null;
            p = orm.GetPropertyInfo(propertyName.Substring(0, index));
            if (p != null) chainProperties.Add(p);
            return propertyName.Substring(index + 1, propertyName.Length - 1 - index);
        }
        
        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <returns></returns>
        public virtual OrmObjectInfo Clone()
        {
            var formatter = new BinaryFormatter();
            var stream = new System.IO.MemoryStream();
            formatter.Serialize(stream, this);
            stream.Position = 0;
            return formatter.Deserialize(stream) as OrmObjectInfo;
        }
        #endregion
    }
}
