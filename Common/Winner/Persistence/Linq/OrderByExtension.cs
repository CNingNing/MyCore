using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Winner.Persistence.Linq
{
    static public class OrderByExtension
    {
        /// <summary>
        /// 升序
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="propertyStr"></param>
        /// <returns></returns>
        public static IQueryable OrderBy(this IQueryable source, Type type, string propertyStr)
        {
            ParameterExpression param = Expression.Parameter(type, "c");
            PropertyInfo property = type.GetProperty(propertyStr);
            Expression propertyAccessExpression = Expression.MakeMemberAccess(param, property);
            LambdaExpression le = Expression.Lambda(propertyAccessExpression, param);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderBy", new[] { type, property.PropertyType }, source.Expression, Expression.Quote(le));
            return source.Provider.CreateQuery(resultExp);
        }

        /// <summary>
        /// 降序
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="propertyStr"></param>
        /// <returns></returns>
        public static IQueryable OrderByDescending(this IQueryable source, Type type, string propertyStr)
        {
            ParameterExpression param = Expression.Parameter(type, "c");
            PropertyInfo property = type.GetProperty(propertyStr);
            Expression propertyAccessExpression = Expression.MakeMemberAccess(param, property);
            LambdaExpression le = Expression.Lambda(propertyAccessExpression, param);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderByDescending", new[] { type, property.PropertyType }, source.Expression, Expression.Quote(le));
            return source.Provider.CreateQuery(resultExp);
        }

        /// <summary>
        /// 升序
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="propertyStr"></param>
        /// <returns></returns>
        public static IQueryable ThenBy(this IQueryable source, Type type, string propertyStr)
        {
            ParameterExpression param = Expression.Parameter(type, "c");
            PropertyInfo property = type.GetProperty(propertyStr);
            Expression propertyAccessExpression = Expression.MakeMemberAccess(param, property);
            LambdaExpression le = Expression.Lambda(propertyAccessExpression, param);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "ThenBy", new[] { type, property.PropertyType }, source.Expression, Expression.Quote(le));
            return source.Provider.CreateQuery(resultExp);
        }

        /// <summary>
        /// 降序
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="propertyStr"></param>
        /// <returns></returns>
        public static IQueryable ThenByDescending(this IQueryable source, Type type, string propertyStr)
        {
            ParameterExpression param = Expression.Parameter(type, "c");
            PropertyInfo property = type.GetProperty(propertyStr);
            Expression propertyAccessExpression = Expression.MakeMemberAccess(param, property);
            LambdaExpression le = Expression.Lambda(propertyAccessExpression, param);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "ThenByDescending", new[] { type, property.PropertyType }, source.Expression, Expression.Quote(le));
            return source.Provider.CreateQuery(resultExp);
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Take",
                    new[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IQueryable Skip(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Skip",
                    new[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }
    }
}
