using System;
using System.Collections;
using System.Collections.Generic;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Relation;
using Winner.Persistence.Translation;
using System.Reflection;
namespace Winner.Persistence.Delay
{


    public class Proxy : DispatchProxy
    {
        #region 属性
        /// <summary>
        /// 实例
        /// </summary>
        public object Target { get; set; }
        /// <summary>
        /// 实体
        /// </summary>
        public IDictionary<object, object> EntitityKeys { get; set; }
        /// <summary>
        /// 映射关系
        /// </summary>
        public OrmPropertyInfo OrmProperty { get; set; }
        /// <summary>
        /// 上下文
        /// </summary>
        public IExecutor Executor { get; set; }
        /// <summary>
        /// 加载查询
        /// </summary>
        public QueryInfo Query { get; set; }
        /// <summary>
        /// 是否立即执行
        /// </summary>
        public bool IsLazyLoadExecute { get; set; }
        /// <summary>
        /// 是否初始化
        /// </summary>
        protected bool IsInitialize { get; set; }

        public Type Type { get; set; }
        #endregion

        #region 构造函数

        public Proxy()
        {

        }
        /// <summary>
        /// 对象
        /// </summary>
        /// <param name="entitityKeys"></param>
        /// <param name="ormProperty"></param>
        /// <param name="context"></param>
        /// <param name="executor"></param>
        /// <param name="isLazyLoadExecute"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="query"></param>
        public virtual void Set(IDictionary<object, object> entitityKeys, OrmPropertyInfo ormProperty,IExecutor executor,bool isLazyLoadExecute, object target, Type type,QueryInfo query)
        {
            EntitityKeys = entitityKeys;
            Target = target;
            OrmProperty = ormProperty;
            Executor = executor;
            IsLazyLoadExecute = isLazyLoadExecute;
            IsInitialize = false;
            Type = type;
            Query = query;
        }
        #endregion

        #region 接口的实现
        /// <summary>
        /// 得到代理
        /// </summary>
        /// <returns></returns>
        public static object Create(Type type)
        {
            MethodInfo mi = typeof(DispatchProxy).GetMethod("Create")
                .MakeGenericMethod(new Type[] { type, typeof(Proxy) });
            return mi.Invoke(null, null);
        }


        #endregion

        #region 延迟调用
        /// <summary>
        /// 拦截方法
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            object returnValue=null;
            if (!IsInitialize)
            {
                if (Query != null)
                {
                    SetEntities();
                }
                var value = EntitityKeys[Target];
                var target= value.GetProperty(OrmProperty.PropertyName);
                if (target == null)
                {
                    if (OrmProperty.Map.MapType == OrmMapType.OneToMany)
                        target = new ArrayList().ToArray(Type);
                    else
                        target = Activator.CreateInstance(Type);
                }
                returnValue = targetMethod.Invoke(target, args);
            }
            return returnValue;
        }


        /// <summary>
        /// 设置实体
        /// </summary>
        protected virtual void SetEntities()
        {
            if (EntitityKeys == null) return ;
            var infos = GetInfos();
            if (infos == null)
            {
                SetNullEntities(null);
            }
            else
            {
                if (OrmProperty.Map.MapType == OrmMapType.OneToOne)
                {
                     SetOnoToOneEntitis(infos);
                }
                else if (OrmProperty.Map.MapType == OrmMapType.OneToMany)
                {
                     SetOnoToManyEntitis(infos);
                }
            }
        }
        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="infos"></param>
        protected virtual void SetNullEntities(IList<EntityInfo> infos)
        {
            foreach (var entity in EntitityKeys)
            {
                SetEntityMapInfo(entity.Key, entity.Value);
            }
        }
        /// <summary>
        /// 设置一对一
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        protected virtual void SetOnoToOneEntitis(IList<EntityInfo> infos)
        {
            foreach (var entity in EntitityKeys)
            {
                var objId = entity.Value.GetProperty(OrmProperty.Map.MapObjectProperty.PropertyName);
                if(objId==null)continue;
                var value = entity.Value;
                foreach (var info in infos)
                {
                    var mapObjId = info.GetProperty(OrmProperty.Map.MapObjectProperty.PropertyName);
                    if (objId.Equals(mapObjId))
                    {
                        value = info;
                        break;
                    }
                }
                SetEntityMapInfo(entity.Key, value);
            }
        }
        /// <summary>
        /// 设置一对多
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        protected virtual void SetOnoToManyEntitis(IList<EntityInfo> infos)
        {
            var type = Type.GetType(OrmProperty.Map.GetMapObject().ObjectName);
            foreach (var entity in EntitityKeys)
            {
                var objId = entity.Value.GetProperty(OrmProperty.Map.ObjectProperty.PropertyName);
                if (objId == null) continue;
                var value = new ArrayList();
                foreach (var info in infos)
                {
                    var mapObjId = info.GetProperty(OrmProperty.Map.MapObjectProperty.PropertyName);
                    if (objId.Equals(mapObjId))
                    {
                        value.Add(info);
                    }
                }

                SetEntityMapInfo(entity.Key, value.ToArray(type));
            }
        }

        /// <summary>
        /// 设置实体映射
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="value"></param>
        protected virtual void SetEntityMapInfo(object entity, object value)
        {
            var type = entity.GetType().GetProperty(OrmProperty.PropertyName);
            type.SetValue(entity, value, null);
        }

        /// <summary>
        /// 得到实体
        /// </summary>
        /// <returns></returns>
        protected virtual IList<EntityInfo> GetInfos()
        {
            if (EntitityKeys == null || EntitityKeys.Count == 0)
                return null;
            var infos = Executor.GetInfos<IList<EntityInfo>>(OrmProperty.Map.GetMapObject(), Query,IsLazyLoadExecute);
            Query = null;
            return infos;
        }

        #endregion



    }
  
}
