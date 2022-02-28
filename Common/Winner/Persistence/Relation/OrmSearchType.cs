namespace Winner.Persistence.Relation
{
    public enum OrmSearchType
    {
        /// <summary>
        /// 模糊
        /// </summary>
        Like=1,
        /// <summary>
        /// 全文索引
        /// </summary>
        FullText=2,
        /// <summary>
        /// 搜索
        /// </summary>
        Search = 4
    }

}
