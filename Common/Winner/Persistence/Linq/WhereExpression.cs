using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Winner.Persistence.Linq
{
    public class WhereExpression : IExpression
    {
        #region 属性
        /// <summary>
        /// 查询内容
        /// </summary>
        public QueryInfo Query { get; protected set; }
        /// <summary>
        /// 是否分组过
        /// </summary>
        public bool IsGroupBy { get; protected set; }
        /// <summary>
        /// 参数名
        /// </summary>
        public string MethodExp { get; set; }
        /// <summary>
        /// 是否分组过
        /// </summary>
        public bool IsSelectWhere { get; protected set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public WhereExpression()
        {


        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isGroupBy"></param>
        public WhereExpression(QueryInfo query, bool isGroupBy,bool isSelectWhere)
        {
            Query = query;
            IsGroupBy = isGroupBy;
            IsSelectWhere = isSelectWhere;
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
            var whereExp = GetTranslateExp(expression);
            if (IsGroupBy)
                Query.Having(whereExp);
            else
                Query.Where(whereExp);
          
        }
        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual string GetTranslateExp(Expression expression)
        {
            if(IsSelectWhere)
                MethodExp = expression.ToString();
            var builder = new StringBuilder();
            if (expression is LambdaExpression)
            {
                BuilderWhereExpression(((LambdaExpression)expression).Body, builder);
            }
            else
            {
                var unaryExpression = expression as UnaryExpression;
                if (unaryExpression == null) return "";
                BuilderWhereExpression(((LambdaExpression)unaryExpression.Operand).Body, builder);
            }
            return builder.ToString();

        }

        /// <summary>
        /// 拼接where
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderWhereExpression(Expression expression, StringBuilder builder)
        {
            if (expression == null) return;
            if (expression is BinaryExpression)
                BuilderWhereBinaryExpression(expression as BinaryExpression, builder);
            else if (expression.NodeType == ExpressionType.Not)
                BuilderWhereNotExpression(expression as UnaryExpression, builder);
            else
                BuilderWhereDefaultExpression(expression, builder);
        }

        /// <summary>
        /// 拼接Not
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderWhereNotExpression(UnaryExpression expression, StringBuilder builder)
        {
            builder.Append("!");
            BuilderWhereExpression(expression.Operand, builder);
        }

        /// <summary>
        /// 拼接表达式目录树
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        /// <param name="isFirst"></param>
        protected virtual void BuilderWhereBinaryExpression(BinaryExpression expression, StringBuilder builder)
        {
            if (expression.NodeType == ExpressionType.Coalesce)
            {
                builder.Append(GetInvokeValue(expression));
                return;
            }
            //if (!(expression.Left is MethodCallExpression) || expression.ToString().Trim().StartsWith("("))
            //    builder.Append("(");
            if (expression.NodeType == ExpressionType.OrElse)
                builder.Append("(");
            if (expression.Right is UnaryExpression && expression.Left is ConstantExpression)
               builder.Append(ConvertConstantExpression(expression.Left as ConstantExpression, ((UnaryExpression)expression.Right).Operand.Type));
            else
                BuilderWhereExpression(expression.Left, builder);

            BuilderWhereNodeType(builder, expression.NodeType);

            if (expression.Left is UnaryExpression && expression.Right is ConstantExpression)
                 builder.Append(ConvertConstantExpression(expression.Right as ConstantExpression, ((UnaryExpression)expression.Left).Operand.Type));
            else
                BuilderWhereExpression(expression.Right, builder);
            //if (!(expression.Left is MethodCallExpression) || expression.ToString().Trim().EndsWith(")"))
            //    builder.Append(")");
            if (expression.NodeType == ExpressionType.OrElse)
                builder.Append(")");
        }

        /// <summary>
        /// 拼接表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderWhereDefaultExpression(Expression expression, StringBuilder builder)
        {
            if (expression is UnaryExpression)
                BuilderWhereUnaryExpression(expression as UnaryExpression, builder);
            else if (expression is MemberExpression)
                builder.Append(ConvertMemberExpression(expression as MemberExpression));
            else if (expression is ConstantExpression)
                builder.Append(ConvertConstantExpression(expression as ConstantExpression));
            else if (expression is MethodCallExpression)
                builder.Append(ConvertMethodCallExpression(expression as MethodCallExpression));
            else
                builder.Append(GetInvokeValue(expression));
           
        }

        /// <summary>
        ///拼接expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="builder"></param>
        protected virtual void BuilderWhereUnaryExpression(UnaryExpression expression, StringBuilder builder)
        {
            if (expression.Operand.NodeType == ExpressionType.Lambda)
            {
                BuilderWhereExpression(((LambdaExpression) expression.Operand).Body, builder);
            }
            else if (expression.Operand.NodeType == ExpressionType.MemberAccess && !expression.Operand.ToString().Contains("value("))
            {
                builder.Append(expression.ConvertUnaryExpression());
            }
            else
            {
                builder.Append(GetInvokeValue(expression));
            }
        }

        /// <summary>
        /// 拼接连接
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="nodeType"></param>
        protected virtual void BuilderWhereNodeType(StringBuilder builder, ExpressionType nodeType)
        {
            var types = new Dictionary<ExpressionType, string>
                {
                    { ExpressionType.AndAlso, "&&" }, { ExpressionType.And, "&&" },
                    { ExpressionType.Equal, "==" },{ ExpressionType.GreaterThan, ">" },
                    { ExpressionType.GreaterThanOrEqual, ">=" },{ ExpressionType.LessThan, "<" },
                    { ExpressionType.LessThanOrEqual, "<=" },{ ExpressionType.NotEqual, "!=" },
                    { ExpressionType.Or, "||" },{ ExpressionType.OrElse, "||"},
                    { ExpressionType.Add, "+" },{ ExpressionType.Subtract, "-" },
                    { ExpressionType.Multiply, "*" },{ ExpressionType.Divide, "/" },
                    { ExpressionType.Modulo, "%" }
                };
            if (types.ContainsKey(nodeType))
                builder.Append(string.Format(" {0} ", types[nodeType]));
        }


        /// <summary>
        /// 转换UnaryExpression
        /// </summary>
        /// <param name="expression"></param>w
        /// <returns></returns>
        protected virtual string ConvertMemberExpression(MemberExpression expression)
        {
            var memberExpression = expression;
            while (true)
            {
                if (memberExpression==null || memberExpression.Expression == null)
                    break;
                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                {
                    if (!string.IsNullOrWhiteSpace(MethodExp))
                    {
                        var pname = expression.ToString().Split('.').FirstOrDefault();
                        if (!MethodExp.Contains($"{pname} =>"))
                            return $"_ParentObject.{ExpressionHelper.ReplaceExpressionParamter(expression.ToString())}";
                    }
                    return ExpressionHelper.ReplaceExpressionParamter(expression.ToString());
                }
                if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    memberExpression = memberExpression.Expression as MemberExpression;
                }
                else
                {
                    break;
                }
            }
            var name = CreateParamterName();
            SetParameter(name, expression.GetExpressionValue());
            return string.Format("@{0}", name);
        }

        /// <summary>
        /// 转换UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual string ConvertConstantExpression(ConstantExpression expression,Type type=null)
        {
            return expression.ConvertConstantExpression(Query, type);
        }
        /// <summary>
        /// 转换UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual string ConvertMethodCallExpression(MethodCallExpression expression)
        {

            switch (expression.Method.Name)
            {
                case "Contains":
                case "StartsWith":
                case "Equals":
                case "EndsWith":
                {
                    string propertyName = GetMethodCallExpressionName(expression.Object ?? expression.Arguments[0]);
                    string paramterName =
                        GetMethodCallExpressionName(expression.Arguments[expression.Arguments.Count - 1]);
                    if (expression.Method.Name.Equals("Equals"))
                        return string.Format("{0}=={1}", propertyName, paramterName);
                    return string.Format("{0}.{1}({2})", propertyName, expression.Method.Name, paramterName);
                }
                case "Substring":
                {
                    string propertyName = GetMethodCallExpressionName(expression.Object ?? expression.Arguments[0]);
                    string paramterName =
                        GetMethodCallExpressionName(expression.Arguments[0]);
                    string paramterName2 = expression.Arguments.Count > 1
                        ? GetMethodCallExpressionName(expression.Arguments[1])
                        : paramterName.Length.ToString();
                    return string.Format("{0}.{1}({2},{3})", propertyName, expression.Method.Name, paramterName,paramterName2);
                }
                case "IndexOf":
                {
                    string propertyName = GetMethodCallExpressionName(expression.Object ?? expression.Arguments[0]);
                    string paramterName =
                        GetMethodCallExpressionName(expression.Arguments[0]);
                    if (expression.Arguments.Count > 1)
                    {
                        string paramterName2 =
                            GetMethodCallExpressionName(expression.Arguments[1]);
                        return string.Format("{0}.{1}({2},{3})", propertyName, expression.Method.Name, paramterName,
                            paramterName2);
                    }
                    return string.Format("{0}.{1}({2})", propertyName, expression.Method.Name, paramterName);
                }
                case "IsNullOrEmpty":
                case "IsNullOrWhiteSpace":
                    string pName = GetMethodCallExpressionName(expression.Object ?? expression.Arguments[0]);
                    return string.Format("{0}.IsNullOrEmpty()",pName);
                case "Any":
                case "Count":
                case "All":
                case "Sum":
                case "Min":
                case "Max":
                case "Average":
                  
                    var builder = new StringBuilder();
                    if (expression.Arguments.Count > 1)
                    {
                        MethodExp = ((LambdaExpression) expression.Arguments[1]).ToString();
                        BuilderWhereExpression(((LambdaExpression)expression.Arguments[1]).Body, builder);
                        MethodExp = null;
                    }
                    if (expression.Method.Name.Equals("Any") || expression.Method.Name.Equals("Count") ||
                        expression.Method.Name.Equals("All"))
                    {
                        string name = GetMethodCallExpressionName(expression.Object ?? expression.Arguments[0]);
                        return string.Format("{0}.{1}({2})",name, expression.Method.Name, builder);
                    }
                    return string.Format("{0}({1})", expression.Method.Name, builder);
                default:
                    return GetInvokeValue(expression);
            }
        }
        
       

        /// <summary>
        /// 得到函数的参数名称
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual string GetMethodCallExpressionName(Expression expression)
        {
            if (expression is UnaryExpression)
            {
                var unaryExpression = expression as UnaryExpression;
                return unaryExpression.ConvertUnaryExpression();
            }
            if (expression is ConstantExpression || expression is MemberExpression)
            {
             
                var memberExpression = expression as MemberExpression;
                if (memberExpression != null)
                {
                    var tMemberExpression = memberExpression;
                    while (tMemberExpression!=null)
                    {
                        if (tMemberExpression.Expression is ParameterExpression)
                            return ExpressionHelper.ReplaceExpressionParamter(memberExpression.ToString());
                        tMemberExpression = tMemberExpression.Expression as MemberExpression;
                    }
                    var paramterName = CreateParamterName();
                    SetParameter(paramterName, memberExpression.GetExpressionValue());
                    return string.Format("@{0}", paramterName);
                }
                var constantExpression = expression as ConstantExpression;
                var pName = CreateParamterName();
                SetParameter(pName, constantExpression.Value);
                return string.Format("@{0}", pName);
            }
            if (expression.NodeType == ExpressionType.MemberAccess)
                return ExpressionHelper.ReplaceExpressionParamter(expression.ToString());
            return GetInvokeValue(expression);
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <returns></returns>
        public virtual string CreateParamterName()
        {
            return Query.CreateParameterName();
        }
        /// <summary>
        /// 设置默认
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual string GetInvokeValue(Expression expression)
        {
            var value = expression.GetExpressionValue();
            var pName = CreateParamterName();
            SetParameter(pName, value);
            return string.Format("@{0}", pName);
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual void SetParameter(string name, object value)
        {
            Query.SetParameter(name, value);
        }

        #endregion

    }
}
