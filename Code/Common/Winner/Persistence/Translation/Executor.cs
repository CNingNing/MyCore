using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Data;
using Winner.Persistence.Delay;
using Winner.Persistence.Linq;
using Winner.Persistence.Relation;
using Winner.Persistence.Route;

namespace Winner.Persistence.Translation
{
    public class Executor : IExecutor
    {
      

        #region 属性

        /// <summary>
        /// 编译器实例
        /// </summary>
        public IDataBase DataBase { get; set; }

        /// <summary>
        /// 编译器实例
        /// </summary>
        public IDbRoute DbRoute { get; set; }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public Executor()
        { 
        }

        /// <summary>
        /// 缓存实例，同步信息实例，编译器实例,ORM信息实例
        /// </summary>
        /// <param name="dataBase"></param>
        public Executor(IDataBase dataBase)
        {
            DataBase = dataBase;

        }
        #endregion

        #region 接口的实现

        #region 查询

        /// <summary>
        /// 查询对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="isLazyLoadExecute"></param>
        /// <returns></returns>
        public virtual T GetInfos<T>(OrmObjectInfo obj, QueryInfo query,  bool isLazyLoadExecute = false)
        {
            query.Object = obj;
            query.GetDataBase = obj.GetDataBase;
            if (!string.IsNullOrEmpty(obj.RouteName))
            {
                var result = new List<T>();
                var queries = DbRoute.GetRouteQueries(query);
                if (query.DbRoute != null && queries == null)
                    return default(T);
                if (queries != null)
                {
                    if (queries.Count == 1)
                    {
                        var infos= GetResult<T>(obj, queries[0], isLazyLoadExecute);
                        query.DataCount = queries[0].DataCount;
                        query.Sql = queries[0].Sql;
                        return infos;
                    }
                    var count = 0;
                    if (query.QueryType == QueryType.Sequence && query.PageSize > 0)
                    {
                        var infos = new ArrayList();
                        for (int i = 0; i < queries.Count; i++)
                        {
                            queries[i].IsReturnCount = true;
                            if (infos.Count < query.PageSize)
                            {
                                queries[i].PageSize = query.PageSize;
                                queries[i].StartIndex = query.PageIndex * query.PageSize- count;
                                if (queries[i].StartIndex < 0)
                                    queries[i].StartIndex = 0;
                                var ts = GetResult<T>(obj, queries[i], isLazyLoadExecute) as IEnumerable<EntityInfo>;
                                if (ts != null)
                                {
                                    foreach (var t in ts)
                                    {
                                        infos.Add(t);
                                        if (infos.Count == queries[i].PageSize)
                                            break;
                                    }
                                }
                            }
                            else if(query.IsReturnCount)
                            {
                                queries[i].PageSize = 1;
                                queries[i].PageIndex = 0;
                                GetResult<T>(obj, queries[i], isLazyLoadExecute);
                            }
                            count += queries[i].DataCount;
                        }
                        query.Sql = queries[0].Sql;
                        query.DataCount = count;
                        var type = Type.GetType(obj.ObjectName);
                        return (T)(infos.ToArray(type) as object);
                    }
                    if (query.QueryType== QueryType.Parallel)
                    {
                        Query(obj, query, isLazyLoadExecute, result, queries, 0);
                    }
                    else
                    {
                        foreach (var q in queries)
                        {
                            var r = GetResult<T>(obj,q,isLazyLoadExecute);
                            result.Add(r);
                            query.DataCount += q.DataCount;
                            count += q.DataCount;
                        }
                    }
                    query.Sql = queries[0].Sql;
                    query.DataCount = count;
                    return MergeResult(result, obj, query);
                }
            }
            return GetResult<T>(obj, query, isLazyLoadExecute);
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="isLazyLoadExecute"></param>
        /// <param name="result"></param>
        /// <param name="queries"></param>
        /// <param name="i"></param>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        protected virtual void Query<T>(OrmObjectInfo obj, QueryInfo query, bool isLazyLoadExecute,IList<T> result, IList<QueryInfo> queries, int i)
        {
            if(i>= queries.Count)
                return;
            var task = new Task<T>(() => { return GetResult<T>(obj, queries[i], isLazyLoadExecute); }, TaskCreationOptions.LongRunning);
            task.Start();
            Query(obj, query, isLazyLoadExecute, result, queries, i + 1);
            task.Wait();
            var r = task.Result;
            result.Add(r);
            query.DataCount += queries[i].DataCount;
        }
        /// <summary>
        /// 得到结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="isLazyLoadExecute"></param>
        /// <returns></returns>
        protected virtual T GetResult<T>(OrmObjectInfo obj, QueryInfo query,bool isLazyLoadExecute = false)
        {
            OrmDataBaseInfo db = DataBase.GetDataBase(query.GetDataBase)?.GetAllGetOrmDataBase()?.FirstOrDefault();
            if (db == null)
                return default(T);
            var result = ExecuteQuery<T>(query, obj, db);
            if (query.IsLazyLoad)
                LazyLoad(result, obj, isLazyLoadExecute);
            RemoteLoad(result, query.RemoteQueries == null ? null : query.RemoteQueries.Values.ToList(), obj, isLazyLoadExecute);
            return result;
        }
        #region 合并路由
 

        /// <summary>
        /// 合并路由
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="results"></param>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual T MergeResult<T>(IList<T> results,OrmObjectInfo obj, QueryInfo query)
        {
            var infos = new ArrayList();
            if(query.QueryType== QueryType.Parallel)
            {
                foreach (var result in results.AsParallel())
                {
                    var ts = result as IEnumerable<EntityInfo>;
                    if (ts == null) continue;
                    foreach (var t in ts.AsParallel())
                    {
                        infos.Add(t);
                    }
                }
            }
            else
            {
                foreach (var result in results)
                {
                    var ts = result as IEnumerable<EntityInfo>;
                    if (ts == null) continue;
                    foreach (var t in ts)
                    {
                        infos.Add(t);
                    }
                }
            }
            var type = Type.GetType(obj.ObjectName);
            return (T)FilterResult(infos.ToArray(type), type, query);
        }

        /// <summary>
        /// 过滤数据
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual object FilterResult(Array infos, Type type, QueryInfo query)
        {
            if (string.IsNullOrEmpty(query.OrderByExp) && query.PageSize <= 0)
                return infos;
            var temps = infos.AsQueryable();
            if (!string.IsNullOrEmpty(query.OrderByExp))
            {
                var orderExps = query.OrderByExp.Split(',');
                var i = 0;
                foreach (var orderExp in orderExps)
                {
                    var exps = orderExp.Split(' ');
                    var pName = exps[0].Trim();
                    var tyName = exps.Length > 1 ? exps[1].Trim().ToLower() : "asc";
                    switch (tyName)
                    {
                        case "asc":
                            temps = i == 0 ? temps.OrderBy(type,pName) : temps.ThenBy(type, pName);
                            break;
                        case "desc":
                            temps = i == 0 ? temps.OrderByDescending(type, pName) : temps.ThenByDescending(type, pName);
                            break;
                    }
                }
            }
            if (query.PageSize > 0)
                temps = temps.Skip(query.PageIndex * query.PageSize).Take(query.PageSize);
            var result = new ArrayList();
            foreach (var t in temps)
            {
                result.Add(t);
            }
            return result.ToArray(type);
        }
        #endregion

        #region 延迟加载

        /// <summary>
        /// 延迟加载
        /// </summary>
        /// <param name="result"></param>
        /// <param name="obj"></param>
        /// <param name="isLoad"></param>
        protected virtual void LazyLoad(object result, OrmObjectInfo obj,bool isLoad = false)
        {
            var ormProperties =
                obj.Properties.Where(it => it.Map != null && !it.Map.IsGreedyLoad && !it.Map.IsRemote && it.Map.IsLazyLoad)
                   .ToList();
            if (ormProperties.Count == 0) return;
            var infos = result as IEnumerable<EntityInfo>;
            if (infos == null || infos.Count()==0) return;
            foreach (var ormProperty in ormProperties)
            {
                if(infos.Count()>200)
                {
                    if (infos.Count() > 200)
                    {
                        var pageIndex = 0;
                        while (true)
                        {
                            var ts = infos.Skip(pageIndex * 200).Take(200);
                            if (ts.Count() == 0)
                                break;
                            SetLazyLoad(ormProperty, ts, isLoad);
                            pageIndex++;
                        }
                    }
                }
                else
                {
                    SetLazyLoad(ormProperty, infos, isLoad);
                }
             
            }
        }

        protected virtual void SetLazyLoad(OrmPropertyInfo ormProperty, IEnumerable<EntityInfo> infos, bool isLoad)
        {
            var entities = new Dictionary<object, object>();
            foreach (var info in infos)
            {
                entities.Add(info,
                    ormProperty.Map.MapType == OrmMapType.OneToMany
                        ? info
                        : info.GetProperty(ormProperty.PropertyName));
            }
            var query = GetLazyLoadQuery(infos, ormProperty);
            foreach (var info in infos)
            {
                var proxy = (Proxy)Proxy.Create(info.GetType().GetProperty(ormProperty.PropertyName).PropertyType);
                proxy.Set(entities, ormProperty, this, isLoad, info, info.GetType().GetProperty(ormProperty.PropertyName).PropertyType, query);
                info.GetType().GetProperty(ormProperty.PropertyName).SetValue(info, proxy, null);
                if (isLoad)
                {
                    Newtonsoft.Json.JsonConvert.SerializeObject(proxy);
                    break;
                }
            }
        }
        /// <summary>
        /// 得到查询对象
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="ormProperty"></param>
        protected virtual QueryInfo GetLazyLoadQuery(IEnumerable<EntityInfo> infos, OrmPropertyInfo ormProperty)
        {
            var ids = infos.Select(it => it.GetProperty(ormProperty.Map.ObjectProperty.PropertyName)).ToList();
            var query = new QueryInfo { IsLazyLoad = true };
            query.From(ormProperty.Map.GetMapObject().ObjectName);
            if (ids.Count == 0)
            {
                query.Where(string.Format("{0}==@MapId", ormProperty.Map.MapObjectProperty.PropertyName))
                     .SetParameter("MapId", ids.FirstOrDefault());
            }
            else
            {
                query.Where(string.Format("@MapIds.Contains({0})", ormProperty.Map.MapObjectProperty.PropertyName))
                     .SetParameter("MapIds", ids.ToArray());
            }
            return query;
        }
        #endregion

        #region 远程加载

        /// <summary>
        /// 延迟加载
        /// </summary>
        /// <param name="result"></param>
        /// <param name="remoteQueries"></param>
        /// <param name="obj"></param>
        /// <param name="isExcuteLazy"></param>
        protected virtual void RemoteLoad(object result, IList<RemoteQueryInfo> remoteQueries, OrmObjectInfo obj,bool isExcuteLazy)
        {
            if (remoteQueries == null || remoteQueries.Count == 0 || result == null)
                return;
            var infos = result as IEnumerable<EntityInfo>;
            if (infos == null)
                return;
            foreach (var remoteQuery in remoteQueries)
            {
                OrmPropertyInfo ormProperty;
                var tempInfos = GetRemoteEntities(infos, remoteQuery.PropertyName, obj, out ormProperty);
                if (ormProperty == null || ormProperty.Map == null || tempInfos==null || tempInfos.Count()==0)
                    continue;
                if(tempInfos.Count()>200)
                {
                    var pageIndex = 0;
                    while (true)
                    {
                        var ts = tempInfos.Skip(pageIndex * 200).Take(200);
                        if (ts.Count() == 0)
                            break;
                        SetRemoteLoad(ts, ormProperty, remoteQuery, obj, isExcuteLazy);
                        pageIndex++;
                    }
                }
                else
                {
                    SetRemoteLoad(tempInfos, ormProperty, remoteQuery, obj, isExcuteLazy);
                }
               
            }
        }
        protected virtual void SetRemoteLoad(IEnumerable<EntityInfo> infos, OrmPropertyInfo ormProperty,RemoteQueryInfo remoteQuery, OrmObjectInfo obj, bool isExcuteLazy)
        {
            if (remoteQuery.IsLazyLoad && ormProperty.Map.IsLazyLoad)
            {
                var entities = new Dictionary<object, object>();
                foreach (var info in infos)
                {
                    entities.Add(info, info.GetProperty(ormProperty.PropertyName));
                }
                var query = GetRemoteQuery(infos, remoteQuery, ormProperty);
                foreach (var info in infos)
                {

                    var proxy = (Proxy)Proxy.Create(info.GetType().GetProperty(ormProperty.PropertyName).PropertyType);
                    proxy.Set(entities, ormProperty, this, isExcuteLazy, info, info.GetType().GetProperty(ormProperty.PropertyName).PropertyType,
                        query);
                    info.GetType().GetProperty(ormProperty.PropertyName).SetValue(info, proxy, null);
                    if (isExcuteLazy)
                    {
                        Newtonsoft.Json.JsonConvert.SerializeObject(proxy);
                        break;
                    }
                }
            }
            else
            {
                var routeInfos = GetRemoteInfos(infos, remoteQuery, ormProperty);
                SetRemoteProperies(infos, routeInfos, ormProperty);
            }
        }
        /// <summary>
        /// 得到远程查询实体
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="propertyName"></param>
        /// <param name="obj"></param>
        /// <param name="ormProperty"></param>
        /// <returns></returns>
        protected virtual IEnumerable<EntityInfo> GetRemoteEntities(IEnumerable<EntityInfo> infos, string propertyName, OrmObjectInfo obj, out OrmPropertyInfo ormProperty)
        {
            if (!propertyName.Contains("."))
            {
                ormProperty = obj.GetPropertyInfo(propertyName);
                return infos;
            }
            var name = propertyName.Substring(0, propertyName.IndexOf("."));
            ormProperty = obj.GetPropertyInfo(name);
            propertyName = propertyName.Substring(propertyName.IndexOf(".") + 1,
                                                  propertyName.Length - propertyName.IndexOf(".") - 1);
            if (ormProperty.Map == null) return infos;
            var result = new List<EntityInfo>();
            foreach (var info in infos)
            {
                if (ormProperty.Map.MapType == OrmMapType.OneToOne)
                {
                    var t = info.GetProperty(name) as EntityInfo;
                    if(t==null) continue;
                    result.Add(t);
                }
                else if (ormProperty.Map.MapType == OrmMapType.OneToMany)
                {
                    var ts = info.GetProperty(name) as IEnumerable<EntityInfo>;
                    if(ts==null) continue;
                    result.AddRange(ts);
                }
            }
            if (!string.IsNullOrEmpty(propertyName))
            {
                return GetRemoteEntities(result, propertyName, ormProperty.Map.GetMapObject(), out ormProperty);
            }
            return result;
        }

        /// <summary>
        /// 得到延迟的加载条件
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="remoteQuery"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual IList<EntityInfo> GetRemoteInfos(IEnumerable<EntityInfo> infos, RemoteQueryInfo remoteQuery,
                                                           OrmPropertyInfo property)
        {
            var query = GetRemoteQuery(infos, remoteQuery, property);
            var remoteInfos = GetInfos<IList<EntityInfo>>(property.Map.GetMapObject(), query);
            return remoteInfos;
        }
        /// <summary>
        /// 得到远程查询条件
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="remoteQuery"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual QueryInfo GetRemoteQuery(IEnumerable<EntityInfo> infos, RemoteQueryInfo remoteQuery, OrmPropertyInfo property)
        {
            var query = new QueryInfo { SelectExp = remoteQuery.SelectExp,Parameters = remoteQuery.Parameters,OrderByExp=remoteQuery.OrderByExp};
            var pName = "_Ids";
            var ids =
                (from object info in infos select info.GetProperty(property.Map.ObjectProperty.PropertyName)).ToArray();
            query.Where(string.Format("@{0}.Contains({1})", pName, property.Map.MapObjectProperty.PropertyName))
                 .SetParameter(pName, ids);
            if (!string.IsNullOrEmpty(remoteQuery.WhereExp))
            {
                query.WhereExp = string.Format("{0} && {1}",query.WhereExp, remoteQuery.WhereExp);
            }
            if (!string.IsNullOrEmpty(query.SelectExp))
                query.SelectExp = string.Format("{0},{1}", property.Map.MapObjectProperty.PropertyName,
                                                query.SelectExp);
            return query;
        }

        /// <summary>
        /// 设置延迟属性
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="remoteInfos"></param>
        /// <param name="property"></param>
        protected virtual void SetRemoteProperies(IEnumerable<EntityInfo> infos, IList<EntityInfo> remoteInfos,
                                                  OrmPropertyInfo property)
        {
            if (remoteInfos == null || remoteInfos.Count == 0)
                return;
            if (property.Map.MapType == OrmMapType.OneToOne)
            {
                foreach (var info in infos)
                {
                    var id = info.GetProperty(property.Map.ObjectProperty.PropertyName);
                    if(id==null) continue;
                    foreach (var entityInfo in remoteInfos)
                    {
                        if (id.Equals(entityInfo.GetProperty(property.Map.MapObjectProperty.PropertyName)))
                        {
                            var redundanceProperties = property.GetObject().Properties.Where(it => it.PropertyName.Contains($"{property.PropertyName}.") && it.PropertyName != property.Map.ObjectProperty.PropertyName);
                            if (entityInfo != null && redundanceProperties.Count() > 0)
                            {
                                var redundanceObject = info.GetProperty(property.PropertyName);
                                if (redundanceObject == null)
                                    continue;
                                foreach (var p in redundanceProperties)
                                {
                                
                                    var pName = p.PropertyName.Replace($"{property.PropertyName}.", "");
                                    entityInfo.SetProperty(pName, redundanceObject.GetProperty(pName));
                                }
                            }
                            info.SetProperty(property.PropertyName, entityInfo);
                           
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (var info in infos)
                {
                    var id = info.GetProperty(property.Map.ObjectProperty.PropertyName);
                    if (id == null) continue;
                    var arrayList = new ArrayList();
                    foreach (var entityInfo in remoteInfos)
                    {
                        if (id.Equals(entityInfo.GetProperty(property.Map.MapObjectProperty.PropertyName)))
                        {
                            arrayList.Add(entityInfo);
                        }
                    }
                    var type = Type.GetType(property.Map.GetMapObject().ObjectName);
                    if (type == null) continue;
                    info.SetProperty(property.PropertyName, arrayList.ToArray(type));
                }
            }
        }

        #endregion
        #endregion

        /// <summary>
        /// 存储
        /// </summary>
        /// <param name="info"></param>
        /// <param name="unitOfWorks"></param>
        /// <returns></returns>
        public virtual IList<IUnitofwork> Save(SaveInfo info, IList<IUnitofwork> unitOfWorks=null)
        {
            IDictionary<OrmDataBaseInfo, SaveInfo> dbs = GetOrmDataBases(info);
            return GetUnitofworks(dbs, unitOfWorks);
        }

   

        /// <summary>
        /// 执行查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual T ExecuteQuery<T>(string name, string commandText, CommandType commandType, params object[] parameters)
        {
            var db = DataBase.GetDataBase(name).GetAllGetOrmDataBase().FirstOrDefault();
            var compiler = DataBase.GetCompiler(db);
            return compiler.ExecuteQuery<T>(db, commandText,commandType, parameters);
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="name"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual int ExecuteCommand(string name, string commandText, CommandType commandType, params object[] parameters)
        {
            var db = DataBase.GetDataBase(name).GetAllSetOrmDataBase().FirstOrDefault();
            var compiler = DataBase.GetCompiler(db);
            return compiler.ExecuteCommand(db, commandText, commandType,parameters);
        }

        #endregion

        #region 查询操作

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        protected virtual T ExecuteQuery<T>(QueryInfo query, OrmObjectInfo obj, OrmDataBaseInfo db)
        {
            var compiler = DataBase.GetCompiler(db);
            return compiler.GetInfos<T>(db, obj, query);
        }



        #endregion

        #region  存储操作

        /// <summary>
        /// 设置DB信息
        /// </summary>
        /// <param name="info"></param>
        protected virtual IDictionary<OrmDataBaseInfo, SaveInfo> GetOrmDataBases(SaveInfo info)
        {
            IDictionary<OrmDataBaseInfo, SaveInfo> result = new Dictionary<OrmDataBaseInfo, SaveInfo>();
            OrmObjectInfo obj = info.Object;
            if (!string.IsNullOrEmpty(obj.RouteName))
            {
                DbRoute.SetRouteSaveInfo(info);
            }
            var db = DataBase.GetDataBase(info.SetDataBase).GetAllSetOrmDataBase().FirstOrDefault();
            result.Add(db, info);
            return result;
        }

        /// <summary>
        /// 得到事务
        /// </summary>
        /// <param name="dbs"></param>
        /// <param name="unitOfWorks"></param>
        /// <returns></returns>
        protected virtual IList<IUnitofwork> GetUnitofworks(IDictionary<OrmDataBaseInfo, SaveInfo> dbs, IList<IUnitofwork>  unitOfWorks)
        {
            unitOfWorks =unitOfWorks?? new List<IUnitofwork>();
            foreach (var db in dbs)
            {
                var compiler = DataBase.GetCompiler(db.Key);
                compiler.AddUnitofwork(db.Key,db.Value, unitOfWorks);
            }
            return unitOfWorks;
        }

 


        #endregion


    }
}
