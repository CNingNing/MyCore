using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Winner.Persistence.Linq
{
    public class GroupByExpression : IExpression
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
        public GroupByExpression(QueryInfo query)
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
            var builder = new StringBuilder();
            BuilderGroupbyExpression(expression, builder);
            if (builder.Length > 0) builder.Remove(builder.Length - 1, 1);
            Query.GroupBy(builder.ToString());
        }


        /// <summary>
        /// 拼接where
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderGroupbyExpression(Expression expression, StringBuilder builder)
        {
            if (expression == null) return;
            if (BuilderOperatorNodeType(builder, expression)) return;
            if (expression is UnaryExpression)
                BuilderUnaryExpression(expression as UnaryExpression, builder);
            if (expression is MemberExpression)
                BuilderMemberExpression(expression as MemberExpression, builder);
            if (expression is ConstantExpression)
                BuilderConstantExpression(expression as ConstantExpression, builder);
            else if (expression is NewArrayExpression)
                BuilderNewArrayExpression(expression as NewArrayExpression, builder);
            else if (expression is MemberInitExpression)
                BuilderMemberInitExpression(expression as MemberInitExpression, builder);
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
                BuilderGroupbyExpression(((BinaryExpression)expression).Left, builder);
                if (builder[builder.Length - 1].Equals(',')) builder.Remove(builder.Length - 1, 1);
                builder.Append(string.Format(" {0} ", types[expression.NodeType]));
                BuilderGroupbyExpression(((BinaryExpression)expression).Right, builder);
                if (builder[builder.Length - 1].Equals(',')) builder.Remove(builder.Length - 1, 1);
                builder.Append(")");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 拼接UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderUnaryExpression(UnaryExpression expression, StringBuilder builder)
        {
            var lambdaExpression = expression.Operand as LambdaExpression;
            if (lambdaExpression != null)
            {
                var newExpression = lambdaExpression.Body as NewExpression;
                if (newExpression != null)
                    BuilderNewExpression(newExpression, builder);
                else
                    BuilderGroupbyExpression(lambdaExpression.Body, builder);
            }
            else
            {
                builder.Append(string.Format("{0},", expression.ConvertUnaryExpression().Trim(')').Trim('('))); 
            }
        }
        /// <summary>
        /// 拼接NewExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderNewExpression(NewExpression expression, StringBuilder builder)
        {
            foreach (var args in expression.Arguments)
            {
                builder.Append(string.Format("{0},", ExpressionHelper.ReplaceExpressionParamter(args.ToString())));
            }
        }
        /// <summary>
        /// 拼接数组
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderNewArrayExpression(NewArrayExpression expression, StringBuilder builder)
        {
            foreach (var e in expression.Expressions)
            {
                if (e is UnaryExpression) BuilderUnaryExpression(e as UnaryExpression, builder);
                else if (e is MemberExpression) BuilderMemberExpression(e as MemberExpression, builder);
                else
                    BuilderGroupbyExpression(e, builder);
            }
        }

        /// <summary>
        /// 初始化类
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderMemberInitExpression(MemberInitExpression expression, StringBuilder builder)
        {
            foreach (MemberAssignment bind in expression.Bindings)
            {
                if (bind.Expression.NodeType == ExpressionType.MemberAccess)
                    BuilderMemberAssignmentAccess(bind, builder);
                else if (bind.Expression.NodeType == ExpressionType.MemberInit)
                    BuilderMemberInitExpression(bind.Expression as MemberInitExpression,
                        builder);
                else
                    BuilderGroupbyExpression(bind.Expression, builder);
                builder.Append(",");

            }
        }
        /// <summary>
        /// 拼接MemberExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderMemberExpression(MemberExpression expression, StringBuilder builder)
        {
            builder.Append(string.Format("{0},",
               ExpressionHelper.ReplaceExpressionParamter(expression.ToString())));
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
        /// <summary>
        /// 拼接常规
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderMemberAssignmentAccess(MemberAssignment expression, StringBuilder builder)
        {
            builder.Append(ExpressionHelper.ReplaceExpressionParamter(expression.Expression.ToString()));
        }
        #endregion

        
    }
}
