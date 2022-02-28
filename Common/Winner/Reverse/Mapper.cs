using System;
using System.Linq;
using System.Reflection;

namespace Winner.Reverse
{
    public class Mapper : IMapper
    {
        

        #region 接口的实现

        /// <summary>
        /// 转换对象
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual TDestination Map<TDestination>(object source)
        {
            var destination = Activator.CreateInstance<TDestination>();
            return Map(source, destination);
        }

        /// <summary>
        /// 转换对象
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public virtual TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return MapProperty(source, destination);
        }
  
        #endregion

        #region 方法

        /// <summary>
        /// 映射属性
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        protected virtual TDestination MapProperty<TSource, TDestination>(TSource source, TDestination destination)
        {
            var properties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |BindingFlags.SetProperty);
            foreach (var property in properties)
            {
                var value = GetValue(source, property.Name);
                if(value==null)continue;
                SetValue(destination, property.Name, value);
            }
            return destination;
        }
 

        /// <summary>
        /// 得到属性值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual object GetValue(object info, string name)
        {
            if (info == null || string.IsNullOrEmpty(name)) return null;
            object rev = name.Contains(".")
                             ? GetRelatePropertyValue(info, name)
                             : GetPropertyValue(info, name);
            
            return TryConvertValue(rev);
        }

        /// <summary>
        /// 填充属性值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public virtual bool SetValue(object info, string name, object value)
        {
            if (info == null || string.IsNullOrEmpty(name)) return false;
            if (name.Contains('.'))
                return SetRelatePropertyValue(info, name, value);
            return SetPropertyValue(info, name, value);
        }
        /// <summary>
        /// 得到属性对象
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual object GetPropertyValue(object info, string name)
        {
            var property = info.GetType()
                   .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .FirstOrDefault(it => it.Name.Equals(name));
            if (property != null)
            {
                return property.GetValue(info, null);
            }
            return null;
        }
        /// <summary>
        /// 得到关联属性对象
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual object GetRelatePropertyValue(object info, string name)
        {
            string[] str = name.Split('.');
            object obj = info;
            foreach (string t in str)
            {
                obj = GetPropertyValue(obj, t);
                if (obj == null)
                    return null;
            }
            return obj;
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual bool SetPropertyValue(object info, string name, object value)
        {
            var property = info.GetType()
                   .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .FirstOrDefault(it => it.Name.Equals(name));
            if (property == null || !property.CanWrite) return false;
            value = TryConvertValue(value);
            if (value != null)
            {
                property.SetValue(info, Convert.ChangeType(value, property.PropertyType), null);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置关联属性
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual bool SetRelatePropertyValue(object info, string name, object value)
        {
            string[] str = name.Split('.');
            object obj = info;
            for (int i = 0; i < str.Length; i++)
            {
                if (obj == null) break;
                if (i != str.Length - 1)
                    obj = GetAndFillPropertyValue(obj, str[i]);
                else
                    return SetPropertyValue(obj, str[i], value);
            }
            return false;
        }
        /// <summary>
        /// 返回属性值,如果属性为null则填充该属性值
        /// </summary>
        protected virtual object GetAndFillPropertyValue(object info, string name)
        {
            object obj = GetPropertyValue(info, name);
            if (obj == null)
            {
                var pi = info.GetType()
                  .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                  .FirstOrDefault(it => it.Name.Equals(name));
                obj = pi == null ? null : Activator.CreateInstance(pi.PropertyType);
                SetPropertyValue(info, name, obj);
            }
            return obj;
        }

        /// <summary>
        /// 试着将value转换为type类型的值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual object TryConvertValue(object value)
        {
            if (value == null) return null;
            var type = value.GetType();
            if (type == typeof(object)) return value;
            try
            {
                return type.IsEnum ? Enum.Parse(type, value.ToString()) : Convert.ChangeType(value, type);
            }
            catch
            {
                return null;
            }
        }
        #endregion


    }
}
