using System;

namespace Winner.Base
{
    public interface IProperty
    {
        /// <summary>
        /// 得到属性
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        T GetValue<T>(object info, string name);
        /// <summary>
        /// 填充属性
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        bool SetValue(object info, string name, object value);
        /// <summary>
        /// 试着将value转换为T类型的值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        T TryConvertValue<T>(object value);
        /// <summary>
        /// 试着将value转换为type类型的值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object TryConvertValue(object value, Type type);
    }
}
