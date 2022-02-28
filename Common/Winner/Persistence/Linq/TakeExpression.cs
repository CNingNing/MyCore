using System.Linq.Expressions;

namespace Winner.Persistence.Linq
{
    public class TakeExpression : IExpression
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
        public TakeExpression(QueryInfo query)
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
            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
            {
                Query.SetPageSize((int)constantExpression.Value);
                return;
            }
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                Query.SetPageSize((int) memberExpression.GetExpressionValue());
            }
        }


      

        #endregion

        
    }
}
