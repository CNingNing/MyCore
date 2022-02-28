using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Winner.Creation
{
    

    public class Factory :  IFactory
    {
        #region 属性
        IDictionary<string,FactoryInfo> _factories =new Dictionary<string, FactoryInfo>();
        /// <summary>
        ///工厂集合
        /// </summary>
        public IDictionary<string, FactoryInfo> Factories
        {
            get { return _factories; }
            set { _factories = value; }
        }
        /// <summary>
        /// 代理实例
        /// </summary>
        public virtual string ProxyName { get; set; }
        #endregion

        #region 接口的实现
        /// <summary>
        /// 得到实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual T Get<T>(string name = null)
        {
            name = name ?? typeof(T).ToString();
            var info = Factories.ContainsKey(name)? Factories[name]:null;
            if (info==null)
                return default(T);
            if (info.IsSingle)
                return (T)info.Target;
            return (T)Create(info);
        }

        /// <summary>
        /// 创建实例，isSingle表示是否为单例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <param name="isSingle"></param>
        public virtual bool Set(string name, object obj, bool isSingle)
        {
            var info =new FactoryInfo{Name=name,Target=obj,IsSingle=isSingle};
            return Set(info);
        }

        /// <summary>
        /// 添加实例
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual bool Set(FactoryInfo info)
        {
            if (info == null) return false;
            if (Factories.ContainsKey(info.Name))
                Factories.Remove(info.Name);
            Factories.Add(info.Name,info);
            return true;
        }

        
        #endregion

        #region 创建代理实例和添加对象

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected virtual object Create(FactoryInfo info)
        {
            object target= CreateTarget(info);
            if (target == null)
                return null;
            TrySetProperties(target, info.Properties);
            TrySetAops(info.Aops);
            return target;
        }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected virtual object CreateTarget(FactoryInfo info)
        {
            object target;
            var obj = CreateClass(info.ClassName);
            if (info.Aops != null && info.Aops.Count > 0)
                target = GetTransparentProxy(obj, info);
            else
                target = obj;
            if (target == null)
                return null;
            return target;
        }
        ///  <summary>
        /// 尝试创建实例
        ///  </summary>
        /// <param name="target"></param>
        /// <param name="properties"></param>
        protected virtual void TrySetProperties(object target, IList<FactoryPropertyInfo> properties)
        {
            try
            {
                if (properties == null || properties.Count == 0 || target==null) return;
                SetProperty(target, properties);
            }
            catch (Exception ex)
            {
                throw new Exception(target.GetType().FullName, ex);
            }
        }

        ///  <summary>
        /// 尝试创建实例
        ///  </summary>
        /// <param name="aops"></param>
        protected virtual void TrySetAops(IList<AopInfo> aops)
        {
            if (aops == null || aops.Count == 0) return;

            foreach (var aop in aops)
            {
                SetAop(aop);
            }
        }

        ///  <summary>
        /// 尝试创建实例
        ///  </summary>
        /// <param name="aop"></param>
        protected virtual void SetAop(AopInfo aop)
        {
            try
            {
                var factory = Factories.ContainsKey(aop.Value) ? Factories[aop.Value] : null;
                if (factory == null)
                    return;
                var target = factory.Target == null ? Create(factory) : factory.Target;
                aop.Handle = (Action<AopArgsInfo>)
                    Delegate.CreateDelegate(typeof(Action<AopArgsInfo>), target, aop.Method);

            }
            catch (Exception ex)
            {

                throw new Exception($"{aop.Name}{aop.Method}", ex);
            }
         
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="target"></param>
        /// <param name="properties"></param>
        protected virtual void SetProperty(object target, IList<FactoryPropertyInfo> properties)
        {
            foreach (var p in properties)
            {
                var property = target.GetType().GetProperties().FirstOrDefault(it => it.Name.Equals(p.Name));
                if (property == null) continue;
                var value = GetPropertyValue(target,property, p);
                if (value == null) continue;
                property.SetValue(target, value, null);
                TrySetProperties(value, p.Properties);
            }
        }

        /// <summary>
        /// 得到属性值
        /// </summary>
        /// <param name="target"></param>
        /// <param name="property"></param>
        /// <param name="p"></param>
        protected virtual object GetPropertyValue(object target, PropertyInfo property, FactoryPropertyInfo p)
        {
            object value;
            if (property.PropertyType.IsInterface)
            {
                var t = Factories.ContainsKey(p.Value) ? Factories[p.Value] : null;
                if (t != null && t.Target != null && t.Target.GetType() == target.GetType())
                {
                    return target;
                }

                value = t == null || !t.IsSingle ? CreateClass(p.Value) : t.Target;
            }
            else
                value = TryConvertValue(p.Value, property.PropertyType);
            return value;
        }

        /// <summary>
        /// 转换值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual object TryConvertValue(object value, Type type)
        {
            if (value == null) return null;
            if (type == typeof(object)) return value;
            try
            {
                if (type.IsEnum)
                {
                    var charValue = Convert.ChangeType(value, typeof(char));
                    if (charValue == null) return Enum.Parse(type, value.ToString());
                    var intValue = Convert.ChangeType(charValue, typeof(int));
                    if (intValue == null) return null;
                    return Enum.Parse(type, intValue.ToString());
                }
                return Convert.ChangeType(value, type);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 创建类
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        protected virtual object CreateClass(string className)
        {
            var t = Type.GetType(className);
            if (t == null) return null;
            return Activator.CreateInstance(t);
        }

        /// <summary>
        /// 创建代理实例
        /// </summary>
        /// <param name="target"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        protected virtual object GetTransparentProxy(object target, FactoryInfo factory)
        {
            var proxy = (Proxy)Proxy.Create(target.GetType());
            proxy.Set(target, factory.Aops);
            return proxy;
        }

        #endregion

    }
}
