using System.Collections.Generic;

namespace Winner.Persistence.Relation
{
    /// <summary>
    /// 加载ORM
    /// </summary>
    public class Orm:IOrm
    {
        #region 属性
        public IDictionary<string,OrmObjectInfo> Orms=new Dictionary<string, OrmObjectInfo>();
        #endregion

        #region 接口实现
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="info"></param>
        public virtual void AddOrm(OrmObjectInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.ObjectName))
                return;
            if (!Orms.ContainsKey(info.ObjectName))
                Orms.Add(info.ObjectName, info);
            var shortName = info.ObjectName.Split(',')[0];
            if (!string.IsNullOrWhiteSpace(shortName) && !Orms.ContainsKey(shortName))
                Orms.Add(shortName, info);
        }
        /// <summary>
        /// 得到对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual OrmObjectInfo GetOrm(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !Orms.ContainsKey(name))
                return null;
            return Orms[name];
        }
        /// <summary>
        /// 得到所有对象
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, OrmObjectInfo> GetOrms()
        {
            return Orms;
        }


        #endregion

       
    }
}
