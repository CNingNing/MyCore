namespace Winner.Persistence
{


    public interface IUnitofwork
    {

        /// <summary>
        /// 是否释放
        /// </summary>
        bool IsExcute { get; set; }
        /// <summary>
        /// 是否释放
        /// </summary>
        bool IsDispose { get; set; }
        /// <summary>
        /// 是否释放
        /// </summary>
        int Sequence { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object GetObject();
        /// <summary>
        /// 执行
        /// </summary>
        void Execute();
        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();
        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();
    }
}
