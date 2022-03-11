using System;
using System.Linq;
using System.Reflection;

namespace Winner.Base
{
    public class Property :  IProperty
    {
        #region 接口的实现

        /// <summary>
        /// 得到属性值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual T GetValue<T>(object info, string name)
        {
            if (info == null || string.IsNullOrEmpty(name)) return default(T);
            object rev = name.Contains(".")
                             ? GetRelatePropertyValue(info, name)
                             : GetPropertyValue(info, name);
            if (rev == null) return default(T);
            return TryConvertValue<T>(rev);
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
        /// 试着将value转换为type类型的值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual T TryConvertValue<T>(object value)
        {
            if (value == null) return default(T);
            var rev = TryConvertValue(value, typeof (T));
            if (rev == null) return default(T);
            return (T) rev;
        }

        /// <summary>
        /// 试着将value转换为type类型的值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object TryConvertValue(object value, Type type)
        {
            if (value == null) return null;
            if (type == typeof(object)) return value;
            try
            {
                if (type.IsEnum)
                {
                    return Enum.Parse(type, value.ToString());
                }
                return Convert.ChangeType(value, type);
            }
            catch
            {
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }
        }

      

        #endregion

        #region 方法

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
            try
            {
                var property = info.GetType()
                 .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                 .FirstOrDefault(it => it.Name.Equals(name));
                if (property == null) return false;
                value = TryConvertValue(value, property.PropertyType);
                if (value != null)
                {
                    property.SetValue(info, value, null);
                    return true;
                }
            }
            catch 
            {
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
            object obj = GetPropertyValue(info,name);
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

        #endregion
    }
}
