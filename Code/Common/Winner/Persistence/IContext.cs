using System.Collections.Generic;
using System.Data;

namespace Winner.Persistence
{
    public interface IContext
    {
  

        /// <summary>
        /// 保存对象
        /// </summary>
        ContextInfo Local { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
        /// <summary>
        /// 得到实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        T Get<T>(object key) where T : EntityInfo;

        /// <summary>
        /// 存储对象
        /// </summary>
        /// <returns></returns>
        IList<IUnitofwork> Save<T>(T entity) where T : EntityInfo;

        /// <summary>
        /// 存储对象
        /// </summary>
        /// <returns></returns>
        IList<IUnitofwork> Save<T>(IList<T> entities) where T : EntityInfo;
        /// <summary>
        /// 存储对象
        /// </summary>
        /// <returns></returns>
        bool Commit(IList<IUnitofwork> unitofworks);
        /// <summary>
        /// 得到实体集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        T GetInfos<T>(QueryInfo query);

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
