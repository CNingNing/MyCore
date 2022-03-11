using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Winner.Persistence.Data;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Compiler.Common
{
    public abstract class UnitofworkBase:IUnitofwork
    {
        /// <summary>
        /// 数据定义
        /// </summary>
        public OrmDataBaseInfo OrmDataBase { get; set; }

        /// <summary>
        /// 存储对象
        /// </summary>
        public IList<SaveInfo> Infos { get; set; }
        #region 接口的定义

        public bool IsExcute { get; set; }

        public bool IsDispose { get; set; }
        public int Sequence { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object GetObject()
        {
            return Infos;
        }
        public virtual void Execute()
        {
           
        }

        public virtual void Commit()
        {
          
        }

        public virtual void Rollback()
        {

        }
        #endregion

        /// <summary>
        /// 得到链接
        /// </summary>
        /// <param name="failoverDataBases"></param>
        /// <returns></returns>
        protected virtual T GetConnnection<T>(IList<OrmDataBaseInfo> failoverDataBases) where T : DbConnection
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
            catch (Exception)
            {
                ormDataBase.IsException = true;
                return null;
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
            return default(T);
        }
        /// <summary>
        /// 重写HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return OrmDataBase.ConnnectString.GetHashCode();
        }
    }
}
