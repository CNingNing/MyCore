using Winner.Persistence.Relation;
using System.Linq;
namespace Winner.Persistence.Translation
{
    public class SaveInfo
    {
 
        /// <summary>
        /// Orm
        /// </summary>
        public OrmObjectInfo Object { get; set; }
        /// <summary>
        /// 实体
        /// </summary>
        public EntityInfo Entity { get; set; }
        /// <summary>
        /// 是否设置版本
        /// </summary>
        public bool IsSetVersion
        {
            get
            {
                return Entity.SaveType == SaveType.Modify &&
                       string.IsNullOrEmpty(Entity.WhereExp) &&
                       Object.VersionProperty != null &&
                       Object.Properties.Any(
                           it =>it.IsOptimisticLocker &&
                           (Entity.Properties == null 
                           || Entity.Properties.Contains(it.PropertyName)));
            }
        }

        private string _setDataBase;
        /// <summary>
        /// 得到写库
        /// </summary>
        public virtual string SetDataBase
        {
            set { _setDataBase = value; }
            get
            {
                if (string.IsNullOrEmpty(_setDataBase))
                    return Object.SetDataBase;
                return _setDataBase;
            }
        }
        /// <summary>
        /// 表索引
        /// </summary>
        public virtual string TableIndex { get; set; }

        private string _setTableName;
        /// <summary>
        /// 得到写库
        /// </summary>
        public virtual string SetTableName
        {
            get
            {
                if (string.IsNullOrEmpty(_setTableName))
                    _setTableName = GetSetTableName(Object);
                return _setTableName;
            }
        }
        /// <summary>
        /// 得到表名
        /// </summary>
        /// <param name="ormObject"></param>
        /// <returns></returns>
        public virtual string GetSetTableName(OrmObjectInfo ormObject)
        {
            if (!string.IsNullOrEmpty(TableIndex) && !string.IsNullOrEmpty(ormObject.RouteName))
                return string.Format("{0}{1}", ormObject.SetTableName, TableIndex);
            return ormObject.SetTableName;
        }
    }



 
    
}