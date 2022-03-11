using System.Text;
using System.Text.RegularExpressions;

namespace Winner.Persistence.Compiler.Common
{
    public class GroupbyCompiler : IGroupbyCompiler
    {
        /// <summary>
        /// 属性
        /// </summary>
        private const string PropertyPattern = @"\w+(\s*\.\s*\w+)*";
        /// <summary>
        /// 操作符匹配
        /// </summary>
        private const string OperatorKeyPattern =  @"\+|\-|\*|\/|\%|\(|\)";

        /// <summary>
        /// 正则表达式
        /// </summary>
        private const string Pattern = OperatorKeyPattern + "|" + PropertyPattern + @"(\s*,|$)?";

        #region 接口的实现

        /// <summary>
        /// 解析Orderby
        /// </summary>
        /// <param name="queryCompiler"></param>
        public virtual void Translate(QueryCompilerInfo queryCompiler)
        {
            queryCompiler.Builder = new StringBuilder();
            if (string.IsNullOrEmpty(queryCompiler.Exp)) return ;
            var match = Regex.Match(queryCompiler.Exp, Pattern);
            while (match.Length > 0)
            {

                if (Regex.IsMatch(match.Value, OperatorKeyPattern))
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
        protected virtual void AppendPropertyName(QueryCompilerInfo queryCompiler,Match match)
        {
            var propertyName = match.Value.Trim(',').Trim();
            var chainProperties = queryCompiler.Object.GetChainProperties(propertyName);
            queryCompiler.AddJoins(chainProperties);
            var property = chainProperties[chainProperties.Count - 1];
            queryCompiler.Builder.Append(queryCompiler.GetFieldName(property, propertyName));
            if (match.Value.Contains(",")) queryCompiler.Builder.Append(",");
        }
        #endregion



    }
}
