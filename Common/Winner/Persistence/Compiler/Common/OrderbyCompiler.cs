using System.Text;
using System.Text.RegularExpressions;

namespace Winner.Persistence.Compiler.Common
{
    public class OrderbyCompiler : IOrderbyCompiler
    {
        /// <summary>
        /// 属性
        /// </summary>
        private const string PropertyPattern = @"\w+(\s*\.\s*\w+)*";
        /// <summary>
        /// 操作符匹配
        /// </summary>
        private const string OperatorKeyPattern = @"\+|\-|\*|\/|\%|\(|\)";
        /// <summary>
        /// 操作符匹配
        /// </summary>
        private const string OrderKeyPattern = @"(\s+(asc|desc))(\s*,|$)?";
        /// <summary>
        /// 正则表达式
        /// </summary>
        private const string Pattern = OperatorKeyPattern + "|" + PropertyPattern + "|" + OrderKeyPattern;

        #region 接口的实现

        /// <summary>
        /// 解析Orderby
        /// </summary>
        /// <param name="queryCompiler"></param>
        public virtual void Translate(QueryCompilerInfo queryCompiler)
        {
            queryCompiler.Builder = new StringBuilder();
            if (string.IsNullOrEmpty(queryCompiler.Exp))return ;
           var match = Regex.Match(queryCompiler.Exp, Pattern);
            while (match.Length > 0)
            {

                if (Regex.IsMatch(match.Value, OperatorKeyPattern) || Regex.IsMatch(match.Value, OrderKeyPattern))
                    queryCompiler.Builder.Append(match.Value);
                else
                    AppendPropertyName(queryCompiler,match);
                match = match.NextMatch();
            }
        }
     
        #endregion

        #region 方法
        /// <summary>
        /// 得到名称
        /// </summary>
        /// <param name="queryCompiler"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        protected virtual void AppendPropertyName(QueryCompilerInfo queryCompiler, Match match)
        {
            var m = Regex.Match(match.Value, PropertyPattern);
            var propertyName = m.Value.Trim();
            var chainProperties = queryCompiler.Object.GetChainProperties(propertyName);
            queryCompiler.AddJoins(chainProperties);
            var property = chainProperties[chainProperties.Count - 1];
            queryCompiler.Builder.Append(queryCompiler.GetFieldName(property, propertyName));
            m = m.NextMatch();
            if (m.Length > 0) queryCompiler.Builder.AppendFormat(" {0}", m.Value);
            if (match.Value.Contains(",")) queryCompiler.Builder.Append(",");
        }

        #endregion

    }
}
