using System;
using System.Linq;
using System.Linq.Expressions;

namespace Winner.Persistence.Linq
{
    public class QueryProvider : IQueryProvider
    {

        public QueryInfo Query { get; private set; }
        /// <summary>
        /// 是否执行过GroupBy
        /// </summary>
        public bool IsGroupBy { get; set; }
        /// <summary>
        /// 是否执行过GroupBy
        /// </summary>
        public bool IsSelectWhere { get; set; }
        /// <summary>
        /// 是否查询
        /// </summary>
        public bool IsSelect { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="query"></param>
        public QueryProvider(QueryInfo query)
        {
            Query = query;
        }
        /// <summary>
        /// 创建查询
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetElementType();
            try
            {
                return (IQueryable)Activator.CreateInstance(
                    typeof(Queryable<>).MakeGenericType(elementType),
                    new object[] { this, expression });
            }
            catch
            {
                throw new Exception();
            }
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            Translate(expression);
            return  new Queryable<TResult>(Query,this, expression);
        }
        /// <summary>
        /// 翻译
        /// </summary>
        public virtual void Translate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Call)
            {
                var methodCallExpression = expression as MethodCallExpression;
                string[] methods = { "Where", "Select", "SelectMany", "GroupBy" };
                if (methodCallExpression != null && (!IsSelect || !methods.Contains(methodCallExpression.Method.Name)))
                {
                    switch (methodCallExpression.Method.Name)
                    {
                        case "Where":
                            new WhereExpression(Query, IsGroupBy, IsSelectWhere).Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "Select":
                            new SelectExpression(Query).Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "SelectMany":
                            new SelectManyExpression(Query).Translate(methodCallExpression);
                            break;
                        case "GroupBy":
                            new GroupByExpression(Query).Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "OrderBy":
                            new OrderByExpression(Query, false, "asc").Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "ThenBy":
                            new OrderByExpression(Query, true, "asc").Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "OrderByDescending":
                            new OrderByExpression(Query, false, "desc").Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "ThenByDescending":
                            new OrderByExpression(Query, true, "desc").Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "Take":
                            new TakeExpression(Query).Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "Skip":
                            new SkipExpression(Query).Translate(methodCallExpression.Arguments[1]);
                            break;
                        case "Distinct":
                            new DistinctExpression(Query).Translate(null);
                            break;

                    }
                    SetQuery(methodCallExpression.Method.Name);
                }
            }
        }
        /// <summary>
        /// 设置查询
        /// </summary>
        /// <param name="mothodName"></param>
        protected virtual void SetQuery(string mothodName)
        {
            switch (mothodName)
            {
                case "Select":case "SelectMany":
                    IsSelect = true;
                    break;
                case "GroupBy":
                    IsGroupBy = true;
                    break;
            }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public object Execute(Expression expression)
        {
            return null;
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public TResult Execute<TResult>(Expression expression)
        {
            return default(TResult);
        }
    }
}
