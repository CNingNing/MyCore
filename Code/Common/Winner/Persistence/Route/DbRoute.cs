using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Exceptions;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Route
{
  

    public class DbRoute : IDbRoute
    {
        private IDictionary<string, DbRouteInfo> _dbRoutes = new Dictionary<string, DbRouteInfo>();

        public IDictionary<string, DbRouteInfo> DbRoutes
        {
            get { return _dbRoutes; }
            set
            {
                _dbRoutes = value;
            }
        }
   
        /// <summary>
        /// 得到路由
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual DbRouteInfo GetDbRoute(string name)
        {
            if (DbRoutes != null && DbRoutes.ContainsKey(name))
                return DbRoutes[name];
            return null;
        }

        #region 读路由

        /// <summary>
        /// 得到读的路由
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual IList<QueryInfo> GetRouteQueries(QueryInfo query)
        {
            if (string.IsNullOrEmpty(query.Object.RouteName) || !DbRoutes.ContainsKey(query.Object.RouteName))
                return null;
            var dbRoute = DbRoutes[query.Object.RouteName];
            query.DbRoute = dbRoute;
            var handleQueries = GetHandleQueries(dbRoute, query);
            if (handleQueries != null && handleQueries.Count > 0)
                return handleQueries;
            if (dbRoute.Rules == null || dbRoute.Rules.Count == 0)
                return null;
            if (query.RouteIds!=null)
            {
                var result = new List<QueryInfo>();
                foreach (var routeId in query.RouteIds)
                {
                    var sharding = dbRoute.GetSharding(routeId);
                    if (sharding != null)
                    {
                        var cloneQuery = GetQuery(dbRoute, query, sharding);
                        cloneQuery.PageIndex = query.PageIndex;
                        cloneQuery.PageSize = query.PageSize;
                        result.Add(cloneQuery);
                    }
                }
                if (result.Count == 1)
                {
                    result[0].PageIndex = query.PageIndex;
                    result[0].PageSize = query.PageSize;
                }
                return result;


            }
            var names = query.GetRouteParameters(query.WhereExp);
            if (names != null && names.Count > 0)
            {
                foreach (var rule in dbRoute.Rules.Where(it=>it.RuleType== RuleType.All || it.RuleType== RuleType.Read))
                {
                    if (!names.ContainsKey(rule.PropertyName))
                        continue;
                    var paramterValues = names[rule.PropertyName].Distinct();
                    var result = new List<QueryInfo>();
                    foreach (var paramterValue in paramterValues)
                    {
                        if (paramterValue is Array)
                        {
                            var arr = paramterValue as Array;
                            foreach (var ar in arr)
                            {
                                if(ar==null)
                                    continue;
                                var value = rule.IsHash ? GenerateLongId(ar.ToString()) : ar;
                                var sharding = dbRoute.GetSharding(value, rule);
                                if (sharding == null) continue;
                                AppendQueryBySharding(result, dbRoute, sharding, query);
                            }
                        }
                        else
                        {
                            if(paramterValue==null)
                                continue;
                            var value = rule.IsHash ? GenerateLongId(paramterValue.ToString()) : paramterValue;
                            var  sharding = dbRoute.GetSharding(value, rule);
                            if (sharding == null) continue;
                            AppendQueryBySharding(result, dbRoute, sharding, query);
                        }
                    }
                    if (result.Count == 1)
                    {
                        result[0].PageIndex = query.PageIndex;
                        result[0].PageSize = query.PageSize;
                    }
                    else if (result.Count > 1 && dbRoute.TopCount > 0 && (query.PageSize + query.PageIndex * query.PageSize)>dbRoute.TopCount)
                    {
                        throw new LimitCountOverflowException(string.Format("Limit Count Is {0}", dbRoute.TopCount));
                    }
                    if (result.Count > 0)
                        return result;
                }
            }
            if (dbRoute.TopCount > 0 && (query.PageSize + query.PageIndex * query.PageSize) > dbRoute.TopCount)
            {
                throw new LimitCountOverflowException(string.Format("Limit Count Is {0}", dbRoute.TopCount));
            }

            switch (dbRoute.Type)
            {
                case DbRouteType.All: return GetAllQueries(dbRoute, query);
                case DbRouteType.Default: return new List<QueryInfo>{ query};
            }
            return null;
        }

        /// <summary>
        /// 得到查询
        /// </summary>
        /// <param name="dbRoute"></param>
        /// <param name="sharding"></param>
        /// <param name="query"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual void AppendQueryBySharding(IList<QueryInfo> result, DbRouteInfo dbRoute, ShardingInfo sharding, QueryInfo query)
        {
            var cloneQuery = GetQuery(dbRoute, query, sharding);
            if (cloneQuery != null &&
                result.Count(
                    it =>
                        it.GetDataBase == cloneQuery.GetDataBase &&
                        it.GetTableName == cloneQuery.GetTableName) == 0)
                result.Add(cloneQuery);
        }
        /// <summary>
        /// 得到查询
        /// </summary>
        /// <param name="dbRoute"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual IList<QueryInfo> GetHandleQueries(DbRouteInfo dbRoute, QueryInfo query)
        {
            if (dbRoute.GetQueryShardingHandle == null)
                return null;
            var shardings = dbRoute.GetQueryShardingHandle(query);
            if (shardings != null && shardings.Count > 0)
            {
                var result = new List<QueryInfo>();
                if (shardings.Count == 1)
                {
                    var sharding = shardings[0];
                    query.TableIndex = sharding.TableIndex;
                    query.GetDataBase = $"{dbRoute.GetDataBase}{sharding.DatabaseIndex}";
                    result.Add(query);
                }
                else
                {
                    MergeQueries(result, dbRoute, query, shardings);
                }
                return result;
            }
            return null;
        }
        /// <summary>
        /// 得到所有路由
        /// </summary>
        /// <param name="dbRoute"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual IList<QueryInfo> GetAllQueries(DbRouteInfo dbRoute, QueryInfo query)
        {
            var result = new List<QueryInfo>();
            var shardings = dbRoute.GetAllShardings();
            if (shardings == null)
                return result;
            MergeQueries(result, dbRoute, query, shardings);
            return result;
        }

        /// <summary>
        /// 合并查询
        /// </summary>
        /// <param name="result"></param>
        /// <param name="dbRoute"></param>
        /// <param name="query"></param>
        /// <param name="shardings"></param>
        /// <returns></returns>
        protected virtual void MergeQueries(IList<QueryInfo> result, DbRouteInfo dbRoute,QueryInfo query,
            IList<ShardingInfo> shardings)
        {
            foreach (var sharding in shardings)
            {
                var cloneQuery = GetQuery(dbRoute, query, sharding);
                if (cloneQuery != null &&
                    result.Count(
                        it => it.GetDataBase == cloneQuery.GetDataBase && it.GetTableName == cloneQuery.GetTableName) ==
                    0)
                    result.Add(cloneQuery);
            }
     
        }
     
        /// <summary>
        /// 得到查询
        /// </summary>
        /// <param name="dbRoute"></param>
        /// <param name="query"></param>
        /// <param name="sharding"></param>
        /// <returns></returns>
        protected virtual QueryInfo GetQuery(DbRouteInfo dbRoute,QueryInfo query, ShardingInfo sharding)
        {
            if (sharding == null) return null;
             var result = new QueryInfo
             {
                 DataCount = query.DataCount,
                 FromExp = query.FromExp,
                 GroupByExp = query.GroupByExp,
                 HavingExp = query.HavingExp,
                 IsDisinct = query.IsDisinct,
                 IsGreedyLoad = query.IsGreedyLoad,
                 IsReturnCount = query.IsReturnCount,
                 IsLazyLoad = query.IsLazyLoad,
                 OrderByExp = query.OrderByExp,
                 PageSize = query.PageSize + query.PageIndex * query.PageSize,
                 RemoteQueries = query.RemoteQueries,
                 SelectExp = query.SelectExp,
                 WhereExp = query.WhereExp,
                 TableIndex = sharding.TableIndex,
                 GetDataBase = $"{dbRoute.GetDataBase}{sharding.DatabaseIndex}",
                 Object = query.Object,
                 DbRoute = query.DbRoute
             };
            if (query.Parameters != null)
            {
                result.Parameters = new Dictionary<string, object>();
                foreach (var parameter in query.Parameters)
                {
                    result.Parameters.Add(parameter.Key, parameter.Value);
                }
            }
            if (query.Cache != null)
            {
                result.Cache = new CacheInfo
                {
                    Time = query.Cache.Time,
                    TimeSpan = query.Cache.TimeSpan
                };
            }
            return result;
        }

     

        #endregion

        #region 写路由

        /// <summary>
        /// 得到写的路由
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public virtual void SetRouteSaveInfo(SaveInfo save)
        {
            if (string.IsNullOrEmpty(save.Object.RouteName) || !DbRoutes.ContainsKey(save.Object.RouteName))
                return;
            var dbRoute = DbRoutes[save.Object.RouteName];
            if (dbRoute.GetSaveShardingHandle != null)
            {
                var sharding = dbRoute.GetSaveShardingHandle(save.Entity);
                if (sharding != null)
                {
                    save.SetDataBase = $"{dbRoute.SetDataBase}{sharding.DatabaseIndex}";
                    save.TableIndex = sharding.TableIndex;
                    return;
                }
            }
            if (dbRoute.Rules == null || dbRoute.Rules.Count == 0)
                return;
            var rules = dbRoute.Rules.Where(it => it.RuleType == RuleType.All || it.RuleType == RuleType.Write);
            foreach (var rule in rules)
            {
                var valueObj = save.Entity.GetProperty(rule.PropertyName);
                if(valueObj==null || string.IsNullOrWhiteSpace(valueObj.ToString()))
                    continue;
                var value = rule.IsHash
                    ? GenerateLongId(valueObj.ToString())
                    : Convert.ToInt64(valueObj) ;
                value = Math.Abs(value);
                var sharding = dbRoute.GetSharding(value, rule);
                if (sharding == null)
                    continue;
                save.SetDataBase = $"{dbRoute.SetDataBase}{sharding.DatabaseIndex}"; 
                save.TableIndex = sharding.TableIndex;
                break;
            }
        }

        #endregion
       
       
  

        /// <summary>
        /// 得到MD5加密
        /// </summary>
        /// <returns></returns>
        protected virtual string EncryptMd5(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var md5 = MD5.Create();
            byte[] bytValue = Encoding.UTF8.GetBytes(input);
            byte[] bytHash = md5.ComputeHash(bytValue);
            var sTemp = new StringBuilder();
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp.Append(bytHash[i].ToString("X").PadLeft(2, '0'));
            }
            return sTemp.ToString().ToLower();
        }
        /// <summary>
        /// 得到缓存值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual long GenerateLongId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            byte[] buffer = Encoding.UTF8.GetBytes(EncryptMd5(input.ToLower()));
            var i = BitConverter.ToInt64(buffer, 0);
            return i;
        }
    }

   
}
