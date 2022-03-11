using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Winner.Cache;
using Winner.Log;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.ContextStorage;
using Winner.Persistence.Relation;
using Winner.Persistence.Translation;
using Winner.Persistence.Works;

namespace Winner.Persistence
{
    /// <summary>
    /// 执行上下文
    /// </summary>
    [Serializable]
    public class Context : IContext
    {
        /// <summary>
        /// 执行实例
        /// </summary>
        public IExecutor Executor { get; set; }

        /// <summary>
        /// 事务实例
        /// </summary>
        public ITransaction Transaction { get; set; }

        /// <summary>
        /// Orm实例
        /// </summary>
        public IOrm Orm { get; set; }

        /// <summary>
        /// Orm实例
        /// </summary>
        public ICache Cacher { get; set; }

        /// <summary>
        /// ContextStorage实例
        /// </summary>
        public IContextStorage ContextStorage { get; set; }
        /// <summary>
        /// 存储对象
        /// </summary>
        public ContextInfo Local
        {
            get
            {
                var rev = ContextStorage.Get();
                if (rev == null)
                {
                    rev = new ContextInfo();
                    ContextStorage.Set(rev);
                }
                return rev;

            }
            set { ContextStorage.Set(value); }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize()
        {
            var objs = Orm.GetOrms().Values;
            objs = objs.Distinct().ToList();
            foreach (var obj in objs)
            {
                if (obj.CacheType== CacheType.None)
                    continue;
                var task = new Thread(() => { LoadCache(obj); });
                task.Start();
            }
        }

        public delegate void LoadCacheHandle(OrmObjectInfo obj);

        /// <summary>
        /// 加载缓存
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void LoadCache(OrmObjectInfo obj)
        {
            for (int i = 0;; i++)
            {
                var query = new QueryInfo {IsGreedyLoad = true, IsLazyLoad = true};
                query.From(obj.ObjectName).SetPageSize(1000).SetPageIndex(i);
                var entities = ExecuteInfos<IList<EntityInfo>>(obj, query,true);
                if (entities == null || entities.Count == 0)
                    break;
                foreach (var entity in entities)
                {
                    var value = entity.GetType().GetProperty(obj.PrimaryProperty.PropertyName).GetValue(entity, null);
                    var cacheKey = GetEntityCacheKey(obj, value);
                    var dataEntity = GetCommonCache<EntityInfo>(obj,cacheKey);
                    if (dataEntity != null)
                    {
                        InsertCommonCache(obj, value, entity);
                    }
                }
            }
        }

        /// <summary>
        /// 得到缓存值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        protected virtual string GetEntityCacheKey(OrmObjectInfo obj, object key)
        {
            return EncryptMd5(string.Format("{0}{1}", obj.ObjectName, key)) ;
        }
        /// <summary>
        /// 得到缓存值
        /// </summary>
        /// <param name="key"></param>
        protected virtual string GetVersionCacheKey(object key)
        {
            return EncryptMd5(string.Format("{0}Version", key));
        }
      
        /// <summary>
        /// 得到实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(object key) where T: EntityInfo
        {
            var type = typeof(T);
            if (key == null || string.IsNullOrWhiteSpace(key.GetType().FullName)) return default(T);
            if (key.GetType().FullName.Equals(type.FullName))
            {
                if (Local.HasStorage(key)) return (T) Local.Storages[key].Entity;
                return default(T);
            }
            var obj = Orm.GetOrm(type.FullName);
            string cacheKey = GetEntityCacheKey(obj, key);
            lock (Local.Locker)
            {
                if (Local.HasEntity(cacheKey))
                {
                    return (T)Local.GetEntity(cacheKey);
                }
            }
            object entity = null;
            if (obj.CacheType != CacheType.None)
            {
                entity = GetCommonCache<T>(obj, cacheKey);
            }
            if (entity == null)
            {
                var query = new QueryInfo { IsLazyLoad = true }.From(type.FullName);
                query.Where(string.Format("{0}==@{0}", obj.PrimaryProperty.PropertyName))
                     .SetParameter(obj.PrimaryProperty.PropertyName, key);
                var entities = ExecuteInfos<IList<T>>(obj, query, obj.CacheType != CacheType.None);
                if (entities != null)
                {
                    entity = entities.FirstOrDefault();
                    InsertCommonCache(obj, key, entity);
                }

            }
            lock (Local.Locker)
            {
                if (!Local.HasEntity(cacheKey))
                {
                    Local.Entities.Add(cacheKey, entity);
                }
            }
            return (T)entity;
        }

     


        /// <summary>
        /// 实在实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        protected virtual SaveInfo Set<T>(T entity)where T: EntityInfo
        {
            var obj = Orm.GetOrm(entity.GetType().FullName);
            if (entity.SaveType == SaveType.Modify && string.IsNullOrEmpty(entity.WhereExp) &&
                obj.VersionProperty != null)
            {
                var dics=new Dictionary<string,string>();
                if (entity.Properties != null)
                {
                    foreach (var property in entity.Properties)
                    {
                        dics.Add(property,property);
                    }
                }
                var hasLocker =
                    obj.Properties.Any(
                        it =>
                        it.IsOptimisticLocker &&
                        (entity.Properties == null || dics.ContainsKey(it.PropertyName)));
                if (hasLocker)
                {
                    var key = entity.GetProperty(obj.PrimaryProperty.PropertyName);
                    var value = Get<T>(key);
                    if (value != null)
                    {
                        var version = value.GetProperty(obj.VersionProperty.PropertyName);
                        entity.SetProperty(obj.VersionProperty.PropertyName, version);
                    }
                }
            }
            if (!Local.HasStorage(entity))
            {
                Local.Storages.Add(entity, new SaveInfo { Entity = entity, Object = obj });
            }
            return Local.Storages[entity];
        }






     
        /// <summary>
        /// 存储对象
        /// </summary>
        /// <returns></returns>
        public virtual IList<IUnitofwork> Save<T>(T entity) where T : EntityInfo
        {
            lock (Local.Locker)
            {
                var units = new List<IUnitofwork>();
                if (entity.SaveType == SaveType.None)
                    return units;
                var saveInfo = Set(entity);
                var count = Local.Unitofworks.Count;
                Executor.Save(saveInfo, Local.Unitofworks);
                for (int i = count; i < Local.Unitofworks.Count; i++)
                {
                    units.Add(Local.Unitofworks[i]);
                }
                return units;
            }
        }

  
        /// <summary>
        /// 存储对象
        /// </summary>
        /// <returns></returns>
        public virtual IList<IUnitofwork> Save<T>(IList<T> entities) where T : EntityInfo
        {
            lock (Local.Locker)
            {
                var count = Local.Unitofworks.Count;
                foreach (var entity in entities)
                {
                    if (entity.SaveType == SaveType.None)
                        continue;
                    var saveInfo = Set(entity);
                    Executor.Save(saveInfo, Local.Unitofworks);
                }
                var units = new List<IUnitofwork>();
                for (int i = count; i < Local.Unitofworks.Count; i++)
                {
                    units.Add(Local.Unitofworks[i]);
                }
                return units;
            }
          
        }

        #region 公共缓存
        /// <summary>
        /// 设置公共缓存
        /// </summary>
        protected virtual T GetCommonCache<T>(OrmObjectInfo obj, string key)
        {
            if (obj.CacheType == CacheType.None)
                return default(T);
            if (obj.CacheType == CacheType.Local)
                return GetLocalCache<T>(key);
            if (obj.CacheType == CacheType.Remote)
                return GetRemoteCache<T>(key);
            if (obj.CacheType == CacheType.LocalAndRemote)
            {
                var versionKey = GetVersionCacheKey(key);
                var remoteVersion = GetRemoteCache<string>(versionKey);
                var localVersion = GetLocalCache<string>(versionKey);
                if (!string.IsNullOrWhiteSpace(remoteVersion) && localVersion == remoteVersion)
                {
                    return GetLocalCache<T>(key);
                }
            }

            if (obj.CacheType == CacheType.LocalAndRemoteDelayCheck)
            {
                var result= GetLocalCache<T>(key);
                if (result != null)
                {
                    var versionKey = GetVersionCacheKey(key);
                    AppendVersionCache(new VersionCacheInfo
                    {
                        CheckTimeSpan = obj.CheckCacheTimeSpan,
                        LastCheckTime = DateTime.Now,
                        LocalKey = key,
                        VersionKey = versionKey,
                        RemoveTime = DateTime.Now.AddSeconds(obj.CacheTime)
                    });
                }
                return result;
            }
            return default(T);
        }
        /// <summary>
        /// 设置公共缓存
        /// </summary>
        protected virtual void InsertCommonCache(OrmObjectInfo obj,object key,object entity)
        {
            if (obj.CacheType == CacheType.None|| entity==null)
                return;
            var cacheKey = GetEntityCacheKey(obj, key);
            if (obj.CacheType == CacheType.Local)
            {
                SetLocalCache(cacheKey, entity, obj.CacheTime);
            }
            else if (obj.CacheType == CacheType.Remote)
            {
                SetRemoteCache(cacheKey, entity, obj.CacheTime);
            }
            else if(obj.CacheType== CacheType.LocalAndRemote || obj.CacheType == CacheType.LocalAndRemoteDelayCheck)
            {
                var versionCacheKey = GetVersionCacheKey(cacheKey);
                var version = GetRemoteCache<string>(versionCacheKey);
                if (string.IsNullOrWhiteSpace(version))
                {
                    version = DateTime.Now.ToString("yyyyMMddHHmmss");
                    SetRemoteCache(versionCacheKey, version, obj.CacheTime);
                }
                SetLocalCache(versionCacheKey, version, obj.CacheTime);
                SetLocalCache(cacheKey, entity, obj.CacheTime);
                if (obj.CacheType == CacheType.LocalAndRemoteDelayCheck)
                {
                    AppendVersionCache(new VersionCacheInfo
                    {
                        CheckTimeSpan = obj.CheckCacheTimeSpan,
                        LastCheckTime=DateTime.Now,
                        LocalKey = cacheKey,
                        VersionKey=versionCacheKey,
                        RemoveTime=DateTime.Now.AddSeconds(obj.CacheTime)
                    });
                }
            }
            var ormProperties =
             obj.Properties.Where(it => it.Map != null && (it.Map.IsGreedyLoad || it.Map.IsLazyLoad));
            foreach (var ormProperty in ormProperties)
            {
                var mapValue = entity.GetProperty(ormProperty.PropertyName);
                if (ormProperty.Map.MapType == OrmMapType.OneToMany)
                {
                    if(mapValue==null)continue;
                    var values = mapValue as IEnumerable<EntityInfo>;
                    if(values==null) continue;
                    foreach (var value in values)
                    {
                        InsertCommonCache(ormProperty.Map.GetMapObject(), value.GetProperty(ormProperty.Map.GetMapObject().PrimaryProperty.PropertyName), value);    
                    }
                }
                else
                {
                    InsertCommonCache(ormProperty.Map.GetMapObject(), mapValue.GetProperty(ormProperty.Map.GetMapObject().PrimaryProperty.PropertyName), mapValue); 
                }
            }
        }
        /// <summary>
        /// 移除公共缓存
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        protected virtual void RemoveCommonCache(OrmObjectInfo obj, object key)
        {

            var cacheKey = GetEntityCacheKey(obj, key);
            object cacheValue = null;
            if (obj.CacheType == CacheType.Local)
            {
                cacheValue = GetLocalCache(cacheKey, Type.GetType(obj.ObjectName));
                if (cacheValue == null) return;
                RemoveLocalCache(cacheKey);
            }
            else if (obj.CacheType == CacheType.Remote)
            {
                cacheValue = GetRemoteCache(cacheKey, Type.GetType(obj.ObjectName));
                if (cacheValue == null) return;
                RemoveRemoteCache(cacheKey);
            }
            else if (obj.CacheType == CacheType.LocalAndRemote || obj.CacheType== CacheType.LocalAndRemoteDelayCheck)
            {
                var varsionKey = GetVersionCacheKey(cacheKey);
                RemoveRemoteCache(varsionKey);
                RemoveLocalCache(varsionKey);
                cacheValue = GetLocalCache(cacheKey, Type.GetType(obj.ObjectName));
                if (cacheValue == null) return;
                RemoveLocalCache(cacheKey);
            }
            var ormMaps =
            obj.Properties.Where(it => it.Map != null && it.Map.IsRemoveCache)
               .Select(it => it.Map).ToList();
            if (ormMaps.Count == 0) return;
            foreach (var ormMap in ormMaps)
            {
                var value = cacheValue.GetProperty(ormMap.ObjectProperty.PropertyName);
                RemoveCommonCache(ormMap.GetMapObject(), value);
            }
        }
        #endregion

        #region 提交

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="unitofworks"></param>
        /// <returns></returns>
        public virtual bool Commit(IList<IUnitofwork> unitofworks)
        {
            lock (Local.Locker)
            {
                unitofworks = unitofworks == null && Local?.Unitofworks != null
                    ? new List<IUnitofwork>(Local.Unitofworks)
                    : unitofworks;
                unitofworks = unitofworks.OrderByDescending(it => it.Sequence).ToList();
                try
                {
                    var rev = Transaction.Commit(unitofworks);
                    if (rev)
                    {
                        RemoveUnitofworks(unitofworks, true);
                    }

                    return rev;

                }
                catch (Exception e)
                {
                    RemoveUnitofworks(unitofworks, false);
                    throw e;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitofworks"></param>
        /// <param name="rev"></param>
        protected virtual void RemoveUnitofworks(IList<IUnitofwork> unitofworks,bool rev)
        {

            foreach (var unitofwork in unitofworks)
            {
                if (Local?.Unitofworks != null)
                    Local.Unitofworks.Remove(unitofwork);
                var storages = unitofwork.GetObject() as IList<SaveInfo>;
                if (storages == null)
                    continue;
                foreach (var info in storages)
                {
                    if (rev)
                    {
                        if (info.Object.IsCacheDependency || info.Object.CacheType != CacheType.None ||
                            info.Object.Properties.Count(it => it.Map != null && it.Map.IsRemoveCache) > 0)
                        {
                            var task = new Thread(() => { FlushCache(info); });
                            task.Start();
                        }
                    }
                    if (Local?.Storages?.ContainsKey(info.Entity) == true)
                        Local.Storages.Remove(info.Entity);
                }
            }
            if (Local?.Entities != null)
                Local.Entities.Clear();
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        protected virtual void FlushCache(SaveInfo info)
        {
          
            if (info.Entity.SaveType == SaveType.None)
                return;
            if (info.Object.IsCacheDependency)
            {
                var varsionKey = GetVersionCacheKey(info.Entity.GetType().FullName);
                var i = 0;
                while (i<5)
                {
                    var rev=RemoveRemoteCache(varsionKey);
                    if(rev)
                        break;
                    i++;
                }
                if(i>=5)
                {
                    Log.AddException(new Exception($"FlushCache Error:{info.Entity.GetType().FullName}"));
                }
            }
            if (info.Object.CacheType == CacheType.None)
                return;
            var id = info.Entity.GetProperty(info.Object.PrimaryProperty.PropertyName);
            if (id == null || id.GetType().IsValueType && id.Equals(0))
                return;
            RemoveCommonCache(info.Object, id);
            var ormMaps =
                info.Object.Properties.Where(it => it.Map != null && it.Map.IsRemoveCache)
                    .Select(it => it.Map).ToList();
            if (ormMaps.Count > 0)
            {
                foreach (var ormMap in ormMaps)
                {
                    var key = info.Entity.GetProperty(ormMap.ObjectProperty.PropertyName);
                    if (key == null || key.GetType().IsValueType && key.Equals(0))
                        continue;
                    RemoveCommonCache(ormMap.GetMapObject(), key);
                }
            }
        }
        #endregion

        #region 查询

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual T GetInfos<T>(QueryInfo query)
        {
            if (query.PageSize < 0 || query.PageIndex < 0)
                return default(T);
            var cacheKey = query.IsSetLocalStorage? CreateQueryCustomCacheKey(query):null;
            var obj = Orm.GetOrm(query.FromExp);
            if (query.IsSetLocalStorage)
            {
             
                lock (Local.Locker)
                {
                    if (Local.HasEntity(cacheKey))
                    {
                        return (T)Local.GetEntity(cacheKey);
                    }
                }
            }
            var infos = ExecuteInfos<T>(obj, query);
            lock (Local.Locker)
            {
                if (query.IsSetLocalStorage)
                {
                    if (!Local.HasEntity(cacheKey))
                    {
                        Local.Entities.Add(cacheKey, infos);
                    }
                }
              
            }
            return infos;
        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteQuery<T>(string name, string commandText, CommandType commandType, params object[] parameters)
        {
            return Executor.ExecuteQuery<T>(name, commandText, commandType, parameters);
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="name"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteCommand(string name, string commandText, CommandType commandType,
                                  params object[] parameters)
        {
            return Executor.ExecuteCommand(name, commandText, commandType, parameters);
        }

        #endregion

        #region 自定义缓存或DB加载

       
        /// <summary>
        /// 缓存值
        /// </summary>
        private static readonly object KeyLoker = new object();



        /// <summary>
        /// 加载数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="isLazyLoadExecute"></param>
        /// <returns></returns>
        protected virtual T ExecuteInfos<T>(OrmObjectInfo obj, QueryInfo query, bool isLazyLoadExecute = false)
        {
            if (query.Cache != null)
                return GetInfosByCache<T>(query, obj);
            var result = Executor.GetInfos<T>(obj, query,isLazyLoadExecute);

            return result;
        }

        /// <summary>
        /// 从缓存中查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual T GetInfosByCache<T>(QueryInfo query, OrmObjectInfo obj)
        {
            SetQueryCustomCacheKey(query, obj);
            QueryCacheInfo<T> cacheResult = GetQueryCache<T>(obj,query);
            query.Cache.IsHit = true;
            if (null == cacheResult || cacheResult.Result == null)
            {
                lock (KeyLoker)
                {
                    if (null == cacheResult || cacheResult.Result == null)
                    {
                        cacheResult = new QueryCacheInfo<T>
                        {
                            Result = Executor.GetInfos<T>(obj, query, true),
                            DataCount = query.DataCount
                        };
                        SetQueryCache(obj, query, cacheResult);
                        query.Cache.IsHit = false;
                    }
                }
            }
            query.DataCount = cacheResult.DataCount;
            return cacheResult.Result;
        }

        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual QueryCacheInfo<T> GetQueryCache<T>(OrmObjectInfo obj, QueryInfo query)
        {
            if (query.Cache.Type == CacheType.None)
                return null;
            if (query.Cache.Time == DateTime.MinValue.ToUniversalTime() && query.Cache.TimeSpan == 0)
                return null;
            if (query.Cache.Type == CacheType.LocalAndRemote && query.Cache.Dependencies != null && query.Cache.Dependencies.Count > 0)
            {
                foreach (var dependency in query.Cache.Dependencies)
                {
                    string subVersionKey = GetVersionCacheKey(dependency);
                    var subLocalVersion = GetLocalCache<string>(subVersionKey);
                    var subRemoteVersion = GetRemoteCache<string>(subVersionKey);
                    if (string.IsNullOrWhiteSpace(subRemoteVersion) || subLocalVersion != subRemoteVersion)
                    {
                        return null;
                    }
                }

            }
            if (query.Cache.Type == CacheType.LocalAndRemoteDelayCheck && query.Cache.Dependencies != null && query.Cache.Dependencies.Count>0)
            {
                return GetLocalCache<QueryCacheInfo<T>>(query.Cache.Key);
            }
            if (query.Cache.Type == CacheType.Local || query.Cache.Type == CacheType.LocalAndRemote || query.Cache.Type == CacheType.LocalAndRemoteDelayCheck)
            {
                return GetLocalCache<QueryCacheInfo<T>>(query.Cache.Key);
            }
            if (query.Cache.Type == CacheType.Remote)
            {
                return GetRemoteCache<QueryCacheInfo<T>>(query.Cache.Key);
            }
            return null;

        }

        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual void SetQueryCache<T>(OrmObjectInfo obj, QueryInfo query, QueryCacheInfo<T> cacheResult)
        {
            if (query.Cache.Type == CacheType.None)
                return ;
            if (query.Cache.Time == DateTime.MinValue.ToUniversalTime() && query.Cache.TimeSpan == 0)
                return ;
            if (query.Cache.Type == CacheType.LocalAndRemote)
            {
                if (query.Cache.Dependencies != null && query.Cache.Dependencies.Count > 0)
                {
                    foreach (var dependency in query.Cache.Dependencies)
                    {
                        string subVersionKey = GetVersionCacheKey(dependency);
                        SetSetQueryCacheVersion(query, dependency, subVersionKey);

                    }
                }
            }
            if (query.Cache.Type == CacheType.LocalAndRemoteDelayCheck)
            {
                if (query.Cache.Dependencies != null && query.Cache.Dependencies.Count > 0)
                {
                    foreach (var dependency in query.Cache.Dependencies)
                    {
                        string subVersionKey = GetVersionCacheKey(dependency);
                        SetSetQueryCacheVersion(query, dependency, subVersionKey);
                        AppendVersionCache(new VersionCacheInfo
                        {
                            VersionKey = subVersionKey,
                            CheckTimeSpan = query.Cache.CheckTimeSpan,
                            LocalKey = query.Cache.Key,
                            LastCheckTime = DateTime.Now,
                            RemoveTime= query.Cache.TimeSpan==0? query.Cache.Time:DateTime.Now.AddSeconds(query.Cache.TimeSpan)
                        });
                    }
                }
            }
            if (query.Cache.Type == CacheType.Local || query.Cache.Type == CacheType.LocalAndRemote || query.Cache.Type == CacheType.LocalAndRemoteDelayCheck)
            {
                if (query.Cache.TimeSpan != 0)
                {
                    SetLocalCache(query.Cache.Key, cacheResult, query.Cache.TimeSpan);
                }
                else if (query.Cache.Time != DateTime.MinValue.ToUniversalTime())
                {
                    SetLocalCache(query.Cache.Key, cacheResult, query.Cache.Time);
                }
            }
            if (query.Cache.Type == CacheType.Remote)
            {
                if (query.Cache.TimeSpan != 0)
                {
                    SetRemoteCache(query.Cache.Key, cacheResult, query.Cache.TimeSpan);
                }
                else if (query.Cache.Time != DateTime.MinValue.ToUniversalTime())
                {
                    SetRemoteCache(query.Cache.Key, cacheResult, query.Cache.Time);
                }
            }
        }

        protected virtual void SetSetQueryCacheVersion(QueryInfo query, string dependency, string subVersionKey)
        {
           
            var version = GetRemoteCache<string>(subVersionKey);
            if (string.IsNullOrWhiteSpace(version))
            {
                version = DateTime.Now.ToString("yyyyMMddHHmmss");
                if (query.Cache.TimeSpan != 0)
                {
                    SetRemoteCache(subVersionKey, version, query.Cache.TimeSpan);
                }
                else if (query.Cache.Time != DateTime.MinValue.ToUniversalTime())
                {
                    SetRemoteCache(subVersionKey, version, query.Cache.Time);
                }

            }
            if (query.Cache.TimeSpan != 0)
            {
                SetLocalCache(subVersionKey, version, query.Cache.TimeSpan);
            }
            else if (query.Cache.Time != DateTime.MinValue.ToUniversalTime())
            {
                SetLocalCache(subVersionKey, version, query.Cache.Time);
            }
        }
     

        
        /// <summary>
        /// 得到缓存的Key
        /// </summary>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual void SetQueryCustomCacheKey(QueryInfo query, OrmObjectInfo obj)
        {
            var value = CreateQueryCustomCacheKey(query);
            query.Cache.Key = value;//string.Format("{0}{1}", query.Cache.Name, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual string CreateQueryCustomCacheKey(QueryInfo query)
        {
            var sb = new StringBuilder();
            sb.Append(query.SelectExp);
            sb.Append(query.FromExp);
            sb.Append(query.WhereExp);
            sb.Append(query.GroupByExp);
            sb.Append(query.HavingExp);
            sb.Append(query.OrderByExp);
            sb.Append(query.PageIndex);
            sb.Append(query.PageSize);
            if (query.Parameters != null)
            {
                foreach (var p in query.Parameters)
                {
                    sb.Append(p.Key);
                    var keys = p.Value as Array;
                    if (keys != null)
                    {
                        foreach (var key in keys)
                        {
                            sb.Append(key);
                        }
                    }
                    else
                    {
                        sb.Append(p.Value);
                    }
                }
            }
            var value = EncryptMd5(sb.ToString());
            return value;
        }
        /// <summary>
        /// 得到MD5加密
        /// </summary>
        /// <returns></returns>
        public virtual string EncryptMd5(string input)
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


     
        #endregion

        #region 缓存
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        protected virtual bool SetRemoteCache<T>(string key, T value, DateTime time)
        {
            return Cacher.Set(key, value, time);
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        protected virtual bool SetRemoteCache<T>(string key, T value, long timeSpan)
        {
            return Cacher.Set(key, value, timeSpan);
        }
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual T GetRemoteCache<T>(string key)
        {
            var value= Cacher.Get<T>(key);
            return value;
        }
        /// <summary>
        /// 得到对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object GetRemoteCache(string key, Type type)
        {
            
            return Cacher.Get(key, type);
        }
        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public virtual bool RemoveRemoteCache(string key)
        {
            return Cacher.Remove(key);
 
        }
        private static readonly MemoryCache CacheInstance = new MemoryCache(new MemoryCacheOptions());
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        protected virtual bool SetLocalCache<T>(string key, T value, DateTime time)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                return false;
            CacheInstance.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = time
            });
            return true;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        protected virtual bool SetLocalCache<T>(string key, T value, long timeSpan)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                return false;
            CacheInstance.Set(key, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(timeSpan)
            });
            return true;
        }
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual T GetLocalCache<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return default(T);
            object value = null;
            if (CacheInstance.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return default(T);
        }
        /// <summary>
        /// 得到对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object GetLocalCache(string key, Type type)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            object value = null;
            if (CacheInstance.TryGetValue(key, out value))
            {
                return Convert.ChangeType(value, type);
            }
            return null;
        }
        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public virtual bool RemoveLocalCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            CacheInstance.Remove(key);
            return true;
        }

        #endregion

        private static IDictionary<string,VersionCacheInfo> VersionCaches { get; set; }=new Dictionary<string, VersionCacheInfo>();
        private static object VersionCacheLocker = new object();
        public Context()
        {
            var task=new Thread(() =>
                {
                    CheckVersionCache();
                });
            task.Start();
        }
        private ILog _log;

        /// <summary>
        /// 实例
        /// </summary>
        public ILog Log
        {
            get
            {
                if (_log == null)
                    _log = Creator.Get<ILog>();
                return _log;
            }
            set { _log = value; }
        }
        protected virtual void CheckVersionCache()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(30000);
                    var keys = new List<string>();
                    var values = VersionCaches.Values.ToArray();
                    foreach (var cacheInfo in values)
                    {
                        if ((DateTime.Now - cacheInfo.LastCheckTime).TotalSeconds < cacheInfo.CheckTimeSpan)
                            continue;
                        cacheInfo.LastCheckTime = DateTime.Now;
                        string subVersionKey = cacheInfo.VersionKey;
                        var subLocalVersion = GetLocalCache<string>(subVersionKey);
                        var subRemoteVersion = GetRemoteCache<string>(subVersionKey);
                        if (!string.IsNullOrWhiteSpace(subRemoteVersion) && subRemoteVersion == subLocalVersion)
                            continue;
                        RemoveLocalCache(subVersionKey);
                        RemoveLocalCache(cacheInfo.LocalKey);
                        keys.Add(cacheInfo.LocalKey);
                    }
                    foreach (var key in keys)
                    {
                        var cacheInfo = VersionCaches[key];
                        RemoveVersionCache(cacheInfo);
                    }
                    keys = VersionCaches.Keys.ToList();
                    foreach (var key in keys)
                    {
                        var cacheInfo = VersionCaches[key];
                        if (DateTime.Now < cacheInfo.RemoveTime)
                            continue;
                        RemoveVersionCache(cacheInfo);
                    }
                }
                catch (Exception ex)
                {
                    Log.AddException(new Exception($"CheckVersionCache:{ex.Message}"));
                }
            }
        }

        protected virtual void AppendVersionCache(VersionCacheInfo versionCache)
        {
            lock (VersionCacheLocker)
            {
                if (VersionCaches.ContainsKey(versionCache.LocalKey))
                {
                    VersionCaches[versionCache.LocalKey].RemoveTime = versionCache.RemoveTime;
                    return;
                }
                VersionCaches.Add(versionCache.LocalKey, versionCache);
            }
        }

        protected virtual void RemoveVersionCache(VersionCacheInfo versionCache)
        {
            if (!VersionCaches.ContainsKey(versionCache.LocalKey))
                return;
            lock (VersionCacheLocker)
            {
                if (!VersionCaches.ContainsKey(versionCache.LocalKey))
                    return;
                VersionCaches.Remove(versionCache.LocalKey);
            }
        }
    }
}
