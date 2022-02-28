using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Data;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Compiler.SqlServer
{
    public class SqlServerUnitofwork : UnitofworkBase
    {
        #region 属性
        /// <summary>
        /// 执行的命令
        /// </summary>
        public ISaveCompiler SaveCompiler { get; set; }
        /// <summary>
        /// 连接对象集合
        /// </summary>
        public SqlConnection Connection { get; set; }
        /// <summary>
        /// 事务对象集合
        /// </summary>
        public SqlTransaction Transaction { get; set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 连接字符串，xql命令集合,对象
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <param name="infos"></param>
        /// <param name="saveCompiler"></param>
        public SqlServerUnitofwork(OrmDataBaseInfo ormDataBase, IList<SaveInfo> infos, ISaveCompiler saveCompiler)
        {
            Connection = new SqlConnection(ormDataBase.ConnnectString);
            OrmDataBase = ormDataBase;
            Infos = infos;
            SaveCompiler = saveCompiler;
        }
        #endregion

        #region 接口的实现

        /// <summary>
        /// 执行
        /// </summary>
        public override void Execute()
        {
            try
            {
                Begin();
              
                SqlCommand cmd = GetSqlCommand();
                Infos = Infos.OrderByDescending(it => it.Entity.SaveSequence).ToList();
                var contentEntities = Infos.Select(it => it.Entity).ToList();
                foreach (var info in Infos)
                {
                    try
                    {
                        var saveCompiler = new SaveCompilerInfo
                        {
                            Command = cmd,
                            ContentEntities = contentEntities,
                            SaveInfo = info,
                            OrmDataBase = OrmDataBase,
                            IsSaveParameters = false
                        };
                        SaveCompiler.Save(saveCompiler);
                    }
                    catch (Exception e)
                    {
                        var id= string.IsNullOrWhiteSpace(info.Object?.PrimaryProperty?.PropertyName) ? "": info.Entity?.GetProperty(info.Object.PrimaryProperty.PropertyName);
                        throw new Exception($"{e.Message}:{info.Object?.ObjectName}:Id is {id}", e);
                    }
                   
                }
              
            }
            catch (Exception ex)
            {
                Close();
                throw ex;
            }

        }
     


        /// <summary>
        /// 提交
        /// </summary>
        public override void Commit()
        {
            try
            {
                Transaction.Commit();
            }
            finally
            {
                Close();
            }
        

        }
        /// <summary>
        /// 回滚
        /// </summary>
        public override void Rollback()
        {
            try
            {
                if (Transaction != null)
                    Transaction.Rollback();
            }
            finally
            {
                Close();
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 开启事务
        /// </summary>
        protected virtual void Begin()
        {
            SetConnnection();
            Transaction = Connection.BeginTransaction();
        }

        /// <summary>
        /// 得到链接
        /// </summary>
        /// <returns></returns>
        protected virtual void SetConnnection()
        {
            Connection = GetConnnection<SqlConnection>(OrmDataBase.GetAllSetOrmDataBase());
        }
        /// <summary>
        /// 重写创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <returns></returns>
        protected override T CreateTryConnection<T>(OrmDataBaseInfo ormDataBase)
        {
            return new SqlConnection(ormDataBase.ConnnectString) as T;
        }

        /// <summary>
        /// 得到SqlCommand
        /// </summary>
        /// <returns></returns>
        protected virtual SqlCommand GetSqlCommand()
        {
            var sqlcmd = new SqlCommand {Connection = Connection, Transaction = Transaction};
            return sqlcmd;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        protected virtual void Close()
        {
            if (Connection != null && Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        #endregion
    }
}
