using System.Linq.Expressions;

namespace Winner.Persistence.Linq
{
    public class DistinctExpression : IExpression
    {
        #region 属性
        /// <summary>
        /// 查询内容
        /// </summary>
        public QueryInfo Query { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="query"></param>
        public DistinctExpression(QueryInfo query)
        {
            Query = query;
          
        }
        #endregion

        #region 解析函数

        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual void Translate(Expression expression)
        {
           Query.Distinct();
        }

 

        #endregion

        
    }
}
