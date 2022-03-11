using System;
using System.Collections.Generic;
using System.Linq;

namespace Winner.Persistence.Data
{
    [Serializable]
    public class OrmDataBaseInfo
    {
        protected delegate void ResetExceptionHandler();
        public string Index { get; set; }
        /// <summary>
        /// 是否默认
        /// </summary>
        public virtual bool IsDefault { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public virtual string ConnnectString { get; set; }
        /// <summary>
        /// 编译器名称
        /// </summary>
        public virtual string CompilerName { get; set; }
        /// <summary>
        ///当前连接数
        /// </summary>
        public virtual int CurrentCount { get; set; }
        /// <summary>
        /// 是否异常才启动故障读转移
        /// </summary>
        public virtual bool IsGetLoadBalance { get; set; }
        /// <summary>
        /// 是否异常才启动故障读转移
        /// </summary>
        public virtual bool IsSetLoadBalance { get; set; }
        /// <summary>
        /// 读取故障转移的数据
        /// </summary>
        public virtual IList<OrmDataBaseInfo> GetFailovers { get; set; }
        /// <summary>
        /// 写的故障转移的数据
        /// </summary>
        public virtual IList<OrmDataBaseInfo> SetFailovers { get; set; }

        /// <summary>
        /// 是否为异常
        /// </summary>
        public virtual bool IsException { get; set; }
        /// <summary>
        /// 是否启用了在线检查
        /// </summary>
        public virtual bool IsStartCheckAlive { get; set; }

        private int _checkAlivePeriod = 5000;
        /// <summary>
        /// 重置为非异常周期
        /// </summary>
        public virtual int CheckAlivePeriod
        {
            get { return _checkAlivePeriod; }
            set { _checkAlivePeriod = value; }
        }


        /// <summary>
        /// 得到读取时候的连接
        /// </summary>
        /// <returns></returns>
        public virtual IList<OrmDataBaseInfo> GetAllGetOrmDataBase()
        {
            var rev = new List<OrmDataBaseInfo>();
            if (GetFailovers != null && GetFailovers.Count > 0)
            {
                rev = IsGetLoadBalance ? GetFailovers.Where(it => it.IsException==false).OrderBy(it => it.CurrentCount).ToList() : GetFailovers.Where(it => it.IsException==false).ToList();
            }
            if(IsException==false) rev.Add(this) ;
            //else rev.Insert(0, this);
            return rev;
        }
        /// <summary>
        /// 得到写时候的连接
        /// </summary>
        /// <returns></returns>
        public virtual IList<OrmDataBaseInfo> GetAllSetOrmDataBase()
        {
            var rev = new List<OrmDataBaseInfo>();
            if (SetFailovers != null && SetFailovers.Count > 0)
            {
                rev = IsSetLoadBalance ? SetFailovers.OrderBy(it => it.IsException).ThenBy(it => it.CurrentCount).ToList() : SetFailovers.OrderBy(it => it.IsException).ToList();
            }
            if (IsException) rev.Add(this);
            else rev.Insert(0, this);
            return rev;
        }
        /// <summary>
        /// 使用连接
        /// </summary>
        public virtual void UseConnect()
        {
            if (CurrentCount>=int.MaxValue)
            {
                CurrentCount = 0;
            }
            CurrentCount++;
        }
    }

}
