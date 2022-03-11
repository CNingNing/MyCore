using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Winner.Persistence.Compiler.Common;

namespace Winner.Persistence.Linq
{
    public static class QueryExtension
    {
        #region 扩展方法
        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> Query<T>(this QueryInfo query)
        {
            var name = string.Format("{0},{1}", typeof (T).FullName,
                                     typeof (T).Module.Name.Replace(".DLL", "").Replace(".dll", ""));
            query.From(name);
            return new Queryable<T>(query);
        }

        #region 扩展方法

        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this IQueryable queryable)
        {
            var query = queryable.GetProperty("Query") as QueryInfo<T>;
            if (query == null)
                return null;
            return query.ToList();
        }

        #endregion

        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static QueryInfo AppendWhere<T>(this QueryInfo query, Expression<Func<T, bool>> predicate)
        {
            var whereExp = query.WhereExp;
            new WhereExpression(query, false, false).Translate(predicate);
            if (!string.IsNullOrWhiteSpace(whereExp))
                query.Where(string.Format("{0} && {1}", whereExp, query.WhereExp.Contains(" or ") ? $"({query.WhereExp})" : query.WhereExp));
            return query;
        }

        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static QueryInfo AppendWhere<T>(this QueryInfo<T> query, Expression<Func<T, bool>> predicate)
        {
            var whereExp = query.WhereExp;
            new WhereExpression(query, false, false).Translate(predicate);
            if (!string.IsNullOrWhiteSpace(whereExp))
                query.Where(string.Format("{0} && {1}", whereExp, query.WhereExp.Contains(" or ") ? $"({query.WhereExp})" : query.WhereExp));
            return query;
        }
        #endregion


    }

}