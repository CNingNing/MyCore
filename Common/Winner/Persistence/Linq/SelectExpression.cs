using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Winner.Persistence.Linq
{
    public class SelectExpression : IExpression
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
        public SelectExpression(QueryInfo query)
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
            Expression exp;
            if (expression.NodeType == ExpressionType.Lambda)
                exp = ((LambdaExpression)expression).Body;
            else
            {
                var unaryExpression = expression as UnaryExpression;
                if (unaryExpression == null) return;
                exp = ((LambdaExpression) unaryExpression.Operand).Body;
            }
            var builder = new StringBuilder();
            BuilderSelectExpression(exp, builder);
            if (builder.Length > 0 && builder[builder.Length - 1].Equals(',')) builder.Remove(builder.Length - 1, 1);
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
            if (BuilderOperatorNodeType(builder,expression))return;
            if (expression is MemberExpression)
                BuilderMemberExpression(expression as MemberExpression, builder);
            else if (expression is NewArrayExpression)
                BuilderNewArrayExpression(expression as NewArrayExpression, builder);
            else if (expression is MemberInitExpression)
                BuilderMemberInitExpression(expression as MemberInitExpression, builder,null);
            else if (expression is ParameterExpression) 
                BuilderParameterExpression(expression as ParameterExpression, builder);
            else if (expression is MethodCallExpression)
                BuilderMethodCallExpression(expression as MethodCallExpression, builder);
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
                builder.Append( "(");
                BuilderSelectExpression(((BinaryExpression) expression).Left, builder);
                if (builder[builder.Length - 1].Equals(',')) builder.Remove(builder.Length - 1, 1);
                builder.Append(string.Format(" {0} ", types[expression.NodeType]));
                BuilderSelectExpression(((BinaryExpression) expression).Right, builder);
                if (builder[builder.Length - 1].Equals(',')) builder.Remove(builder.Length - 1, 1);
                builder.Append(")");
                return true;
            }
            return false;
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
                else if (e is MethodCallExpression)
                    BuilderMethodCallExpression(e as MethodCallExpression, builder);
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
            builder.Append("*,");
        }

        /// <summary>
        /// 初始化类
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        /// <param name="asName"></param>
        protected virtual void BuilderMemberInitExpression(MemberInitExpression expression, StringBuilder builder, string asName)
        {
            foreach (MemberAssignment bind in expression.Bindings)
            {
                if (bind.Expression.NodeType == ExpressionType.MemberAccess)
                    BuilderMemberAssignmentAccess(bind, builder, asName);
                else if (bind.Expression.NodeType == ExpressionType.MemberInit)
                    BuilderMemberInitExpression(bind.Expression as MemberInitExpression,
                        builder, bind.Member.Name);
                else if (bind.Expression.NodeType == ExpressionType.Call)
                    BuilderMethodCallExpression(bind.Expression as MethodCallExpression, builder, bind.Member.Name);
                else
                {
                    BuilderSelectExpression(bind.Expression, builder);
                    builder.Append(string.Format(" as {0}", MergeName(asName, bind.Member.Name)));
                }
            }
        }

        /// <summary>
        /// 拼接常规
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        /// <param name="asName"></param>
        protected virtual void BuilderMemberAssignmentAccess(MemberAssignment expression, StringBuilder builder,string asName)
        {
            var name = ExpressionHelper.ReplaceExpressionParamter(expression.Expression.ToString());
            if (expression.Expression.ToString().Contains(".Key."))
            {
                if (((MemberExpression) expression.Expression).Expression.Type.ToString().Contains("<>"))
                {
                    name = name.Replace("Key.", "");
                }
            }
            builder.Append(string.Format("{0} as {1},", name, MergeName(asName, expression.Member.Name)));
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
        /// 拼接UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderUnaryExpression(UnaryExpression expression,StringBuilder builder)
        {
            builder.Append(string.Format("{0},", expression.ConvertUnaryExpression()));
        }

        /// <summary>
        /// 拼接MethodCallExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        /// <param name="asName"></param>
        protected virtual void BuilderMethodCallExpression(MethodCallExpression expression, StringBuilder builder,string asName=null)
        {
            if(expression==null)
                return;
            switch (expression.Method.Name)
            {
                case "Select":
                    BuilderSelectMethodCallExpression(expression, builder);
                    break;
                case "Count":
                    builder.Append("Count()");
                    break;
                case "Sum":case "Min":case "Max":case "Average":
                    BuilderGroupMethodCallExpression(expression, builder);
                    break;
                default:
                    BuilderDefaultMethodCallExpression(expression, builder);break;
            }
            builder.Append(string.IsNullOrEmpty(asName) ? "," :string.Format(" as {0},",asName) );
        }
        /// <summary>
        /// 编译
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="expression"></param>
        protected virtual void BuilderQueryProvider(MethodCallExpression expression, StringBuilder builder)
        {
             var t = expression.Arguments[0] as MethodCallExpression;
            var query = new QueryInfo { Parameters = Query.Parameters };
            var queryProvider = new QueryProvider(query){IsSelectWhere = true}; 
            while (t != null)
            {
                queryProvider.Translate(t);
                if (t.Arguments.Count > 0 && t.Arguments[0].NodeType == ExpressionType.Call)
                {
                    t = t.Arguments[0] as MethodCallExpression;
                }
                else
                {
                    break;
                }
            }
            builder.Append(t==null?"":ExpressionHelper.ReplaceExpressionParamter(t.Arguments[0].ToString()));
            if (query.PageSize > 0)
            {
                builder.AppendFormat(".Take({0}).Skip({1})", query.PageSize, query.PageSize * query.PageIndex);
            }
            if (!string.IsNullOrEmpty(query.WhereExp))
            {
                builder.AppendFormat(".Where({0})", query.WhereExp);
            }
            if (!string.IsNullOrEmpty(query.OrderByExp))
            {
                builder.AppendFormat(".OrderBy({0})", query.OrderByExp);
            }
            if (!string.IsNullOrEmpty(query.GroupByExp))
            {
                builder.AppendFormat(".GroupBy({0})", query.GroupByExp);
            }
            if (!string.IsNullOrEmpty(query.HavingExp))
            {
                builder.AppendFormat(".Having({0})", query.HavingExp);
            }
            Query.Parameters = query.Parameters;
        }
        /// <summary>
        /// 添加select方法
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderSelectMethodCallExpression(MethodCallExpression expression, StringBuilder builder)
        {
            if (expression.Arguments[0].NodeType == ExpressionType.Call)
            {
                var n = new StringBuilder();
                BuilderQueryProvider(expression, n);
                builder.Append(n);
            }
            else
            {
                builder.AppendFormat(ExpressionHelper.ReplaceExpressionParamter(expression.Arguments[0].ToString()));
            }
            var s = new StringBuilder();
            BuilderSelectExpression(((LambdaExpression)expression.Arguments[1]).Body, s);
            if (s.Length > 0) s.Remove(s.Length - 1, 1);
            builder.Append(string.Format(".Select({0})", s));
        }

        /// <summary>
        /// 添加统计方法
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderGroupMethodCallExpression(MethodCallExpression expression, StringBuilder builder)
        {
            var b = new StringBuilder();
            BuilderSelectExpression(((LambdaExpression)expression.Arguments[1]).Body, b);
            if (b.Length > 0) b.Remove(b.Length - 1, 1);
            builder.Append(string.Format("{0}({1})", expression.Method.Name, b));
        }

        /// <summary>
        /// 拼接默认方法
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderDefaultMethodCallExpression(MethodCallExpression expression, StringBuilder builder)
        {
            var b = new StringBuilder();
            BuilderSelectExpression(expression.Arguments.Count > 0 ? expression.Arguments[0] :
                expression.Object, b);
            if (b.Length > 0) b.Remove(b.Length - 1, 1);
            builder.Append(b);
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
