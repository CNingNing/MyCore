using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Common
{
    public class QueryCompiler : IQueryCompiler
    {
        #region 属性
        /// <summary>
        /// 查询实例
        /// </summary>
        public virtual ISelectCompiler SelectCompiler { get; set; } 
        /// <summary>
        /// 条件实例
        /// </summary>
        public virtual IWhereCompiler WhereCompiler { get; set; } = new WhereCompiler();
        /// <summary>
        /// 分组实例
        /// </summary>
        public virtual IGroupbyCompiler GroupbyCompiler { get; set; } = new GroupbyCompiler();
        /// <summary>
        /// 排序实例
        /// </summary>
        public virtual IOrderbyCompiler OrderbyCompiler { get; set; } = new OrderbyCompiler();
        /// <summary>
        /// 分组条件实例
        /// </summary>
        public virtual IHavingCompiler HavingCompiler { get; set; } = new HavingCompiler();
        /// <summary>
        /// 
        /// </summary>
        public virtual string FeildBeforeTag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string FeildAfterTag { get; set; }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public QueryCompiler()
        { 
        }
    
        #endregion

        #region 接口的实现

        /// <summary>
        /// 解析查询
        /// </summary>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        public virtual void Translate(DbCommand command, OrmObjectInfo obj, QueryInfo query)
        {
            TranslateQuery(obj, query);
            command.CommandText = query.Sql;
            if (query.Parameters == null) return;
            foreach (var paramter in query.SqlParameters)
            {
                AddParamter(command,paramter.Key,paramter.Value);
            }
        }

        /// <summary>
        /// 翻译查询
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        public virtual void TranslateQuery(OrmObjectInfo obj, QueryInfo query)
        {
            TranslateQuery(obj, query, null);
        }

        /// <summary>
        /// 翻译查询
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="queryCompiler"></param>
        public virtual void TranslateQuery(OrmObjectInfo obj, QueryInfo query, QueryCompilerInfo queryCompiler)
        {
            TranslateQuery(obj, query, queryCompiler, false);
        }

        /// <summary>
        /// 翻译查询
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="queryCompiler"></param>
        /// <param name="isIgnoreAs"></param>
        public virtual void TranslateQuery(OrmObjectInfo obj, QueryInfo query, QueryCompilerInfo queryCompiler,bool isIgnoreAs)
        {
            var table = new TableInfo { Joins = new Dictionary<string, JoinInfo>() };
            if (queryCompiler != null && queryCompiler.Table != null)
            {
                table.Tag = queryCompiler.Table.CreateSubTag();
            }
            var selectComplier = new QueryCompilerInfo(obj, query.SelectExp, table, null, FeildBeforeTag, FeildAfterTag) { Query = query, TranslateQuery = TranslateQuery ,IsIgnoreAs= isIgnoreAs };
            var groupbyComplier = new QueryCompilerInfo(obj, query.GroupByExp, table, null, FeildBeforeTag, FeildAfterTag) { Query = query };
            var orderbyComplier = new QueryCompilerInfo(obj, query.OrderByExp, table, null, FeildBeforeTag, FeildAfterTag) { Query = query };
            var whereComplier = new WhereCompilerInfo(obj, query.WhereExp, table, null, FeildBeforeTag, FeildAfterTag) { Query = query, Parent = queryCompiler?.Parent };
            var havingComplier = new WhereCompilerInfo(obj, query.HavingExp, table, null, FeildBeforeTag, FeildAfterTag) { Query = query };
            SelectCompiler.Translate(selectComplier);
            WhereCompiler.Translate(whereComplier);
            GroupbyCompiler.Translate(groupbyComplier);
            HavingCompiler.Translate(havingComplier);
            OrderbyCompiler.Translate(orderbyComplier);
            if (queryCompiler != null && !string.IsNullOrEmpty(queryCompiler.Chainon))
            {
                if (whereComplier.Builder.Length > 0)
                    whereComplier.Builder.Append(" and ");
                whereComplier.Builder.Append(string.Format(queryCompiler.Chainon, table.AsName));
            }
            var tableSql = selectComplier.GetJoinTable(query);
            query.Sql = GetSql(obj, selectComplier, tableSql, whereComplier, groupbyComplier, orderbyComplier,
                              havingComplier, queryCompiler, query);
        }
        #endregion

        #region 查询方法

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
        protected virtual string GetSql(OrmObjectInfo obj, QueryCompilerInfo selectComplier, string tableSql,
                                        WhereCompilerInfo whereComplier, QueryCompilerInfo groupbyComplier,
                                        QueryCompilerInfo orderbyComplier, WhereCompilerInfo havingComplier,
            QueryCompilerInfo queryCompiler, QueryInfo query)
        {
            var sql = new StringBuilder();
            var orderbyExp = query.PageSize != 0
                                 ? GetDefaultOrderby(obj,query, orderbyComplier)
                                 : orderbyComplier.Builder.ToString();
            if (query.PageSize == 0)
            {
                BuilderSql(sql, selectComplier.Builder.ToString(), tableSql, whereComplier.Builder.ToString(),
                           groupbyComplier.Builder.ToString(), havingComplier.Builder.ToString(), orderbyExp, query.IsDisinct);
            }
            else
            {
                BuilderPageSql(sql, selectComplier.Builder.ToString(), tableSql, whereComplier.Builder.ToString(),
                               groupbyComplier.Builder.ToString(), havingComplier.Builder.ToString(), orderbyExp, query);

            }
            if (query.IsReturnCount && query.PageSize>0)
                BuilderCountSql(sql, obj, selectComplier, tableSql, whereComplier.Builder.ToString(),
                                groupbyComplier.Builder.ToString(), havingComplier.Builder.ToString(),query.IsDisinct);
            return sql.ToString();
        }

        /// <summary>
        /// 设置Query的Orderby属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="orderbyCompiler"></param>
        protected virtual string GetDefaultOrderby(OrmObjectInfo obj,QueryInfo query, QueryCompilerInfo orderbyCompiler)
        {
            if (orderbyCompiler.Builder.Length>0) return orderbyCompiler.Builder.ToString();
            if (!string.IsNullOrEmpty(query.SelectExp))
            {
                var selectArray = query.SelectExp.Split(',');
                foreach (var property in obj.Properties)
                {
                    if (selectArray.Contains(property.PropertyName))
                        return string.Format("{0} asc", orderbyCompiler.GetFieldName(property, property.PropertyName));
                } 
            }
            return string.Format("{0} asc", obj.PrimaryProperty != null && !query.IsDisinct ? orderbyCompiler.GetFieldName(obj.PrimaryProperty, obj.PrimaryProperty.PropertyName) : orderbyCompiler.GetFieldName(obj.Properties.First(), obj.Properties.First().PropertyName));
        }

        /// <summary>
        /// 拼接sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="selectExp"></param>
        /// <param name="fromExp"></param>
        /// <param name="whereExp"></param>
        /// <param name="groupbyExp"></param>
        /// <param name="havingExp"></param>
        /// <param name="orderbyExp"></param>
        /// <param name="isDisinct"></param>
        /// <returns></returns>
        protected virtual void BuilderSql(StringBuilder sql,string selectExp, string fromExp, string whereExp,
            string groupbyExp, string havingExp, string orderbyExp, bool isDisinct)
        {
            sql.AppendFormat("select {0}{1} from {2}", isDisinct ? "Distinct " : "", selectExp, fromExp);
            if (!string.IsNullOrEmpty(whereExp))
                sql.AppendFormat(" where {0}", whereExp);
            if (!string.IsNullOrEmpty(groupbyExp))
                sql.AppendFormat(" group by {0}", groupbyExp);
            if (!string.IsNullOrEmpty(havingExp))
                sql.AppendFormat(" having {0}", havingExp);
            if (!string.IsNullOrEmpty(orderbyExp))
                sql.AppendFormat(" order by {0}", orderbyExp);

        }

        /// <summary>
        /// 拼接分页sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="selectExp"></param>
        /// <param name="fromExp"></param>
        /// <param name="whereExp"></param>
        /// <param name="groupbyExp"></param>
        /// <param name="havingExp"></param>
        /// <param name="orderbyExp"></param>
        /// <param name="query"></param>
        protected virtual void BuilderPageSql(StringBuilder sql, string selectExp, string fromExp, string whereExp,
            string groupbyExp, string havingExp, string orderbyExp,QueryInfo query)
        {
            var startIndex = query.StartIndex == -1 ? query.PageIndex * query.PageSize : query.StartIndex;
            string forderby = orderbyExp.Replace(" asc", " asc1").Replace(" desc", " asc").Replace(" asc1", " desc");
            sql.Append("select * from (");
            sql.AppendFormat("select{0} top {1} * from (", query.IsDisinct ?" Distinct":"", query.PageSize);
            selectExp = string.Format("top {0} {1}", startIndex, selectExp);
            BuilderSql(sql, selectExp, fromExp, whereExp, groupbyExp, havingExp, orderbyExp, false);
            sql.AppendFormat(") t order by {0}", GetReplacePageOrderbyTable("t", forderby));
            sql.AppendFormat(") t order by {0}", GetReplacePageOrderbyTable("t", orderbyExp));

        }
    

        /// <summary>
        /// 替换Orderby的表名
        /// </summary>
        /// <param name="name"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        protected virtual string GetReplacePageOrderbyTable(string name, string orderby)
        {
            const string pattern = @",.*?\.";
            orderby = Regex.Replace(string.Format(",{0}", orderby), pattern, string.Format(",{0}.", name));
            orderby = orderby.Remove(0, 1);
            return orderby;
        }

        /// <summary>
        /// 得到数据总数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="selectComplier"></param>
        /// <param name="fromExp"></param>
        /// <param name="whereExp"></param>
        /// <param name="groupbyExp"></param>
        /// <param name="havingExp"></param>
        /// <param name="obj"></param>
        /// <param name="isDisinct"></param>
        /// <returns></returns>
        protected virtual void BuilderCountSql(StringBuilder sql,
            OrmObjectInfo obj, QueryCompilerInfo selectComplier,
            string fromExp, string whereExp,
            string groupbyExp, string havingExp, bool isDisinct)
        {
            sql.AppendFormat(";select count(1) from ( select {0}{1} from ", isDisinct ? "Distinct " : "", selectComplier.Builder);
            sql.Append(fromExp);
            if (!string.IsNullOrEmpty(whereExp))
                sql.AppendFormat(" where {0}", whereExp);
            if (!string.IsNullOrEmpty(groupbyExp))
                sql.AppendFormat(" group by {0}", groupbyExp);
            if (!string.IsNullOrEmpty(havingExp))
                sql.AppendFormat(" having {0}", havingExp);
            sql.Append(") t");
        }

  
        #endregion

        #region 添加参数


        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual void AddParamter(DbCommand command, string name, object value)
        {
            if (string.IsNullOrEmpty(name) || value == null) return;
            command.Parameters.Add(value);
        }

        #endregion

    }
}
