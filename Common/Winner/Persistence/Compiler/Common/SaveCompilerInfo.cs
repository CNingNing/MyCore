using System.Collections.Generic;
using System.Data.Common;
using Winner.Persistence.Data;
using Winner.Persistence.Relation;
using Winner.Persistence.Translation;
using System.Linq;
namespace Winner.Persistence.Compiler.Common
{
    public class SaveCompilerInfo
    {
        public DbCommand Command { get; set; }
        /// <summary>
        /// 存储信息
        /// </summary>
        public SaveInfo SaveInfo { get; set; }

        public IList<EntityInfo> ContentEntities { get; set; } 
        /// <summary>
        /// 数据库存储信息
        /// </summary>
        public OrmDataBaseInfo OrmDataBase { get; set; }
        /// <summary>
        /// 是否存储过参数
        /// </summary>
        public bool IsSaveParameters { get; set; }

        private IList<OrmPropertyInfo> _mapProperties=new List<OrmPropertyInfo>();
        /// <summary>
        /// 已经存储的对象
        /// </summary>
        public IList<OrmPropertyInfo> MapProperties
        {
            get { return _mapProperties; }
            set { _mapProperties = value; }
        }
        /// <summary>
        /// 是否在map中
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual bool IsInMap(OrmPropertyInfo property)
        {
            return
                MapProperties.Count(
                    it => it.PropertyName == property.PropertyName && it.ObjectName == property.ObjectName)>0;
        }
    }
}
