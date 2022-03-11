using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Declare;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.MySql
{
    public class MySqlQueryCompiler : QueryCompiler
    {

        /// <summary>
        /// 查询实例
        /// </summary>
        public override ISelectCompiler SelectCompiler { get; set; } = new MySqlSelectCompiler();
        /// <summary>
        /// 查询实例
        /// </summary>
        public override IWhereCompiler WhereCompiler { get; set; } = new MySqlWhereCompiler();
        /// <summary>
        /// 
        /// </summary>
        public override string FeildBeforeTag { get; set; } = "`";
        /// <summary>
        /// 
        /// </summary>
        public override string FeildAfterTag { get; set; } = "`";
 
        /// <summary>
        /// 得到Sql语句
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="selectComplier"></param>
        /// <param name="tableSql"></param>
        /// <param name="whereComplier"></param>
        /// <param name="havingComplier"></param>
        /// <param name="orderbyComplier"></param>
        /// <param name="groupbyComplier"></param>
        /// <param name="queryCompiler"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override string GetSql(OrmObjectInfo obj, QueryCompilerInfo selectComplier, string tableSql,
                                        WhereCompilerInfo whereComplier, QueryCompilerInfo groupbyComplier,
                                        QueryCompilerInfo orderbyComplier, WhereCompilerInfo havingComplier,
            QueryCompilerInfo queryCompiler,QueryInfo query)
        {
            var sql = new StringBuilder();
            var orderbyExp = query.PageSize != 0
                                 ? GetDefaultOrderby(obj, query, orderbyComplier)
                                 : orderbyComplier.Builder.ToString();
            if (queryCompiler?.Parent != null)
            {
                query.OrderByExp = orderbyExp?.Replace($"{orderbyComplier.Table.AsName}.", $"order by {queryCompiler.Parent.Table.AsName}.");
                orderbyExp = null;
            }
            if (query.PageSize == 0 || queryCompiler?.Parent != null)
            {
                BuilderSql(sql, selectComplier.Builder.ToString(), tableSql, whereComplier.Builder.ToString(),
                           groupbyComplier.Builder.ToString(), havingComplier.Builder.ToString(), orderbyExp, query.IsDisinct);
            }
            else
            {
                BuilderPageSql(sql, selectComplier.Builder.ToString(), tableSql, whereComplier.Builder.ToString(),
                               groupbyComplier.Builder.ToString(), havingComplier.Builder.ToString(), orderbyExp, query);

            }
            if (query.IsReturnCount && query.PageSize > 0)
                BuilderCountSql(sql, obj, selectComplier, tableSql, whereComplier.Builder.ToString(),
                                groupbyComplier.Builder.ToString(), havingComplier.Builder.ToString(), query.IsDisinct);
            return sql.ToString();
        }
        #region 重写分页

        protected override void BuilderPageSql(StringBuilder sql, string selectExp, string fromExp, string whereExp, 
            string groupbyExp, string havingExp, string orderbyExp, QueryInfo query)
        {
            int start = query.StartIndex==-1? query.PageIndex * query.PageSize: query.StartIndex;
            BuilderSql(sql,selectExp,
                fromExp, whereExp, groupbyExp, havingExp, "", query.IsDisinct);
            sql.Append(string.Format(" order by {0} limit {1},{2} ", orderbyExp, start, query.PageSize));
        }
        /// <summary>
        /// 设置Query的Orderby属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="orderbyCompiler"></param>
        protected override string GetDefaultOrderby(OrmObjectInfo obj, QueryInfo query, QueryCompilerInfo orderbyCompiler)
        {
            if (orderbyCompiler.Builder.Length > 0)
            {
                var name = orderbyCompiler.GetFieldName(obj.PrimaryProperty, obj.PrimaryProperty.PropertyName);
                if (obj.PrimaryProperty==null || orderbyCompiler.Builder.ToString().Contains(name))
                    return orderbyCompiler.Builder.ToString();
                orderbyCompiler.Builder.Append($",{name} asc");
                return orderbyCompiler.Builder.ToString();
            }
            if (!string.IsNullOrEmpty(query.SelectExp))
            {
                var selectArray = query.SelectExp.Split(',');
                foreach (var property in obj.Properties)
                {
                    if (selectArray.Contains(property.PropertyName))
                    {
                        return obj.PrimaryProperty==null || obj.PrimaryProperty.PropertyName == property.PropertyName || !string.IsNullOrWhiteSpace(query.GroupByExp) ?
                            string.Format("{0} asc", orderbyCompiler.GetFieldName(property, property.PropertyName))
                            : string.Format("{0} asc,{1}", orderbyCompiler.GetFieldName(property, property.PropertyName), orderbyCompiler.GetFieldName(obj.PrimaryProperty, obj.PrimaryProperty.PropertyName));
                    }
                       
                }
            }
            return string.Format("{0} asc", obj.PrimaryProperty != null && !query.IsDisinct ? orderbyCompiler.GetFieldName(obj.PrimaryProperty, obj.PrimaryProperty.PropertyName) : orderbyCompiler.GetFieldName(obj.Properties.First(), obj.Properties.First().PropertyName));
        }

        #endregion

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void AddParamter(System.Data.Common.DbCommand command, string name, object value)
        {
            if (string.IsNullOrEmpty(name)) return;
            name = string.Format("@{0}", name);
            if (value == null)
            {
                command.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            }
            else if (value is Array)
            {
                var array = value as Array;
                if (array.Length > 0)
                {
                    var builder = new StringBuilder();
                    for (int i = 0; i < array.Length; i++)
                    {
                        builder.Append(string.Format("{0},", array.GetValue(i)));
                    }
                    command.Parameters.Add(new MySqlParameter(name, builder.ToString())); 
                }
                
            }
            else if (value.GetType().IsEnum)
            {
                var chars = value.GetType().GetCustomAttributes(typeof (CharEnumAttribute), true);
                command.Parameters.Add(chars.Length > 0
                                           ? new MySqlParameter(name, Convert.ChangeType(value, typeof (char)))
                                           : new MySqlParameter(name, value));
            }
            else
            {
                command.Parameters.Add(new MySqlParameter(name, value));
            }

        }


    }
}
