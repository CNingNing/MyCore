using System.Collections.Generic;

namespace Winner.Persistence.Relation
{
    public interface IOrm 
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="info"></param>
        void AddOrm(OrmObjectInfo info);
        /// <summary>
        /// 得到对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        OrmObjectInfo GetOrm(string name);
        /// <summary>
        /// 得到所有对象
        /// </summary>
        /// <returns></returns>
        IDictionary<string, OrmObjectInfo> GetOrms();
     
    }
}
