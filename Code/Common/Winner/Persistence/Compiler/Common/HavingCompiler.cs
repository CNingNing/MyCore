using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Common
{
    public class HavingCompiler : IHavingCompiler
    {
        #region 声明
        protected delegate void MatchHandler(WhereCompilerInfo havingCompiler, Match match, string key);
        private const string BreakersPattern = @"\([^\(\)]*(((?'Open'\()[^\(\)]*)+((?'-Open'\))[^\(\)]*)+)*(?(Open)(?!))\)";
        /// <summary>
        /// 数字
        /// </summary>
        private const string NumberPattern = @"\d+";
        /// <summary>
        /// as
        /// </summary>
        private const string PropertyPattern = @"@?\w+(\s*\.\s*\w+)*";
        /// <summary>
        /// 函数
        /// </summary>
        private const string MethodPattern = @"{0}\s*" + BreakersPattern;
        /// <summary>
        /// 得到关键字
        /// </summary>
        private const string KeyPattern = @"Count\s*\(|Sum\s*\(|Min\s*\(|Max\s*\(|Average\s*\(";
        /// <summary>
        /// 比较
        /// </summary>
        private const string OperatorPattern = @"\<\s*\=|\>\s*\=|\!\s*\=|\=\s*\=|\>|\<|\-|\+|\/|\%|\*|\)|\(|\!|\&\&|\|\|";
        /// <summary>
        /// 比较
        /// </summary>
        private const string ConnectorPattern = @"\)|\(|\!|\&\&|\|\|";

        /// <summary>
        /// 匹配
        /// </summary>
        private static readonly string Pattern = string.Format(MethodPattern, "Count") + "|" +
                                                 string.Format(MethodPattern, "Sum")
                                                 + "|" + string.Format(MethodPattern, "Min") + "|" +
                                                 string.Format(MethodPattern, "Max")
                                                 + "|" + string.Format(MethodPattern, "Average")
                                                 + @"|" + OperatorPattern + @"|"
                                                 + PropertyPattern;
        #endregion

        #region 接口的实现

        /// <summary>
        /// 解析Having
        /// </summary>
        /// <param name="havingCompiler"></param>
        /// <returns></returns>
        public virtual void Translate(WhereCompilerInfo havingCompiler)
        {
            havingCompiler.Builder=new StringBuilder();
            Translate(havingCompiler, havingCompiler.Exp);
        }
        #endregion

        #region 根据属性得到字段名称

        /// <summary>
        /// 解析Having
        /// </summary>
        /// <param name="havingCompiler"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual void Translate(WhereCompilerInfo havingCompiler, string exp)
        {
            if (string.IsNullOrEmpty(exp)) return;
            var match = Regex.Match(exp, Pattern);
            while (match.Length > 0)
            {
                AppendHavingSql(havingCompiler, match);
                havingCompiler.PreviousMatch = match;
                match = match.NextMatch();
            }
        }

        #endregion
      


        /// <summary>
        /// 拼接SQL
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        protected virtual void AppendHavingSql(WhereCompilerInfo whereCompiler, Match match)
        {
            var keyMatch = Regex.Match(match.Value, KeyPattern);
            var key = keyMatch.Value.Replace(" ", "");
            var handler = GetMatchHandler(key);
            if (handler != null)
            {
                handler(whereCompiler, match, key);
                return;
            }
            var rev =  AppendProperty(whereCompiler, match);
            if (!rev) AppendOperator(whereCompiler, match);
        }

        /// <summary>
        /// 得到匹配实例
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual MatchHandler GetMatchHandler(string key)
        {
            IDictionary<string, MatchHandler> handlers = new Dictionary<string, MatchHandler>
                {
                {"Count(",AppendHavingMethod},{"Sum(",AppendHavingMethod},
                {"Max(",AppendHavingMethod},{"Min(",AppendHavingMethod},
                {"Average(",AppendHavingMethod}
           };
            return handlers.ContainsKey(key) ? handlers[key] : null;
        }

        #region 解析操作符

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        protected virtual bool AppendProperty(WhereCompilerInfo whereCompiler, Match match)
        {
            if (!Regex.IsMatch(match.Value, PropertyPattern)) return false;
            var key = match.Value.Trim();
            if (key.Contains("@"))
            {
                whereCompiler.Builder.Append(match.Value);
                return true;
            }
            if (Regex.IsMatch(match.Value, NumberPattern))
            {
                whereCompiler.Builder.Append(match.Value);
                return true;
            }
            var chainProperties = whereCompiler.Object.GetChainProperties(key);
            var fieldName = GetFeildNameAndAddJoins(whereCompiler, chainProperties, key);
            fieldName = Regex.IsMatch(match.NextMatch().Value.Trim(), ConnectorPattern)
                && (whereCompiler.PreviousMatch==null || Regex.IsMatch(whereCompiler.PreviousMatch.Value.Trim(), ConnectorPattern))
                            ? string.Format("{0}=1", fieldName)
                            : fieldName;
            whereCompiler.Builder.Append(fieldName);
            return true;
        }

        /// <summary>
        /// 替换操作符
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        protected virtual void AppendOperator(WhereCompilerInfo whereCompiler, Match match)
        {
            string operatorName = match.Value.Trim().Replace(" ", "");
            switch (operatorName)
            {
                case "&&": whereCompiler.Builder.Append(" and "); return;
                case "||": whereCompiler.Builder.Append(" or "); return;
                case "!": whereCompiler.Builder.Append("not "); return;
            }
            if (operatorName.Equals("==")) operatorName = "=";
            whereCompiler.Builder.Append(operatorName);
        }
        /// <summary>
        /// 设置查询的条件
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="chainProperties"></param>
        /// <param name="propertyName"></param>
        protected virtual string GetFeildNameAndAddJoins(WhereCompilerInfo whereCompiler, IList<OrmPropertyInfo> chainProperties, string propertyName)
        {
            var property = chainProperties[chainProperties.Count - 1];
            if (whereCompiler.Table.Joins != null) whereCompiler.AddJoins(chainProperties);
            var fieldName = whereCompiler.GetFieldName(property, propertyName);
            return fieldName;
        }
        #endregion

        #region 解析函数


        /// <summary>
        /// 添加函数
        /// </summary>
        /// <param name="havingCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected virtual void AppendHavingMethod(WhereCompilerInfo havingCompiler, Match match, string key)
        {
            var m = Regex.Match(match.Value, BreakersPattern);
            if (!m.Success) return ;
            var propertyName = m.Value.Trim('(').Trim(')');
            var chainProperties = havingCompiler.Object.GetChainProperties(propertyName);
            havingCompiler.AddJoins(chainProperties);
            havingCompiler.Builder.Append(key.Equals("Average") ? "Avg" : key);
            if (key.Equals("Count"))
                havingCompiler.Builder.Append("1)");
            else
                Translate(havingCompiler, string.Format("{0})", propertyName));
           
        }

        #endregion
   
    }
}
