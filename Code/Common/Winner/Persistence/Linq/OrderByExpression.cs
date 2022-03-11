using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Winner.Persistence.Linq
{
    public class OrderByExpression : IExpression
    {
        #region 属性
        /// <summary>
        /// 查询内容
        /// </summary>
        public QueryInfo Query { get; private set; }
        /// <summary>
        /// 是否继续排序
        /// </summary>
        public bool IsThen { get; set; }
        /// <summary>
        /// 排序方式
        /// </summary>
        public string OrderType { get; set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isThen"></param>
        /// <param name="orderType"></param>
        public OrderByExpression(QueryInfo query, bool isThen, string orderType)
        {
            Query = query;
            IsThen = isThen;
            OrderType = orderType;
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
            var builder = new StringBuilder();
            BuilderOrderbyExpression(expression, builder);
            Query.OrderBy(IsThen ?
                string.Format("{0},{1} {2}", Query.OrderByExp, builder, OrderType)
                : string.Format("{0} {1}", builder, OrderType));
        }

 

        #endregion

        /// <summary>
        /// 拼接where
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderOrderbyExpression(Expression expression, StringBuilder builder)
        {
            if (expression == null) return;
            if(expression is LambdaExpression)
                expression = ((LambdaExpression)expression).Body;
            if (BuilderOperatorNodeType(builder, expression)) return;
            if (expression is UnaryExpression)
                BuilderUnaryExpression(expression as UnaryExpression, builder);
            else if (expression is MemberExpression)
                BuilderMemberExpression(expression as MemberExpression, builder);
            if (expression is ConstantExpression)
                BuilderConstantExpression(expression as ConstantExpression, builder);
        }
        /// <summary>
        /// 拼接连接
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="expression"></param>
        protected virtual bool BuilderOperatorNodeType(StringBuilder builder, Expression expression)
        {
            var types = new Dictionary<ExpressionType, string>
                {
                    { ExpressionType.Add, "+" },{ ExpressionType.Subtract, "-" },
                    { ExpressionType.Multiply, "*" },{ ExpressionType.Divide, "/" },
                    { ExpressionType.Modulo, "%" }
                };
            if (types.ContainsKey(expression.NodeType))
            {
                builder.Append("(");
                BuilderOrderbyExpression(((BinaryExpression)expression).Left, builder);
                builder.Append(string.Format(" {0} ", types[expression.NodeType]));
                BuilderOrderbyExpression(((BinaryExpression)expression).Right, builder);
                builder.Append(")");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderUnaryExpression(UnaryExpression expression, StringBuilder builder)
        {
            var lambdaExpression = expression.Operand as LambdaExpression;
            if (lambdaExpression != null)
            {
                BuilderOrderbyExpression(lambdaExpression.Body, builder);
            }
            else
            {
                builder.Append(string.Format("{0},", expression.ConvertUnaryExpression().Trim(')').Trim('(')));
            }
        }
        /// <summary>
        /// 拼接MemberExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderMemberExpression(MemberExpression expression, StringBuilder builder)
        {
            builder.Append(ExpressionHelper.ReplaceExpressionParamter(expression.ToString()));
        }
        /// <summary>
        /// 拼接MemberExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderConstantExpression(ConstantExpression expression, StringBuilder builder)
        {
            builder.Append(expression.Value);
        }
    }
}
