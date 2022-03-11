using System.Text;
using System.Text.RegularExpressions;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Oracle
{
    public class OracleSelectCompiler : SelectCompiler
    {

        /// <summary>
        /// 
        /// </summary>
        public override string FeildBeforeTag { get; set; } = "\"";
        /// <summary>
        /// 
        /// </summary>
        public override string FeildAfterTag { get; set; } = "\"";
        protected override string GetManyFieldName(OrmPropertyInfo property, string propertName, QueryCompilerInfo selectComplier, QueryCompilerInfo subSelectComplier)
        {
            var name = propertName.Replace(string.Format(".{0}", property.PropertyName), "");
            var feildName = selectComplier.GetFieldName(property.Map.ObjectProperty, name);
            var tableName = subSelectComplier.GetJoinTable(property.Map);
            if (subSelectComplier.SubQuery != null)
            {
                subSelectComplier.Chainon = string.Format("{0}.{1}={2}", "{0}",
                    $"{FeildBeforeTag}{property.Map.MapObjectProperty.FieldName}{FeildAfterTag}", feildName);
            }
            if (subSelectComplier.Builder.Length > 1 && subSelectComplier.Builder[subSelectComplier.Builder.Length - 2] == ',' && subSelectComplier.Builder[subSelectComplier.Builder.Length - 1] == '\'')
                subSelectComplier.Builder.Remove(subSelectComplier.Builder.Length - 2, 1);
            var builder = new StringBuilder();
            builder.Append($"'\"{propertName}\":['||");
            builder.Append($"(select listagg(");
            builder.Append("'{'||");
            builder.Append(subSelectComplier.Builder.Replace("\",','", "\",'||'"));
            builder.Append("||'}'");
            builder.Append(
                $",',') within  group(ORDER BY {subSelectComplier.Table.AsName}.{FeildBeforeTag}{property.Map.GetMapObject().PrimaryProperty.FieldName}{FeildAfterTag})  from {tableName} where {subSelectComplier.Table.AsName}.{FeildBeforeTag}{property.Map.MapObjectProperty.FieldName}{FeildAfterTag}={feildName}{subSelectComplier.SubQueryJoinWhere}");
            builder.Append($" group by {subSelectComplier.Table.AsName}.{FeildBeforeTag}{property.Map.MapObjectProperty.FieldName}{FeildAfterTag})");
            builder.Append($"||']'");
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
                name = $"replace({name},'\\\"','\\\\\\\"')";
            return $"'\"{propertyName}\":'||'\"'||{name}||'\",'";
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
    }
}
