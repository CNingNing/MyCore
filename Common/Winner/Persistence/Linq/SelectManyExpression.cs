using System.Linq.Expressions;
using System.Text;

namespace Winner.Persistence.Linq
{
    public class SelectManyExpression : IExpression
    {
        #region 属性
        /// <summary>
        /// 查询内容
        /// </summary>
        public QueryInfo Query { get; private set; }
        /// <summary>
        /// 主要的参数名称
        /// </summary>
        protected string ParameterName { get; set; }
        /// <summary>
        /// 附属参数名称
        /// </summary>
        protected string SubParamterName { get; set; }
        /// <summary>
        /// 一对多的属性名称
        /// </summary>
        protected string SubName { get; set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="query"></param>
        public SelectManyExpression(QueryInfo query)
        {
            Query = query;
        }

        #endregion

        #region 翻译

        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual void Translate(Expression expression)
        {
            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression == null) return;
            var unaryExpression = (UnaryExpression) methodCallExpression.Arguments[2];
            var lambdaExpression = (LambdaExpression) unaryExpression.Operand;
            var builder = new StringBuilder();
            ParameterName= lambdaExpression.Parameters[0].Name;
            SubParamterName = lambdaExpression.Parameters[1].Name;
            SubName= ((UnaryExpression)methodCallExpression.Arguments[1]).ConvertUnaryExpression();
            BuilderSelectExpression(lambdaExpression.Body, builder);
            if (builder.Length > 0) builder.Remove(builder.Length - 1, 1);
            Query.Select(builder.ToString());
        }


        /// <summary>
        /// 拼接where
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderSelectExpression(Expression expression, StringBuilder builder)
        {
            if (expression == null) return;
            if (expression is MemberExpression)
                BuilderMemberExpression(expression as MemberExpression, builder);
            else if (expression is NewArrayExpression)
                BuilderNewArrayExpression(expression as NewArrayExpression, builder);
            else if (expression is MemberInitExpression)
                BuilderMemberInitExpression(expression as MemberInitExpression, builder, null);
            else if (expression is ParameterExpression)
                BuilderParameterExpression(expression as ParameterExpression, builder);
            else if (expression is NewExpression)
                BuilderNewExpression(expression as NewExpression, builder);
        }

        /// <summary>
        /// 拼接New
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderNewExpression(NewExpression expression, StringBuilder builder)
        {
            foreach (var e in expression.Arguments)
            {
                if (e is UnaryExpression) BuilderUnaryExpression(e as UnaryExpression, builder);
                else if (e is MemberExpression) BuilderMemberExpression(e as MemberExpression, builder);
                else
                    BuilderSelectExpression(e, builder);
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
                    BuilderSelectExpression(e, builder);
            }
        }

        /// <summary>
        /// 初始化参数
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderParameterExpression(ParameterExpression expression, StringBuilder builder)
        {
            var name = SubParamterName.Equals(expression.ToString()) ? string.Format("{0}.Select(*)", SubName) : "*";
            builder.Append(string.Format("{0},",name));
        }

        /// <summary>
        /// 初始化类
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        /// <param name="asName"></param>
        protected virtual void BuilderMemberInitExpression(MemberInitExpression expression, StringBuilder builder,string asName)
        {
            foreach (MemberAssignment bind in expression.Bindings)
            {
                if (bind.Expression.NodeType == ExpressionType.MemberAccess)
                    BuilderMemberAssignmentAccess(bind, builder, asName);
                else if (bind.Expression.NodeType == ExpressionType.MemberInit)
                    BuilderMemberInitExpression(bind.Expression as MemberInitExpression,
                        builder, bind.Member.Name);
                else
                    BuilderSelectExpression(bind.Expression, builder);
            }
        }

        /// <summary>
        /// 拼接常规
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        /// <param name="asName"></param>
        protected virtual void BuilderMemberAssignmentAccess(MemberAssignment expression, StringBuilder builder, string asName)
        {
            var name = ExpressionHelper.ReplaceExpressionParamter(expression.Expression.ToString());
            name = GetSubName(expression.Expression.ToString(), name);
            builder.Append(string.Format("{0} as {1},", name, MergeName(asName, expression.Member.Name)));
        }

        /// <summary>
        /// 拼接MemberExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderMemberExpression(MemberExpression expression, StringBuilder builder)
        {
            var name = ExpressionHelper.ReplaceExpressionParamter(expression.ToString());
            name = GetSubName(expression.ToString(), name);
            builder.Append(string.Format("{0},", name));
        }

        /// <summary>
        /// 拼接UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderUnaryExpression(UnaryExpression expression, StringBuilder builder)
        {
            var name = expression.ConvertUnaryExpression();
            name = GetSubName(expression.ToString(), name);
            builder.Append(string.Format("{0},", name));
        }
 
        /// <summary>
        /// 得到子类名
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual string GetSubName(string expression, string name)
        {
            if (expression.Contains(string.Format("{0}.", SubParamterName)))
                return string.Format("{0}.Select({1})", SubName, name);
            return name;
        }

        /// <summary>
        /// 得到标志
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="newString"></param>
        /// <returns></returns>
        protected virtual string MergeName(string flag, string newString)
        {
            return string.IsNullOrEmpty(flag)
                             ? newString
                             : string.Format("{0}.{1}", flag,
                                            newString);
        }
  
        #endregion

 
    }
}
