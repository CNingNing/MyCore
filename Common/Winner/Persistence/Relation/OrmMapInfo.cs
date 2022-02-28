using System;
using System.Collections.Generic;

namespace Winner.Persistence.Relation
{

    [Serializable]
    public class OrmMapInfo
    {
        /// <summary>
        /// 映射的对象
        /// </summary>
        public string MapObjectName { get; set; }
        /// <summary>
        /// 主键属性
        /// </summary>
        public OrmPropertyInfo ObjectProperty { get; set; }
        /// <summary>
        /// 外键属性
        /// </summary>
        public OrmPropertyInfo MapObjectProperty { get; set; }
        /// <summary>
        /// 关系
        /// </summary>
        public OrmMapType MapType { get; set; }
        /// <summary>
        /// 添加时候的操作
        /// </summary>
        public bool IsAdd { get; set; }
        /// <summary>
        /// 更新时候的操作
        /// </summary>
        public bool IsModify { get; set; }
        /// <summary>
        /// 删除时候的操作
        /// </summary>
        public bool IsRemove { get; set; }
        /// <summary>
        /// 还原时候的操作
        /// </summary>
        public bool IsRestore { get; set; }
        /// <summary>
        /// 是否加载贪婪加载
        /// </summary>
        public bool IsGreedyLoad { get; set; }
        /// <summary>
        /// 是否延迟加载
        /// </summary>
        public bool IsLazyLoad { get; set; }
        /// <summary>
        /// 是否远程
        /// </summary>
        public bool IsRemote { get; set; }
        /// <summary>
        /// 是否移除缓存
        /// </summary>
        public bool IsRemoveCache { get; set; }
        /// <summary>
        /// 远程名称
        /// </summary>
        public string RemoteName { get; set; }
        /// <summary>
        /// 是否关联对象一起切表
        /// </summary>
        public bool IsMapTableAutoSharding { get; set; } = true;
        public IDictionary<string, OrmObjectInfo> Orms { get; set; }
        /// <summary>
        /// 得到ORM
        /// </summary>
        /// <returns></returns>
        public virtual OrmObjectInfo GetMapObject()
        {
            return Orms[MapObjectName];
        }
        /// <summary>
        /// 检查是远程
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckRemote()
        {
            return IsRemote && string.IsNullOrEmpty(RemoteName);
        }
    }
}
