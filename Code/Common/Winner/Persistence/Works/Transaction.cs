using System;
using System.Collections.Generic;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Works
{
    public class Transaction : ITransaction
    {
        #region 属性
    

     
        /// <summary>
        /// Orm实例
        /// </summary>
        public IOrm Orm { get; set; }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public Transaction()
        { 
        }

        /// <summary>
        /// 缓存实例，同步信息实例，编译器实例,ORM信息实例
        /// </summary>
        /// <param name="orm"></param>
        public Transaction(IOrm orm)
        {
            Orm = orm;
        }
        #endregion

        #region 接口的实现
        
        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="unitOfWorks"></param>
        /// <returns></returns>
        public virtual bool Commit(IList<IUnitofwork> unitOfWorks)
        {
            if (unitOfWorks == null || unitOfWorks.Count == 0)
                return false;
            CommitUnitofworks(unitOfWorks);
            return true;
        }
       
        #endregion

        #region 提交

        /// <summary>
        /// 存储
        /// </summary>
        /// <param name="unitofworks"></param>
        protected virtual void CommitUnitofworks(IList<IUnitofwork> unitofworks)
        {
            if (unitofworks == null) return;
            try
            {
                ExecuteTrains(unitofworks);
                CommitTrains(unitofworks);
            }
            catch (Exception ex)
            {
                RollbackTrains(unitofworks);
                throw ex;
            }

        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        /// <param name="unitofworks"></param>
        protected virtual void RollbackTrains(IList<IUnitofwork> unitofworks)
        {
            foreach (var unitofwork in unitofworks)
            {
                if (unitofwork != null && unitofwork.IsExcute && !unitofwork.IsDispose)
                {
                    unitofwork.IsDispose = true;
                    unitofwork.Rollback();
                }
                    
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="unitofworks"></param>
        protected virtual void CommitTrains(IList<IUnitofwork> unitofworks)
        {
            foreach (var unitofwork in unitofworks)
            {
                if (unitofwork != null && unitofwork.IsExcute && !unitofwork.IsDispose)
                {
                    try
                    {
                        unitofwork.IsDispose = true;
                        unitofwork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitofwork.IsDispose = false;
                        throw ex;
                    }
                }
                    
            }
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="unitofworks"></param>
        protected virtual void ExecuteTrains(IList<IUnitofwork> unitofworks)
        {
            foreach (var unitofwork in unitofworks)
            {
                if (unitofwork != null && !unitofwork.IsExcute && !unitofwork.IsDispose)
                {
                    try
                    {
                        unitofwork.IsExcute = true;
                        unitofwork.Execute();
                    }
                    catch(Exception ex)
                    {
                        unitofwork.IsExcute = false;
                        throw ex;
                    }
                }
                  
            }
        }
        #endregion



       

    }
}
