using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Winner.Persistence.Linq
{
    public class Queryable<T> : IOrderedQueryable<T>
    {
        public QueryInfo Query { get; private set; }
        public IQueryProvider Provider { get; private set; }
        public Expression Expression { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="query"></param>
        public Queryable(QueryInfo query)
        {
            Provider = new QueryProvider(query);
            Expression = Expression.Constant(this);
            Query = query;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <param name="expression"></param>
        public Queryable(QueryInfo query,QueryProvider provider,Expression expression)
        {
            Provider = provider;
            Expression = expression;
            Query = query;
        }
        public Type ElementType
        {
            get { return typeof (T); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<IEnumerable>(Expression)).GetEnumerator();
        }

    }
}
