using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Relation;
using System.Linq;
namespace Winner.Persistence.Compiler.MySql
{
    public class MySqlSelectCompiler : SelectCompiler
    {
        /// <summary>
        /// 
        /// </summary>
        public override string FeildBeforeTag { get; set; } = "`";
        /// <summary>
        /// 
        /// </summary>
        public override string FeildAfterTag { get; set; } = "`";
        protected override string GetManyFieldName(OrmPropertyInfo property, string propertName, QueryCompilerInfo selectComplier, QueryCompilerInfo subSelectComplier)
        {
            bool isEnd = selectComplier.Builder.ToString().EndsWith($",']'),");
            var name = propertName.Replace(string.Format(".{0}", property.PropertyName), "");
            var feildName = selectComplier.GetFieldName(property.Map.ObjectProperty, name);
            var tableName = subSelectComplier.GetJoinTable(property.Map);
            if (subSelectComplier.SubQuery != null)
            {
                subSelectComplier.Chainon = string.Format("{0}.{1}={2}", "{0}",
                    $"{FeildBeforeTag}{property.Map.MapObjectProperty.FieldName}{FeildAfterTag}", feildName);
            }
            if(subSelectComplier.Builder.Length>1 && subSelectComplier.Builder[subSelectComplier.Builder.Length-2]==',' && subSelectComplier.Builder[subSelectComplier.Builder.Length - 1] == '\'')
                subSelectComplier.Builder.Remove(subSelectComplier.Builder.Length - 2,1);
            var builder = new StringBuilder();
            builder.Append($"concat('{(isEnd?",":"")}\"{propertName}\":[',");
            builder.Append($"(select");
            if (subSelectComplier.SubQuery != null && subSelectComplier.SubQuery.PageSize > 0)
            {
                var count = subSelectComplier.Exp.Count(s => s == ',')+1;
                //count = subSelectComplier.Builder.ToString().Count(s => s == ':');
                if (subSelectComplier.SubQuery.PageIndex == 0)
                {
                    builder.Append($" substring_index(");
                    builder.Append($" group_concat(concat_ws('',");
                    builder.Append("'{',");
                    builder.Append(subSelectComplier.Builder);
                    builder.Append(",'}'");
                    builder.Append($" ) {subSelectComplier.SubQuery.OrderByExp} )");
                    builder.Append($" ,',',{subSelectComplier.SubQuery.PageSize* count})");
                }
                else
                {
                    builder.Append($" substring_index(");
                    builder.Append($" substring_index(");
                    builder.Append($" group_concat(concat_ws('',");
                    builder.Append("'{',");
                    builder.Append(subSelectComplier.Builder);
                    builder.Append(",'}'");
                    builder.Append($" ) {subSelectComplier.SubQuery.OrderByExp} )");
                    builder.Append($" ,',',{subSelectComplier.SubQuery.PageSize*(subSelectComplier.SubQuery.PageIndex+1)*count})");
                    builder.Append($" ,',',{0-subSelectComplier.SubQuery.PageSize * count})");
                }
                   
            }
            else
            {
                builder.Append($" group_concat(concat_ws('',");
                builder.Append("'{',");
                builder.Append(subSelectComplier.Builder);
                builder.Append(",'}'");
                builder.Append($" ) separator ',')");
            }
          
            builder.Append($" from {tableName} where {subSelectComplier.Table.AsName}.{FeildBeforeTag}{property.Map.MapObjectProperty.FieldName}{FeildAfterTag}={feildName}{subSelectComplier.SubQueryJoinWhere}");
            builder.Append($" group by {subSelectComplier.Table.AsName}.{FeildBeforeTag}{property.Map.MapObjectProperty.FieldName}{FeildAfterTag})");
            builder.Append($",']')");
            return builder.ToString();
        }


        /// <summary>
        /// 得到查询字段名称
        /// </summary>
        /// <param name="queryCompiler"></param>
        /// <param name="property"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override string GetFieldName(QueryCompilerInfo queryCompiler, OrmPropertyInfo property, string propertyName)
        {
            if (queryCompiler.Parent == null)
                return base.GetFieldName(queryCompiler, property, propertyName);
            var name = base.GetFieldName(queryCompiler, property, propertyName);
            if (property.PropertyType == typeof(string))
                name = $"replace(replace({name},'\\\\','\\\\\\\\'),'\\\"','\\\\\"')";//$"replace({name},'\\\"','\\\\\\\"')";//'\\\\'在数据库里'\\'代表一个'\'
            else if (property.PropertyType == typeof(bool))
                name = $"case {name} when 0 then '0' else '1' end";
            return $"'\"{propertyName}\":','\"',{name},'\",'";
        }

        /// <summary>
        /// 转换属性到字段
        /// </summary>
        /// <param name="property"></param>
        /// <param name="queryCompiler"></param>
        /// <param name="propertyName"></param>
        protected override string GetAsName(OrmPropertyInfo property,
            QueryCompilerInfo queryCompiler, string propertyName)
        {
            if (queryCompiler.Parent == null)
                return base.GetAsName(property, queryCompiler, propertyName);
            if (!property.AllowRead) return null;
            var builder = new StringBuilder();
            builder.Append(GetFieldName(queryCompiler,property, propertyName));
            //builder.Append(" as ");
            //builder.AppendFormat("{0}_{1}", propertyName.Replace(".", "_"), queryCompiler.FieldCount);
            builder.Append(",");
            return builder.ToString();
        }



        /// <summary>
        /// 匹配as
        /// </summary>
        /// <param name="queryCompiler"></param>
        /// <param name="match"></param>
        /// <param name="propertyName"></param>
        protected override void AppendAsSql(QueryCompilerInfo queryCompiler, Match match, string propertyName)
        {
            if (queryCompiler.Parent == null)
            {
                base.AppendAsSql(queryCompiler, match, propertyName);
                return;
            }
            var asMatch = Regex.Match(match.Value, AsPattern);
            if (asMatch.Length > 0)
            {
                queryCompiler.Builder.Append(asMatch.Value);
                queryCompiler.Builder.AppendFormat("_{0}", queryCompiler.FieldCount);
                return;
            }
            //if (!string.IsNullOrEmpty(propertyName) && (Regex.IsMatch(match.Value, CommaPatten) || match.NextMatch().Length == 0))
            //    queryCompiler.Builder.AppendFormat(" as {0}_{1}", propertyName.Replace(".", "_"), queryCompiler.FieldCount);
            if (Regex.IsMatch(match.Value, CommaPatten)) queryCompiler.Builder.Append(",");
        }

        protected override void AppendRemoteQueryCompiler(QueryCompilerInfo queryCompiler, OrmPropertyInfo property, IList<OrmPropertyInfo> chainProperties)
        {
            if (queryCompiler.IsIgnoreAs==false && queryCompiler.Parent != null)
                queryCompiler.IsIgnoreAs = true;
            base.AppendRemoteQueryCompiler(queryCompiler, property, chainProperties);
        }
    }
}
