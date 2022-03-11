using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Winner.Persistence.Relation;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Compiler.Common
{
    public class QueryCompilerInfo
    {
        public Action<OrmObjectInfo,QueryInfo, QueryCompilerInfo,bool> TranslateQuery { get; set; }
        /// <summary>
        /// 父级
        /// </summary>
        public QueryCompilerInfo Parent { get; set; }
        /// <summary>
        /// 映射属性
        /// </summary>
        public string RemotePropertyName { get; set; }
        /// <summary>
        /// 子查询嵌套查询限制
        /// </summary>
        public string SubQueryJoinWhere { get; set; }
        /// <summary>
        /// 外链
        /// </summary>
        public string Chainon { get; set; }
        /// <summary>
        /// 查询
        /// </summary>
        public QueryInfo Query { get; set; }
        /// <summary>
        /// 查询
        /// </summary>
        public QueryInfo SubQuery { get; set; }
        /// <summary>
        /// 对象
        /// </summary>
        public OrmObjectInfo Object { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TableInfo Table { get; set; }
        /// <summary>
        /// 解析内容
        /// </summary>
        public string Exp { get; set; }

        /// <summary>
        /// 拼接
        /// </summary>
        public StringBuilder Builder { get; set; }
        /// <summary>
        /// 字段数量
        /// </summary>
        public int FieldCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FeildBeforeTag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FeildAfterTag { get; set; }
        /// <summary>
        /// 是否忽略As
        /// </summary>
        public bool IsIgnoreAs { get; set; }

        public QueryCompilerInfo(OrmObjectInfo obj, string exp,TableInfo table, StringBuilder builder,string feildBeforeTag, string feildAfterTag)
        {
            Object = obj;
            Exp = exp;
            Table = table;
            Builder = builder;
            FeildBeforeTag = feildBeforeTag;
            FeildAfterTag = feildAfterTag;
        }
 
   

        /// <summary>
        /// 得到别名
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual string GetAsName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return Table.AsName;
            if (Table.Joins.ContainsKey(propertyName)) return Table.Joins[propertyName].AsName;
            return Table.AsName;
        }

        /// <summary>
        /// 得到查询字段名称
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual string GetFieldName(OrmPropertyInfo property, string propertyName)
        {
            FieldCount++;
            if (property.IsCustom)
            {
                var lastIndex = propertyName.LastIndexOf('.');
                var name = lastIndex < 0 ? propertyName : propertyName.Substring(0, lastIndex);
                return string.Format(property.FieldName, GetAsName(name));
            }
            return string.Format("{0}.{1}", GetAsName(string.IsNullOrEmpty(propertyName)?propertyName:
                propertyName.Replace(string.Format(".{0}", property.PropertyName),"")), $"{FeildBeforeTag}{property.FieldName}{FeildAfterTag}");
        }

        /// <summary>
        /// 添加连接表
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyName"></param>
        public virtual void AddJoin(OrmPropertyInfo property, string propertyName)
        {
            var index = propertyName.LastIndexOf('.');
            var lastName =propertyName.Substring(0,index) ;
            if (!Table.Joins.ContainsKey(propertyName))
            {
                Table.Joins.Add(propertyName, new JoinInfo
                    {
                        AsFieldName = $"{FeildBeforeTag}{property.Map.MapObjectProperty.FieldName}{FeildAfterTag}",
                        AsName = Table.CreateAsName(),
                        JoinName = GetAsName(lastName),
                        JoinFieldName = $"{FeildBeforeTag}{property.Map.ObjectProperty.FieldName}{FeildAfterTag}",
                        Object = property.Map.GetMapObject(),
                        Map= property.Map
                });
            }
        }

        /// <summary>
        /// 添加连接表
        /// </summary>
        /// <param name="chainProperties"></param>
        public virtual void AddJoins(IList<OrmPropertyInfo> chainProperties)
        {
            var lastName = "";
            var currentName = "";
            foreach (var chainProperty in chainProperties)
            {
                currentName = string.IsNullOrEmpty(currentName)
                                  ? chainProperty.PropertyName
                                  : string.Format("{0}.{1}", currentName, chainProperty.PropertyName);
                if (chainProperty.Map==null || chainProperty.Map.MapType==OrmMapType.OneToMany || chainProperty.Map.CheckRemote())
                    break;
                if (!Table.Joins.ContainsKey(currentName))
                {
                    Table.Joins.Add(currentName, new JoinInfo
                        {
                            AsFieldName = $"{FeildBeforeTag}{chainProperty.Map.MapObjectProperty.FieldName}{FeildAfterTag}",
                            AsName = Table.CreateAsName(),
                            JoinName = GetAsName(lastName),
                            JoinFieldName = $"{FeildBeforeTag}{chainProperty.Map.ObjectProperty.FieldName}{FeildAfterTag}",
                            Object = chainProperty.Map.GetMapObject(),
                            Map=chainProperty.Map
                    });
                }
                lastName = currentName;
            }
        }

        /// <summary>
        /// 得到连接表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual string GetJoinTable( QueryInfo query)
        {
            var builder = new StringBuilder();
            var getTableName = $"{FeildBeforeTag}{query.GetGetTableName(Object)}{FeildAfterTag}";
            if (!string.IsNullOrEmpty(Object.Mark))
                getTableName = string.Format("(select * from {0} where {1})", getTableName, $"{FeildBeforeTag}{Object.Mark}{FeildAfterTag}>0");
            builder.AppendFormat("{0} {1}", getTableName, Table.AsName);
            BuilderJoinTable(builder);
            return builder.ToString();
        }

        /// <summary>
        /// 得到连接表
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public virtual string GetJoinTable(SaveInfo save)
        {
            var builder = new StringBuilder();
            var setTableName = $"{FeildBeforeTag}{save.SetTableName}{FeildAfterTag}";
            if (!string.IsNullOrEmpty(Object.Mark))
                setTableName = string.Format("(select * from {0} where {1})", setTableName, $"{FeildBeforeTag}{Object.Mark}{FeildAfterTag}>0");
            builder.AppendFormat("{0} {1}", setTableName, Table.AsName);
            BuilderJoinTable(builder);
            return builder.ToString();
        }

        /// <summary>
        /// 得到连接表
        /// </summary>
        /// <returns></returns>
        public virtual string GetJoinTable(OrmMapInfo map)
        {
            //var map = string.IsNullOrWhiteSpace(RemotePropertyName)
            //    ? null
            //    : Query.Object?.GetChainProperties(RemotePropertyName)?.LastOrDefault()?.Map;
            var builder = new StringBuilder();
            Match match=null;
            if (SubQuery == null)
            {
                var tableName = map == null ? Query.GetGetTableName(Object) : Query.GetGetTableName(map);
                tableName = $"{FeildBeforeTag}{tableName}{FeildAfterTag}";
                if (string.IsNullOrEmpty(Object.Mark))
                    builder.Append(tableName);
                else
                    builder.AppendFormat("(select * from {0} where {1})",
                        tableName,
                        $"{FeildBeforeTag}{Object.Mark}{FeildAfterTag}>0");
            }
            else
            {
                match =string.IsNullOrWhiteSpace(SubQuery.WhereExp)?null: Regex.Match(SubQuery.WhereExp, @"\s*(.*)_ParentObject\.(.)*(\s|$)");
                if (match?.Length > 0)
                    SubQuery.WhereExp = Regex.Replace(SubQuery.WhereExp, @"((\w|\.)*\s*==\s*(_ParentObject)(\w|\.)*)|((_ParentObject)(\w|\.)*\s*==\s*(\w|\.)*)","1==1");
                if (map != null && map.IsMapTableAutoSharding)
                {
                    SubQuery.TableIndex = Query.TableIndex;
                }
                TranslateQuery(Object, SubQuery, this,true);
                if (SubQuery.SqlParameters != null)
                {
                    Query.SqlParameters = Query.SqlParameters ?? new Dictionary<string, object>(Query.Parameters);


                    foreach (var sqlParameter in SubQuery.SqlParameters)
                    {
                        if (!Query.SqlParameters.ContainsKey(sqlParameter.Key))
                            Query.SqlParameters.Add(sqlParameter.Key, sqlParameter.Value);
                    }
                    if (SubQuery.Parameters != null)
                    {
                        foreach (var parameter in SubQuery.Parameters)
                        {
                            if (!SubQuery.SqlParameters.ContainsKey(parameter.Key) &&
                                Query.Parameters.ContainsKey(parameter.Key))
                                Query.Parameters.Remove(parameter.Key);
                        }
                    }
                }
                //SubQuery.Sql = Regex.Replace(SubQuery.Sql, @" as (\w|\""|`|\[|\])*,", ",");
                builder.AppendFormat("({0})", SubQuery.Sql);
            }
            builder.AppendFormat(" {0}", Table.AsName);
            BuilderJoinTable(builder);
            if(match!=null && match.Length>0)
            {
                var parentMatch = Regex.Match(match.Value, @"(\w|\.|\=|\s)*_ParentObject\.(\w|\.|\=|\s|\(|\))*");
                if(parentMatch.Success)
                {
                    var values = parentMatch.Value.Replace("(", "").Replace(" ", "").Replace(")", "").Replace("==", "=").Split('=');
                    if (values.Length == 2)
                    {
                        string p0 = null;
                        string p1 = null;
                        if (values[0].Contains("_ParentObject."))
                        {
                            values[0] = values[0].Replace("_ParentObject.", "");
                            p0 = Parent.GetFieldName(Parent.Object.GetChainProperties(values[0]).LastOrDefault(), values[0]);

                        }
                        else
                        {
                            p0 = GetFieldName(Object.GetChainProperties(values[0]).LastOrDefault(), values[0]);
                        }
                        if (values[1].Contains("_ParentObject."))
                        {
                            values[1] = values[1].Replace("_ParentObject.", "");
                            p1 = Parent.GetFieldName(Parent.Object.GetChainProperties(values[1]).LastOrDefault(), values[1]);

                        }
                        else
                        {
                            p1 = GetFieldName(Object.GetChainProperties(values[1]).LastOrDefault(), values[1]);
                        }
                        SubQueryJoinWhere = string.Format(" and {0}={1}", p0, p1);

                    }
                }
             
            }
            return builder.ToString();
        }



        /// <summary>
        /// 拼接表
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void BuilderJoinTable(StringBuilder builder)
        {
            foreach (var join in Table.Joins)
            {
                builder.AppendFormat(" left join {0} {1} on {1}.{2}={3}.{4}",
                                            GetJoinTableName(join.Value), join.Value.AsName,
                                             join.Value.AsFieldName,
                                             join.Value.JoinName, join.Value.JoinFieldName);
            }
        }
        /// <summary>
        /// 得到关联表名称
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        protected virtual string GetJoinTableName(JoinInfo join)
        {
            if (string.IsNullOrEmpty(join.Object.Mark)) return $"{FeildBeforeTag}{Query.GetGetTableName(join)}{FeildAfterTag}" ;
            return string.Format("(select * from {0} where {1})", $"{FeildBeforeTag}{ Query.GetGetTableName(join)}{FeildAfterTag}", $"{FeildBeforeTag}{join.Object.Mark}{FeildAfterTag}>0");
        }
        /// <summary>
        /// 得到默认条件
        /// </summary>
        /// <param name="isSave"></param>
        /// <returns></returns>
        public virtual string GetDefaultWhere(bool isSave)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Object.Mark))
            {
                builder.AppendFormat("{0}.{1}", Table.AsName, $"{FeildBeforeTag}{Object.Mark}{FeildAfterTag}>0");
            }
            if (Table.Joins == null) return builder.ToString();
            foreach (var join in Table.Joins)
            {

                if (string.IsNullOrEmpty(join.Value.Object.Mark)) continue;
                builder.Append(" and ");
                builder.AppendFormat("{0}.{1}", join.Value.AsName, $"{FeildBeforeTag}{join.Value.Object.Mark}{FeildAfterTag}>0");
            }
            if (builder.Length > 0)
            {
                builder.Insert(0, "(");
                builder.Append(")");
            }
            return builder.ToString();
        }
    }
}
