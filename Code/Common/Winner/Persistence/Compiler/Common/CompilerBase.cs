using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Winner.Log;
using Winner.Persistence.Compiler.Reverse;
using Winner.Persistence.Data;
using Winner.Persistence.Relation;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Compiler.Common
{
    public abstract class CompilerBase : ICompiler
    {
        #region 属性
        /// <summary>
        /// 转换对象实例
        /// </summary>
        public virtual IFill Fill { get; set; }=new XmlFill();

        /// <summary>
        /// 转换对象实例
        /// </summary>
        public virtual ISaveCompiler SaveCompiler { get; set; } = new SaveCompiler();
        /// <summary>
        /// 解析查询实例
        /// </summary>
        public virtual IQueryCompiler QueryCompiler { get; set; } = new QueryCompiler();

        private ILog _log;

        /// <summary>
        /// 实例
        /// </summary>
        public ILog Log
        {
            get
            {
                if (_log == null)
                    _log = Creator.Get<ILog>();
                return _log;
            }
            set { _log = value; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        protected CompilerBase()
        {
            
        }
        /// <summary>
        ///填充实例，存储实例，查询实例
        /// </summary>
        /// <param name="fill"></param>
        /// <param name="save"></param>
        /// <param name="queryCompiler"></param>
        protected CompilerBase(IFill fill, ISaveCompiler save, IQueryCompiler queryCompiler)
        {
            Fill = fill;
            SaveCompiler = save;
            QueryCompiler = queryCompiler;
        }
        #endregion

        #region 方法

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual T SetProperty<T>(IDataReader reader, OrmObjectInfo obj)
        {
            return Fill.Reverse<T>(reader, obj);
        }
        /// <summary>
        /// 设置属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual T SetProperty<T>(DataTable table, OrmObjectInfo obj)
        {
            return Fill.Reverse<T>(table, obj);
        }
        /// <summary>
        /// 解析查询
        /// </summary>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual void Translate(DbCommand command, OrmObjectInfo obj, QueryInfo query)
        {
            QueryCompiler.Translate(command, obj,query);
        }

        /// <summary>
        /// 得到链接
        /// </summary>
        /// <param name="failoverDataBases"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual T GetConnnection<T>(IList<OrmDataBaseInfo> failoverDataBases, QueryInfo query) where T:DbConnection
        {
            return failoverDataBases.Select(TryGetConnection<T>).FirstOrDefault(con => con != null);
        }

        /// <summary>
        /// 查询故障转移
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <returns></returns>
        protected virtual T TryGetConnection<T>(OrmDataBaseInfo ormDataBase) where T : DbConnection
        {
            var sqlcon = CreateTryConnection<T>(ormDataBase); 
            try
            {
                if (sqlcon.State != ConnectionState.Open)
                    sqlcon.Open();
                ormDataBase.UseConnect();
                return sqlcon;
            }
            catch (Exception ex)
            {
                Log.AddException(new Exception($"{ormDataBase.ConnnectString}:{ex.Message}",ex));
                ormDataBase.IsException = true;
                var task = new Thread(() => { CheckConnectionAlive<T>(ormDataBase); });
                task.Start();
                return null;
            }
        }
        private static object CheckAliveLocker=new object();
        /// <summary>
        /// 检查连接一次
        /// </summary>
        /// <param name="ormDataBase"></param>
        protected virtual void CheckConnectionAlive<T>(OrmDataBaseInfo ormDataBase) where T : DbConnection
        {
            lock (CheckAliveLocker)
            {
                if(ormDataBase.IsStartCheckAlive || !ormDataBase.IsException)
                    return;
                ormDataBase.IsStartCheckAlive = true;
            }
            System.Threading.Thread.Sleep(ormDataBase.CheckAlivePeriod);
            try
            {
                using (var sqlcon = CreateTryConnection<T>(ormDataBase))
                {
                    sqlcon.ConnectionString = ormDataBase.ConnnectString;
                    sqlcon.Open();
                    sqlcon.Close();
                    ormDataBase.IsException = false;
                    ormDataBase.IsStartCheckAlive = false;
                }
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                ormDataBase.IsException = true;
                ormDataBase.IsStartCheckAlive = false;
                var task = new Thread(() => { CheckConnectionAlive<T>(ormDataBase); });
                task.Start();
            }
        }
        /// <summary>
        /// 创建连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <returns></returns>
        protected virtual T CreateTryConnection<T>(OrmDataBaseInfo ormDataBase) where T : DbConnection
        {
            return  default(T);
        }

        #endregion
 

        #region 接口的定义
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public abstract T GetInfos<T>(OrmDataBaseInfo ormDataBase, OrmObjectInfo obj, QueryInfo query);

        /// <summary>
        /// 得到事务
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <param name="info"></param>
        /// <param name="unitOfWorks"></param>
        public abstract void AddUnitofwork(OrmDataBaseInfo ormDataBase, SaveInfo info, IList<IUnitofwork> unitOfWorks);

        /// <summary>
        /// 执行查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public abstract T ExecuteQuery<T>(OrmDataBaseInfo ormDataBase, string commandText, CommandType commandType, params object[] parameters);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public abstract int ExecuteCommand(OrmDataBaseInfo ormDataBase, string commandText, CommandType commandType, params object[] parameters);

        #endregion






    }
}
