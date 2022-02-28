using System;
using System.Collections;
using System.Collections.Generic;

namespace Winner.Persistence
{
    [Serializable]
    public class ParameterInfo
    {
        /// <summary>
        /// 参数
        /// </summary>
        public virtual IDictionary<string, object> Parameters { get; set; }
        /// <summary>
        /// 创建参数
        /// </summary>
        /// <returns></returns>
        public virtual string CreateParameterName()
        {
            return string.Format("P{0}", Parameters == null ? 0 : Parameters.Count);
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual void SetParameter<T>(T value)
        {
            var name = CreateParameterName();
        
            SetParameter(name, value);
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual void SetParameter<T>(string name, T value)
        {
            Parameters = Parameters ?? new Dictionary<string, object>();
            if (value is IEnumerable && value.GetType().IsGenericType && value.GetType().GetGenericArguments().Length > 0)
            {
                var enumerable = ((IEnumerable)value).GetEnumerator();
                var rev = new ArrayList();
                while (enumerable.MoveNext())
                {
                    rev.Add(enumerable.Current);
                }
                Parameters.Add(name, rev.ToArray());
                return;
            }
            Parameters.Add(name, value);
        }
 
    }
}
