using System.Collections.Generic;
using System.Data;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Translation
{
    public interface IExecutor
    {
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="isLazyLoadExecute"></param>
        /// <returns></returns>
        T GetInfos<T>(OrmObjectInfo obj, QueryInfo query, bool isLazyLoadExecute);

        /// <summary>
        /// 得到事务对象
        /// </summary>
        /// <param name="info"></param>
        /// <param name="unitOfWorks"></param>
        /// <returns></returns>
        IList<IUnitofwork> Save(SaveInfo info, IList<IUnitofwork> unitOfWorks = null);


        /// <summary>
        /// 执行查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        T ExecuteQuery<T>(string name, string commandText, CommandType commandType, params object[] parameters);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="name"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteCommand(string name, string commandText, CommandType commandType, params object[] parameters);
      
    }
}
