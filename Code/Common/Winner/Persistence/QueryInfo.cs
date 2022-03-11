using System;
using System.Collections.Generic;
using System.Linq;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Linq;
using Winner.Persistence.Relation;
using Winner.Persistence.Route;
using Winner.Persistence.Translation;


namespace Winner.Persistence
{


    public class QueryInfo : ParameterInfo
    {
        #region  属性
        /// <summary>
        /// 是否返回总数
        /// </summary>
        public virtual bool IsSetLocalStorage { get; set; }
        /// <summary>
        /// Sql语句
        /// </summary>
        public virtual string Sql { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public virtual IDictionary<string,object> SqlParameters { get; set; } 
        /// <summary>
        /// 查询类型
        /// </summary>
        public QueryType QueryType { get; set; }
        /// <summary>
        /// 设置查询缓存
        /// </summary>
        public virtual CacheInfo Cache { get; set; }
        /// <summary>
        /// 页索引
        /// </summary>
        public virtual int PageIndex { get; set; }
        /// <summary>
        /// 页大小
        /// </summary>
        public virtual int PageSize { get; set; }

        /// <summary>
        /// 页索引
        /// </summary>
        public virtual int StartIndex { get; set; } = -1;
        /// <summary>
        /// 是否返回总数
        /// </summary>
        public virtual bool IsReturnCount { get; set; } = true;
        /// <summary>
        /// 返回数据总数
        /// </summary>

        public virtual int DataCount { get; set; }
    
        /// <summary>
        /// 查询对象
        /// </summary>
        public virtual string SelectExp { get; set; }
        /// <summary>
        /// 查询对象
        /// </summary>
        public virtual string FromExp { get; set; }    
        /// <summary>
        /// 查询对象
        /// </summary>
        public virtual string WhereExp { get; set; }
        /// <summary>
        /// 查询对象
        /// </summary>
        public virtual string GroupByExp { get; set; }
        /// <summary>
        /// 查询对象
        /// </summary>
        public virtual string HavingExp { get; set; }
        /// <summary>
        /// 查询对象
        /// </summary>
        public virtual string OrderByExp { get; set; }
        /// <summary>
        /// 是否去除重复
        /// </summary>
        public virtual bool IsDisinct { get; set; }
        /// <summary>
        /// 是否延迟加载
        /// </summary>
        public virtual bool IsLazyLoad { get; set; }
        /// <summary>
        /// 是否贪婪加载
        /// </summary>
        public virtual bool IsGreedyLoad { get; set; }
        /// <summary>
        /// 是否贪婪加载
        /// </summary>
        public virtual int[] RouteIds { get; set; }
        #endregion

        #region 查询动态属性
        /// <summary>
        /// ORM操作
        /// </summary>
        public virtual OrmObjectInfo Object { get; set; }
        private string _getDataBase;
        /// <summary>
        /// 得到写库
        /// </summary>
        public virtual string GetDataBase
        {
            set { _getDataBase = value; }
            get
            {
                if (string.IsNullOrEmpty(_getDataBase))
                    return Object?.GetDataBase;
                return _getDataBase;
            }
        }
        /// <summary>
        /// 表索引
        /// </summary>
        public virtual string TableIndex { get; set; }
        /// <summary>
        /// DbRoute
        /// </summary>
        public virtual DbRouteInfo DbRoute { get; set; }

        private string _getTableName;
        /// <summary>
        /// 得到写库
        /// </summary>
        public virtual string GetTableName
        {
            get
            {
                if (string.IsNullOrEmpty(_getTableName))
                    _getTableName = GetGetTableName(Object);
                return _getTableName;
            }
        }
        /// <summary>
        /// 得到表名
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public virtual string GetGetTableName(OrmMapInfo map)
        {
            var ormObject = map.GetMapObject();
            if (ormObject.ObjectName != Object.ObjectName  && !map.IsMapTableAutoSharding)
                return ormObject.GetTableName;
            if (!string.IsNullOrEmpty(TableIndex) && !string.IsNullOrEmpty(ormObject.RouteName))
                return string.Format("{0}{1}", ormObject.GetTableName, TableIndex);
            return ormObject.GetTableName;
        }
        /// <summary>
        /// 得到表名
        /// </summary>
        /// <param name="ormObject"></param>
        /// <returns></returns>
        public virtual string GetGetTableName(OrmObjectInfo ormObject)
        {
            if (!string.IsNullOrEmpty(TableIndex) && !string.IsNullOrEmpty(ormObject.RouteName))
                return string.Format("{0}{1}", ormObject.GetTableName, TableIndex);
            return ormObject?.GetTableName;
        }
        /// <summary>
        /// 得到表名
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        public virtual string GetGetTableName(JoinInfo join)
        {
            var getTableName = join.Map != null && join.Map.IsRemote && !string.IsNullOrEmpty(join.Map.RemoteName)
                ? join.Map.RemoteName
                : join.Object.GetTableName;
            if (join.Object.RouteName != Object.ObjectName && join.Map != null && !join.Map.IsMapTableAutoSharding)
                return getTableName;
            if (!string.IsNullOrEmpty(TableIndex) && !string.IsNullOrEmpty(join.Object.RouteName))
                return string.Format("{0}{1}", getTableName, TableIndex);
            return getTableName;
        }
        #endregion

        #region 方法

        /// <summary>
        /// 并行计算
        /// </summary>
        public virtual QueryInfo AsParallel()
        {
            QueryType = QueryType.Parallel;
            return this;
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new QueryInfo SetParameter<T>(T value)
        {
            base.SetParameter(value);
            return this;
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public new QueryInfo SetParameter<T>(string name, T value)
        {
            base.SetParameter(name, value);
            return this;
        }
        

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public virtual QueryInfo SetCacheTime(DateTime cacheTime)
        {
            Cache = Cache ?? new CacheInfo ();
            Cache.Time = cacheTime;
            return this;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        public virtual QueryInfo SetCacheType(CacheType cacheType)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.Type = cacheType;
            return this;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="checkTimeSpan"></param>
        /// <returns></returns>
        public virtual QueryInfo SetCacheCheckTimeSpan(int checkTimeSpan)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.CheckTimeSpan = checkTimeSpan;
            return this;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <returns></returns>
        public virtual QueryInfo AppendCacheDependency<T>()
        {
            Cache = Cache ?? new CacheInfo();
            Cache.Dependencies = Cache.Dependencies ?? new List<string>();
            Cache.Dependencies.Add(typeof(T).FullName);
            return this;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual QueryInfo SetCacheName(string name)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.Name = name;
            return this;
        }
        ///// <summary>
        ///// 设置缓存
        ///// </summary>
        ///// <param name="cacheTime"></param>
        ///// <returns></returns>
        //public virtual QueryInfo SetCacheTime2(DateTime cacheTime)
        //{
        //    Cache = Cache ?? new CacheInfo();
        //    Cache.Time2 = cacheTime;
        //    return this;
        //}
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public virtual QueryInfo SetCacheTimeSpan(long timeSpan)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.TimeSpan = timeSpan;
            return this;
        }
        /// <summary>
        /// 设置页索引
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public virtual QueryInfo SetPageIndex(int pageIndex)
        {
            PageIndex = pageIndex;
            return this;
        }
        /// <summary>
        /// 设置页大小
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual QueryInfo SetPageSize(int pageSize)
        {
            PageSize = pageSize;
            return this;
        }
        /// <summary>
        /// 设置返回数据总数
        /// </summary>
        /// <param name="dataCount"></param>
        /// <returns></returns>
        public virtual QueryInfo SetDataCount(int dataCount)
        {
            DataCount = dataCount;
            return this;
        }

        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <returns></returns>
        public virtual QueryInfo Distinct()
        {
            IsDisinct = true;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual QueryInfo Select(string exp)
        {
            SelectExp = exp;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual QueryInfo From(string exp)
        {
            FromExp = exp;
            return this;
        }

        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <returns></returns>
        public virtual QueryInfo From<T>()
        {
            FromExp = typeof(T).FullName;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual QueryInfo Where(string exp)
        {
            WhereExp = exp;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual QueryInfo AppendWhere(string exp)
        {
            WhereExp = string.IsNullOrWhiteSpace(WhereExp) ? exp : string.Format("{0} && {1}", WhereExp, exp.Contains(" or ") ? $"({exp})" : exp);
            return this;
        }

        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual QueryInfo GroupBy(string exp)
        {
            GroupByExp = exp;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual QueryInfo Having(string exp)
        {
            HavingExp = exp;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual QueryInfo OrderBy(string exp)
        {
            OrderByExp = exp;
            return this;
        }
  
        #endregion

        #region 远程映射
        /// <summary>
        /// 延迟
        /// </summary>
        public virtual IDictionary<string, RemoteQueryInfo> RemoteQueries { get; set; }


        #endregion
    
    }

    public class QueryInfo<T> : QueryInfo
    {
        #region 执行的查询

        /// <summary>
        /// 查询对象
        /// </summary>
        /// <returns></returns>
        public virtual IList<T> ToList()
        {
            return Creator.Get<IContext>().GetInfos<IList<T>>(this);
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public virtual TResult Execute<TResult>()
        {
            return Creator.Get<IContext>().GetInfos<TResult>(this);
        }

        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IQueryable<T> Query()
        {
            var name = string.Format("{0},{1}", typeof(T).FullName,
                                     typeof(T).Module.Name.Replace(".DLL", "").Replace(".dll", ""));
            From(name);
            return new Queryable<T>(this);
        }

        #endregion

        #region 方法


        /// <summary>
        /// 并行计算
        /// </summary>
        public new QueryInfo<T> AsParallel()
        {
            QueryType = QueryType.Parallel;
            return this;
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetParameter<TParameter>(TParameter value)
        {
            base.SetParameter(value);
            return this;
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetParameter<TParameter>(string name, TParameter value)
        {
            base.SetParameter(name, value);
            return this;
        }


        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetCacheTime(DateTime cacheTime)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.Time = cacheTime;
            return this;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetCacheType(CacheType cacheType)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.Type = cacheType;
            return this;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="checkTimeSpan"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetCacheCheckTimeSpan(int checkTimeSpan)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.CheckTimeSpan = checkTimeSpan;
            return this;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <returns></returns>
        public new QueryInfo<T> AppendCacheDependency<TDependency>()
        {
            Cache = Cache ?? new CacheInfo();
            Cache.Dependencies = Cache.Dependencies ?? new List<string>();
            Cache.Dependencies.Add(typeof(TDependency).FullName);
            return this;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetCacheName(string name)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.Name = name;
            return this;
        }
 
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetCacheTimeSpan(long timeSpan)
        {
            Cache = Cache ?? new CacheInfo();
            Cache.TimeSpan = timeSpan;
            return this;
        }
 
        /// <summary>
        /// 设置页索引
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetPageIndex(int pageIndex)
        {
            PageIndex = pageIndex;
            return this;
        }
        /// <summary>
        /// 设置页大小
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetPageSize(int pageSize)
        {
            PageSize = pageSize;
            return this;
        }
        /// <summary>
        /// 设置返回数据总数
        /// </summary>
        /// <param name="dataCount"></param>
        /// <returns></returns>
        public new QueryInfo<T> SetDataCount(int dataCount)
        {
            DataCount = dataCount;
            return this;
        }

        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <returns></returns>
        public new QueryInfo<T> Distinct()
        {
            IsDisinct = true;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public new QueryInfo<T> Select(string exp)
        {
            SelectExp = exp;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public new QueryInfo<T> From(string exp)
        {
            FromExp = exp;
            return this;
        }

        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <returns></returns>
        public new QueryInfo<T> From<TEntity>()
        {
            FromExp = typeof(TEntity).FullName;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public new QueryInfo<T> Where(string exp)
        {
            WhereExp = exp;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public new QueryInfo<T> AppendWhere(string exp)
        {
            WhereExp = string.IsNullOrWhiteSpace(WhereExp) ? exp : string.Format("{0} && {1}", WhereExp, exp.Contains(" or ") ? $"({exp})" : exp);
            return this;
        }

        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public new QueryInfo<T> GroupBy(string exp)
        {
            GroupByExp = exp;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public new QueryInfo<T> Having(string exp)
        {
            HavingExp = exp;
            return this;
        }
        /// <summary>
        /// 设置查询语句
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public new QueryInfo<T> OrderBy(string exp)
        {
            OrderByExp = exp;
            return this;
        }

        public void AppendWhere(Func<object, object> p)
        {
            throw new NotImplementedException();
        }


        #endregion
    }

}