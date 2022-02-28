using System;
using System.Linq.Expressions;

namespace Winner.Persistence.Linq
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// 得到变量值
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        static public object GetExpressionValue(this Expression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            var value= getter();
            if (expression.NodeType == ExpressionType.Convert && expression is UnaryExpression)
            {
                var type = ((UnaryExpression) expression).Operand.Type;
                if (type.IsEnum)
                    return Enum.Parse(type, value.ToString());
                return Convert.ChangeType(value, type);
            }
            return value;
        }
        /// <summary>
        /// 转换UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        static public string ConvertUnaryExpression(this UnaryExpression expression)
        {
            return ReplaceExpressionParamter(expression.Operand.ToString());
        }
        /// <summary>
        /// 替换参数
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        static public string ReplaceExpressionParamter(string expression)
        {
            var index = expression.IndexOf(".", StringComparison.Ordinal);
            var lastIndex = expression.IndexOf(",");
            if (lastIndex < 0)
                lastIndex = expression.IndexOf(")");
            if (lastIndex < 0)
                lastIndex = expression.Length;
            return expression.Substring(index + 1, lastIndex - index - 1);
        }

        /// <summary>
        /// 转换UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static public string ConvertConstantExpression(this ConstantExpression expression, ParameterInfo parameter, Type type)
        {
            if (expression.Value == null)
            {
                return "null";
            }
            var name = parameter.CreateParameterName();
            if(type!=null && type.IsEnum)
                parameter.SetParameter(name, Enum.Parse(type, expression.Value.ToString()));
            else
                parameter.SetParameter(name, expression.Value);
            
            return string.Format("@{0}", name);
        }
    }
}
