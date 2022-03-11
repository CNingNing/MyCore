namespace Winner.Persistence.ContextStorage
{

    public interface IContextStorage
    {
        /// <summary>
        /// 得到上下文
        /// </summary>
        /// <returns></returns>
        ContextInfo Get();
        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="contexnt"></param>
        void Set(ContextInfo contexnt);
    }

}
