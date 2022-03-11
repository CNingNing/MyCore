using System.Collections.Generic;
using System.Data;
using Winner.Persistence.Data;
using Winner.Persistence.Relation;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Compiler.Common
{
   

    public interface ICompiler
    {
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        T GetInfos<T>(OrmDataBaseInfo ormDataBase, OrmObjectInfo obj, QueryInfo query);

        /// <summary>
        /// 得到事务
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <param name="info"></param>
        /// <param name="unitOfWorks"></param>
        void AddUnitofwork(OrmDataBaseInfo ormDataBase, SaveInfo info, IList<IUnitofwork> unitOfWorks);

        /// <summary>
        /// 执行查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        T ExecuteQuery<T>(OrmDataBaseInfo ormDataBase, string commandText, CommandType commandType, params object[] parameters);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteCommand(OrmDataBaseInfo ormDataBase, string commandText, CommandType commandType, params object[] parameters);

    }
}
