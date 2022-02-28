using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Winner.Persistence.Compiler.Analysis;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Common
{
    public class WhereCompiler : IWhereCompiler
    {
        #region 声明
        /// <summary>
        /// 
        /// </summary>
        public virtual string FeildBeforeTag { get; set; } = "[";
        /// <summary>
        /// 
        /// </summary>
        public virtual string FeildAfterTag { get; set; } = "]";

        protected delegate void MatchHandler(WhereCompilerInfo whereCompiler, Match match, string key);

        protected const string BreakersPattern =@"\([^\(\)]*(((?'Open'\()[^\(\)]*)+((?'-Open'\))[^\(\)]*)+)*(?(Open)(?!))\)";
        /// <summary>
        /// 数字
        /// </summary>
        protected const string NumberPattern = @"\d+";
        /// <summary>
        /// 函数
        /// </summary>
        protected const string MethodPattern = PropertyPattern + @"\s*\.{0}\s*" + BreakersPattern;

        /// <summary>
        /// 双引号
        /// </summary>
        protected const string QuotationPattern = "\"[^\"]+\\\"[^\\\"]*(((?'Open'\\\")[^\\\"]*)+((?'-Open'\\\")[^\\\"]*)+)*(?(Open)(?!))\\\"+\"|\"[^\"]+\"";
        /// <summary>
        /// 比较
        /// </summary>
        protected const string OperatorPattern = @"\<\s*\=|\>\s*\=|\!\s*\=|\=\s*\=|\>|\<|\-|\+|\/|\%|\*|\)|\(|\!|\&\&|\|\|";
        /// <summary>
        /// 连接
        /// </summary>
        protected const string ConnectorPattern = @"\(|\)|\&\&|\|\|";
        /// <summary>
        /// as
        /// </summary>
        protected const string PropertyPattern = @"@?\w+(\s*\.\s*\w+)*";
        /// <summary>
        /// 得到关键字
        /// </summary>
        protected const string KeyPattern = @"\.All\(|\.Any\(|\.Count\(|\.IsNullOrEmpty\(|\.Contains\(|\.StartsWith\(|\.EndsWith\(|\.Substring\(|\.IndexOf\(";

        /// <summary>
        /// 匹配
        /// </summary>
        protected static readonly string Pattern = string.Format(MethodPattern, "Count") + "|" +
                                                 string.Format(MethodPattern, "Any")
                                                 + "|" + string.Format(MethodPattern, "All") + "|" +
                                                 string.Format(MethodPattern, "IsNullOrEmpty")
                                                 + "|" + string.Format(MethodPattern, "Contains") + "|" +
                                                 string.Format(MethodPattern, "StartsWith")
                                                 + "|" + string.Format(MethodPattern, "EndsWith")
                                                 + @"|" + OperatorPattern + @"|"
                                                 + PropertyPattern;
        #endregion

        #region 接口的实现

        /// <summary>
        /// 解析条件
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <returns></returns>
        public virtual void Translate(WhereCompilerInfo whereCompiler)
        {
            whereCompiler.Builder = new StringBuilder();
            if (!string.IsNullOrEmpty(whereCompiler.Exp))
            {
                AppendSql(whereCompiler);
            }
            var builder = new StringBuilder();
            if (whereCompiler.IsSaveWhere)
            {
                var tableName = whereCompiler.SaveCompiler.SaveInfo.SetTableName;
                builder.Append("exists(select * from ");
                builder.Append(whereCompiler.GetJoinTable(whereCompiler.SaveCompiler.SaveInfo));
                builder.AppendFormat(" where {0}.{1}={2}"
                    ,tableName,whereCompiler.Object.PrimaryProperty.FieldName
                    ,GetFieldName(whereCompiler,whereCompiler.Object.PrimaryProperty,null));
                if (whereCompiler.Builder.Length > 0)
                {
                    builder.Append(" and ");
                    builder.Append(whereCompiler.Builder); 
                }
                builder.Append(")");
                whereCompiler.Builder = builder;
            }
        }
        #endregion

        #region 解析语句

        /// <summary>
        /// 替换引号
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <returns></returns>
        protected virtual void FillterQuotation(WhereCompilerInfo whereCompiler)
        {
            var builder = new StringBuilder();
            var match = Regex.Match(whereCompiler.Exp, QuotationPattern);
            whereCompiler.Query.SqlParameters = whereCompiler.Query.SqlParameters ?? (whereCompiler.Query.Parameters==null?new Dictionary<string, object>():new Dictionary<string, object>(whereCompiler.Query.Parameters));
            while (match.Length > 0)
            {
                var name = string.Format("P{0}", whereCompiler.Query.SqlParameters.Count + 1);
                whereCompiler.Query.SqlParameters.Add(name, match.Value);
                builder.Append(whereCompiler.Exp.Substring(0, match.Index));
                builder.AppendFormat("@{0}", name);
                builder.Append(whereCompiler.Exp.Substring(match.Index + match.Length + 1,
                    whereCompiler.Exp.Length - match.Index - match.Length - 1));
                match = match.NextMatch();
            }
            if(builder.Length>0) whereCompiler.Exp= builder.ToString();
        }

        /// <summary>
        /// 得到where语句
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <returns></returns>
        protected virtual void AppendSql(WhereCompilerInfo whereCompiler)
        {
            FillterQuotation(whereCompiler);
            var match = Regex.Match( whereCompiler.Exp,Pattern);
            while (match.Length > 0)
            {
                AppendWhereSql(whereCompiler, match);
                whereCompiler.PreviousMatch = match;
                match = match.NextMatch();
            }
        }


        /// <summary>
        /// 拼接SQL
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        protected virtual void AppendWhereSql(WhereCompilerInfo whereCompiler, Match match)
        {
            var keyMatch = Regex.Match(match.Value, KeyPattern);
            var key = keyMatch.Value.Replace(" ", "");
            var handler = GetMatchHandler(key);
            if (handler != null)
            {
                handler(whereCompiler, match, key);
                return;
            }
            var rev = AppendProperty(whereCompiler, match);
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
                {".IsNullOrEmpty(",ReplaceIsNullOrEmpty},{".Contains(",ReplaceContains},
                {".EndsWith(",ReplaceEndsWith},{".StartsWith(",ReplaceStartsWith},
                {".All(",ReplaceAll},{".Any(",ReplaceAny},{".Count(",ReplaceCount}
            };
            return handlers.ContainsKey(key) ? handlers[key] : null;
        }
     

        #endregion

        #region 解析操作符

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        protected virtual bool AppendProperty(WhereCompilerInfo whereCompiler, Match match)
        {
            if (!Regex.IsMatch(match.Value.Trim(), PropertyPattern)) return false;
            var key = match.Value.Trim();
            //if (key.StartsWith("_ParentObject.") && whereCompiler.Parent != null)
            //{
            //    key = key.Replace("_ParentObject.", "");
            //    var p = whereCompiler.Parent.Object.GetChainProperties(key).LastOrDefault();
            //    whereCompiler.Builder.Append(whereCompiler.Parent.GetFieldName(p, key));
            //    return true;
            //}
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
            if (key.Equals("null"))
            {
                whereCompiler.Builder.Append(match.Value);
                return true;
            }

            string fieldName;
            if (whereCompiler.Parent!=null && key.StartsWith("_ParentObject."))
            {
                key = key.Replace("_ParentObject.", "");
                var chainProperties = whereCompiler.Parent.Object.GetChainProperties(key);
                fieldName = GetFeildNameAndAddJoins(whereCompiler.Parent, chainProperties, key);
            }
            else
            {
                var chainProperties = whereCompiler.Object.GetChainProperties(key);
                fieldName = GetFeildNameAndAddJoins(whereCompiler, chainProperties, key);
            }
            if (
                (Regex.IsMatch(match.NextMatch().Value.Trim(), ConnectorPattern)
                || match.NextMatch().Value.Trim().Length==0)
                && (whereCompiler.PreviousMatch == null
                    || Regex.IsMatch(whereCompiler.PreviousMatch.Value.Trim(), ConnectorPattern)
                    || whereCompiler.PreviousMatch.Value.Trim().Equals("!"))
                )
            {
                if (whereCompiler.PreviousMatch != null && whereCompiler.PreviousMatch.Value.Trim().Equals("!"))
                {
                    whereCompiler.Builder.Remove(whereCompiler.Builder.Length - 4, 4);
                    fieldName = string.Format("{0}=0", fieldName);
                }
                else
                {
                      fieldName = string.Format("{0}=1", fieldName);
                }
            }
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
            if (operatorName.Equals("==") || operatorName.Equals("!="))
            {
                if (match.NextMatch().Value.Replace(" ", "").Equals("null"))
                {
                    whereCompiler.Builder.Append(operatorName.Equals("==") ? " is " : " is not ");
                    return;
                }
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
        protected virtual string GetFeildNameAndAddJoins(QueryCompilerInfo whereCompiler, IList<OrmPropertyInfo> chainProperties, string propertyName)
        {
            var property = chainProperties[chainProperties.Count - 1];
            if (whereCompiler.Table.Joins != null) whereCompiler.AddJoins(chainProperties);
            var fieldName = GetFieldName(whereCompiler,property, propertyName);
            return fieldName;
        }
        #endregion
 
        #region 得到一对多条件

        /// <summary>
        /// 转换一对多
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="isReturnCount"></param>
        /// <returns></returns>
        protected virtual WhereCompilerInfo TranslateManyWhere(WhereCompilerInfo whereCompiler, Match match, bool isReturnCount)
        {
            var lastIndex = Regex.Match(match.Value, PropertyPattern).Value.LastIndexOf('.');
            var propertyName = match.Value.Trim().Substring(0, lastIndex);
            var chainProperties = whereCompiler.Object.GetChainProperties(propertyName);
            return TranslateManyWhere(whereCompiler, match, propertyName, chainProperties, isReturnCount);
        }

        /// <summary>
        /// 转换一对多
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="propertyName"></param>
        /// <param name="chainProperties"></param>
        /// <param name="isReturnCount"></param>
        /// <returns></returns>
        protected virtual WhereCompilerInfo TranslateManyWhere(WhereCompilerInfo whereCompiler, Match match,
            string propertyName, IList<OrmPropertyInfo> chainProperties, bool isReturnCount)
        {
            var m = Regex.Match(match.Value, BreakersPattern);
            var subText = m.Value.Trim().Substring(1,m.Value.Length-2);
            if (whereCompiler.Table.Joins != null) whereCompiler.AddJoins(chainProperties);
            var subWhereCompiler = TranslateQueryManyWhere(whereCompiler, subText, chainProperties, propertyName, isReturnCount);                        
            return subWhereCompiler;
        }


        /// <summary>
        /// 翻译查询一对关系
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="subText"></param>
        /// <param name="chainProperties"></param>
        /// <param name="propertyName"></param>
        /// <param name="isReturnCount"></param>
        /// <returns></returns>
        protected virtual WhereCompilerInfo TranslateQueryManyWhere(WhereCompilerInfo whereCompiler,
                                                                    string subText,
                                                                    IList<OrmPropertyInfo> chainProperties,
                                                                    string propertyName, bool isReturnCount)
        {

            var property = chainProperties[chainProperties.Count - 1];
            return GetSubWhereCompiler(whereCompiler, property, subText, chainProperties, propertyName, isReturnCount);
        }

        /// <summary>
        /// 转换1对多条件
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="property"></param>
        /// <param name="subText"></param>
        /// <param name="chainProperties"></param>
        /// <param name="propertyName"></param>
        /// <param name="isReturnCount"></param>
        /// <returns></returns>
        protected virtual WhereCompilerInfo GetSubWhereCompiler(WhereCompilerInfo whereCompiler,
                                                                OrmPropertyInfo property,
                                                                string subText, IList<OrmPropertyInfo> chainProperties,
                                                                string propertyName, bool isReturnCount)
        {
            var subWhereCompiler = new WhereCompilerInfo(property.Map.GetMapObject(), subText,
                                                     new TableInfo { Joins = new Dictionary<string, JoinInfo>(), Tag = whereCompiler.Table.CreateSubTag() },
                                                         new StringBuilder(), whereCompiler.FeildBeforeTag, whereCompiler.FeildAfterTag) { Query =whereCompiler.Query,Parent=whereCompiler};
            Translate(subWhereCompiler);
            var lastIndex = propertyName.LastIndexOf('.');
            if (lastIndex > -1) propertyName = propertyName.Substring(0, lastIndex);
            if (subWhereCompiler.Builder.Length > 0)
            {
                subWhereCompiler.Builder.Insert(0,"(");
                subWhereCompiler.Builder.Append(") and ");
            } 
            subWhereCompiler.Builder.Insert(0, " where ");
            if (isReturnCount)
            {
                subWhereCompiler.Builder.Insert(0, string.Format("(select count(1) from {0}  "
                    , subWhereCompiler.GetJoinTable(property.Map)));
            }
            else
            {
                subWhereCompiler.Builder.Insert(0, string.Format("{0} in (select {1} from {2}  "
                    , GetFieldName(whereCompiler, property.Map.ObjectProperty, propertyName),
                    subWhereCompiler.GetFieldName(property.Map.MapObjectProperty, null),
                subWhereCompiler.GetJoinTable(property.Map)));
            }
            subWhereCompiler.Builder.AppendFormat("{0}={1})"
                                                          , GetFieldName(whereCompiler,property.Map.ObjectProperty, propertyName)
                                                          , subWhereCompiler.GetFieldName(property.Map.MapObjectProperty,null));
            return subWhereCompiler;
        }


        #endregion

        #region 解析函数

   

        /// <summary>
        /// 解析Count函数
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected virtual void ReplaceCount(WhereCompilerInfo whereCompiler, Match match, string key)
        {
            var subWhere = TranslateManyWhere(whereCompiler, match,true);
            whereCompiler.Builder.Append(subWhere.Builder);
        }
        /// <summary>
        /// 解析Any函数
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected virtual void ReplaceAny(WhereCompilerInfo whereCompiler, Match match, string key)
        {
            var subWhere = TranslateManyWhere(whereCompiler, match,false);
            whereCompiler.Builder.Append(subWhere.Builder);
        }
        /// <summary>
        /// 解析All函数
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected virtual void ReplaceAll(WhereCompilerInfo whereCompiler, Match match, string key)
        {
            var lastIndex = Regex.Match(match.Value, PropertyPattern).Value.LastIndexOf('.');
            var propertyName = match.Value.Trim().Substring(0, lastIndex);
            var chainProperties = whereCompiler.Object.GetChainProperties(propertyName);
            var property = chainProperties[chainProperties.Count - 1];
            var subWhere = TranslateManyWhere(whereCompiler, match, propertyName, chainProperties,true);
            string mapTableName = whereCompiler.Table.Joins != null ? whereCompiler.Query.GetGetTableName(property.Map)
                :whereCompiler.SaveCompiler.SaveInfo.GetSetTableName(property.Map.GetMapObject());
            var allWhere = string.Format("=(select count(1) from {0} _AllCountTable where _AllCountTable.{1}={2})"
                    , mapTableName, property.Map.MapObjectProperty.FieldName
                    , GetFieldName(whereCompiler,property.Map.ObjectProperty, propertyName));
            whereCompiler.Builder.Append(subWhere.Builder);
            whereCompiler.Builder.Append(allWhere);
        }

        /// <summary>
        /// 替换Contains关键字
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual void ReplaceContains(WhereCompilerInfo whereCompiler, Match match, string key)
        {
            var m = Regex.Match(match.Value, PropertyPattern);
            var chainProperties = new List<OrmPropertyInfo>();
            var lastIndex = m.Value.LastIndexOf('.');
            var propertyName = match.Value.Trim().Substring(0, lastIndex);
            string firstName = GetStringMothedName(whereCompiler, propertyName, chainProperties);
            m = Regex.Match(match.Value, BreakersPattern);
            string lastName = GetStringMothedName(whereCompiler, m.Value.Trim().Trim('(').Trim(')'), chainProperties);
            if (ContainsArray(whereCompiler, firstName, lastName)) return;
            if (chainProperties.Count > 0 && chainProperties[0].IsCustom)
            {
                propertyName = firstName.Contains("@SysParameter0") ? firstName : lastName;
                var paramterName = firstName.Contains("@SysParameter0") ? lastName : firstName;
                whereCompiler.Builder.AppendFormat(propertyName.Replace("@SysParameter0", paramterName) );
            }
            else
            {
                AppendContains(whereCompiler, match, chainProperties, firstName, lastName);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="chainProperties"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        protected virtual void AppendContains(WhereCompilerInfo whereCompiler, Match match, List<OrmPropertyInfo> chainProperties,string firstName,string lastName)
        {
            if (firstName.StartsWith("@"))
            {
                AppendLike(whereCompiler, match, chainProperties, firstName, lastName);
                return;
            }
            var property = chainProperties.Last();
            if((property.SearchType & (int)OrmSearchType.Search)>0)
            {
                if (AppendSearch(whereCompiler, match, chainProperties, firstName, lastName))
                    return;
            }
            if ((property.SearchType & (int)OrmSearchType.FullText) > 0)
            {
                if (AppendFullText(whereCompiler, match, chainProperties, firstName, lastName))
                    return;
            }
            if (property.SearchType==0 || (property.SearchType & (int)OrmSearchType.Like) > 0)
            {
                if (AppendLike(whereCompiler, match, chainProperties, firstName, lastName))
                    return;
            }
            whereCompiler.Builder.AppendFormat("1!=1");
        }
        /// <summary>
        /// 自定义分词
        /// </summary>
        protected virtual bool AppendLike(WhereCompilerInfo whereCompiler, Match match, List<OrmPropertyInfo> chainProperties, string firstName, string lastName)
        {
            whereCompiler.Builder.AppendFormat("({0} like {1} +'%' or {0} like '%'+{1} +'%' or {0} like '%'+{1})",
                firstName, lastName);
            return true;
        }
        /// <summary>
        /// 自定义分词
        /// </summary>
        protected virtual bool AppendFullText(WhereCompilerInfo whereCompiler, Match match, List<OrmPropertyInfo> chainProperties, string firstName, string lastName)
        {
            return false;
        }
        /// <summary>
        /// 自定义分词
        /// </summary>
        protected virtual bool AppendSearch(WhereCompilerInfo whereCompiler, Match match, List<OrmPropertyInfo> chainProperties, string firstName, string lastName)
        {
            var property = chainProperties.LastOrDefault();
            var obj = property?.GetObject();
            if (obj == null || string.IsNullOrWhiteSpace(obj.AnalyzerName) || property.SearchTableCount == 0 ||
                firstName.Contains("@"))
            {
                return false;
            }
            var pname = lastName.Replace("@", "");
            var value = whereCompiler.Query.Parameters == null || !whereCompiler.Query.Parameters.ContainsKey(pname) ? null : whereCompiler.Query.Parameters[pname]?.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            var words = Creator.Get<IAnalyzer>().Resolve(property, value);
            if (words == null || words.Count == 0)
            {
                return false;
            }
            var builder=new StringBuilder();
            var i = 0;
            foreach(var word in words)
            {
                var tableName = word.Name.GetSearchTableName(obj, property);
                builder.Append(string.Format(" {0} in (select Id from {1} where {2}Id{3}={0} and Name='{4}') "
                    , whereCompiler.GetFieldName(obj.PrimaryProperty, null), tableName,FeildBeforeTag,FeildAfterTag,word.Name));
                if(i!=words.Count-1)
                    builder.Append("or");
                i++;
            }
            builder.Insert(0,"(");
            builder.Append(")");
            whereCompiler.Builder.Append(builder);
            return true;
        }
       
        /// <summary>
        /// 匹配开始
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected virtual void ReplaceStartsWith(WhereCompilerInfo whereCompiler, Match match, string key)
        {
            ReplaceStringMothed(whereCompiler, match, "{0} like {1} +'%'");
        }
        /// <summary>
        /// 数组Contains方法
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        protected virtual bool ContainsArray(WhereCompilerInfo whereCompiler, string firstName, string lastName)
        {
            if (!firstName.Contains("@")) return false;
            var pName = firstName.Replace("@", "");
            if (whereCompiler.Query.SqlParameters.ContainsKey(pName) && whereCompiler.Query.SqlParameters[pName] == null)
            {
                whereCompiler.Builder.AppendFormat("{0} is null", lastName);
                return true;
            }
            if (whereCompiler.Query.SqlParameters.ContainsKey(pName) && whereCompiler.Query.SqlParameters[pName] is Array)
            {
                var array = whereCompiler.Query.SqlParameters[pName] as Array;
                if (array.Length == 0)
                {
                    whereCompiler.Builder.AppendFormat("{0} is null", lastName);
                    return true;
                }
                whereCompiler.Builder.AppendFormat("{0} in (", lastName);
                for (int i = 0; i < array.Length; i++)
                {
                    var name = string.Format("{0}_{1}", pName, i);
                    whereCompiler.Query.SqlParameters.Add(name, array.GetValue(i));
                    whereCompiler.Builder.AppendFormat("@{0}", name);
                    if (i != array.Length - 1) whereCompiler.Builder.Append(",");
                }
                whereCompiler.Query.SqlParameters.Remove(pName);
                whereCompiler.Builder.Append(")");
                return true;
            }
            return false;
        }

    

        /// <summary>
        /// 匹配结束
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected virtual void ReplaceEndsWith(WhereCompilerInfo whereCompiler, Match match, string key)
        {
            ReplaceStringMothed(whereCompiler, match, "{0} like '%'+ {1}");
         
        }

        /// <summary>
        /// 替换IsNullOrEmpty函数
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected virtual void ReplaceIsNullOrEmpty(WhereCompilerInfo whereCompiler, Match match, string key)
        {
             ReplaceStringMothed(whereCompiler, match, "({0} is null or {0}='')");
        }
        /// <summary>
        /// 替换字符函数
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="whereString"></param>
        protected virtual void ReplaceStringMothed(WhereCompilerInfo whereCompiler, Match match, string whereString)
        {
            var m = Regex.Match(match.Value, PropertyPattern);
            var chainProperties = new List<OrmPropertyInfo>();
            var lastIndex = m.Value.LastIndexOf('.');
            var propertyName = match.Value.Trim().Substring(0, lastIndex);
            string firstName = GetStringMothedName(whereCompiler, propertyName, chainProperties);
            m = Regex.Match(match.Value, BreakersPattern);
            string lastName = GetStringMothedName(whereCompiler, m.Value.Trim().Trim('(').Trim(')'), chainProperties);
            whereCompiler.Builder.AppendFormat(whereString, firstName, lastName);
        }

        /// <summary>
        /// 得到比较操作名称
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="name"></param>
        /// <param name="chainProperties"></param>
        /// <returns></returns>
        protected virtual string GetStringMothedName(WhereCompilerInfo whereCompiler, string name,
                                                     List<OrmPropertyInfo> chainProperties)
        {
            if (string.IsNullOrEmpty(name) || name.Equals("null")) return name;
            if (name.Contains("@")) return name;
            chainProperties.AddRange(whereCompiler.Object.GetChainProperties(name));
            return GetFeildNameAndAddJoins(whereCompiler, chainProperties, name);
        }

        #endregion

        /// <summary>
        /// 得到查询字段名称
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="property"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual string GetFieldName(QueryCompilerInfo whereCompiler, OrmPropertyInfo property, string propertyName)
        {
            return whereCompiler.GetFieldName(property, propertyName);

        }

    }
}
