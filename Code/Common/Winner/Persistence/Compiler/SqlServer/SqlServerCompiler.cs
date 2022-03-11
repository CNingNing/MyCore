using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Compiler.Reverse;
using Winner.Persistence.Data;
using Winner.Persistence.Relation;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Compiler.SqlServer
{
    public class SqlServerCompiler : CompilerBase
    {
        /// <summary>
        /// 转换对象实例
        /// </summary>
        public override ISaveCompiler SaveCompiler { get; set; } = new SqlServerSaveCompiler();
        /// <summary>
        /// 解析查询实例
        /// </summary>
        public override IQueryCompiler QueryCompiler { get; set; } = new SqlServerQueryCompiler();
        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public SqlServerCompiler()
        { 
        }
              /// <summary>
        ///填充实例，存储实例，查询实例
        /// </summary>
        /// <param name="fill"></param>
        /// <param name="saveCompiler"></param>
        /// <param name="findCompiler"></param>
        public SqlServerCompiler(IFill fill, ISaveCompiler saveCompiler, IQueryCompiler findCompiler)
            : base(fill, saveCompiler, findCompiler)
        { }
        #endregion

        #region 接口的实现

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T GetInfos<T>(OrmDataBaseInfo ormDataBase, OrmObjectInfo obj, QueryInfo query)
        {
            if (obj == null) return default(T);
            var cmd=new SqlCommand();
            Translate(cmd, obj, query);
            return GetInfosByCommand<T>(ormDataBase, obj, cmd, query);
        }

        /// <summary>
        /// 得到事务
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <param name="info"></param>
        /// <param name="unitOfWorks"></param>
        /// <returns></returns>
        public override void AddUnitofwork(OrmDataBaseInfo ormDataBase, SaveInfo info, IList<IUnitofwork> unitOfWorks)
        {
           
            foreach (var unitOfWork in unitOfWorks)
            {
                if (!unitOfWork.IsExcute && !unitOfWork.IsDispose && unitOfWork.GetHashCode() == ormDataBase.ConnnectString.GetHashCode())
                {
                    var tunitofwork = (UnitofworkBase)unitOfWork;
                    tunitofwork.Infos = tunitofwork.Infos ?? new List<SaveInfo>();
                    if (!tunitofwork.Infos.Contains(info))
                        tunitofwork.Infos.Add(info);
                    return;
                }
            }
            unitOfWorks.Add(new SqlServerUnitofwork(ormDataBase, new List<SaveInfo>{ info}, SaveCompiler));
        }

        /// <summary>
        /// 执行查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override T ExecuteQuery<T>(OrmDataBaseInfo ormDataBase, string commandText, CommandType commandType, params object[] parameters)
        {
            if (string.IsNullOrEmpty(commandText) || ormDataBase==null) return default(T);
            using (var sqlcon = GetConnnection<SqlConnection>(ormDataBase.GetAllGetOrmDataBase(),null))
            {
                var sqlcmd = new SqlCommand();
                sqlcmd = FillCommandTypeCommand(sqlcmd, commandText, commandType,parameters);
                sqlcmd.Connection = sqlcon;
                return GetInfosByType<T>(null,null,sqlcmd);
            }
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="ormDataBase"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override int ExecuteCommand(OrmDataBaseInfo ormDataBase, string commandText, CommandType commandType, params object[] parameters)
        {
            if (string.IsNullOrEmpty(commandText) || ormDataBase==null) return 0;
            using (var sqlcon = GetConnnection<SqlConnection>(ormDataBase.GetAllSetOrmDataBase(), null))
            {
                var sqlcmd = new SqlCommand();
                sqlcmd = FillCommandTypeCommand(sqlcmd, commandText,commandType, parameters);
                sqlcmd.Connection = sqlcon;
                return sqlcmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region 存储过程填充

        /// <summary>
        /// 填充存储过程
        /// </summary>
        /// <param name="sqlcmd"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        protected virtual SqlCommand FillCommandTypeCommand(SqlCommand sqlcmd, string commandText, CommandType commandType, params object[] parameters)
        {
            sqlcmd.CommandText = commandText;
            sqlcmd.CommandType = commandType;
            if(parameters.Length>0)
                foreach (var parameter in parameters)
                {
                    sqlcmd.Parameters.Add(parameter);
                }
            return sqlcmd;
        }

        #endregion

        #region 填充

        /// <summary>
        /// CommandInfo得到返回结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <param name="obj"></param>
        /// <param name="cmd"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual T GetInfosByCommand<T>(OrmDataBaseInfo ormDataBase, OrmObjectInfo obj, SqlCommand cmd, QueryInfo query)
        {
            if (string.IsNullOrEmpty(cmd.CommandText))return default(T);
            using (var sqlcon = GetConnnection<SqlConnection>(ormDataBase.GetAllGetOrmDataBase(), query))
            {
                cmd.Connection = sqlcon;
                return GetInfosByType<T>(query,obj, cmd);
            }
        }

        /// <summary>
        /// 选择返回方式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <param name="sqlcmd"></param>
        /// <returns></returns>
        protected virtual T GetInfosByType<T>(QueryInfo query, OrmObjectInfo obj, SqlCommand sqlcmd)
        {
            if (typeof (T) == typeof (DataTable))
            {
                var da=new SqlDataAdapter(sqlcmd);
                var ds=new DataSet();
                da.Fill(ds);
                return (T)(ds.Tables[0] as object);
            }
            if (query.QueryType== QueryType.Parallel)
            {
                var da = new SqlDataAdapter(sqlcmd);
                var ds = new DataSet();
                da.Fill(ds);
                var rev = SetProperty<T>(ds.Tables[0], obj);
                if (query.PageSize != 0 && query.IsReturnCount)
                {
                    query.DataCount = Convert.ToInt32(ds.Tables[1].Rows[0][0]);
                }
                return rev;
            }
            else
            {
                SqlDataReader reader = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection);
                var rev = SetProperty<T>(reader, obj);
                if (query.PageSize != 0 && query.IsReturnCount)
                {
                    reader.NextResult();
                    reader.Read();
                    query.DataCount = Convert.ToInt32(reader[0]);
                }
                return rev;
            }
           
        }

    
     
        /// <summary>
        /// 重写
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ormDataBase"></param>
        /// <returns></returns>
        protected override T CreateTryConnection<T>(OrmDataBaseInfo ormDataBase)
        {
            return new SqlConnection(ormDataBase.ConnnectString) as T;
        }

        #endregion
       
   

    }
}
