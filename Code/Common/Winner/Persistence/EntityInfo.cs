using System;
using System.Collections.Generic;

namespace Winner.Persistence
{

    [Serializable]
    public class EntityInfo:ParameterInfo
    {
       
        private SaveType _saveType = SaveType.None;
        /// <summary>
        /// 操作类型
        /// </summary>

        public virtual SaveType SaveType
        {
            get { return _saveType; }
            set { _saveType = value; }
        }
        /// <summary>
        /// 存储顺序
        /// </summary>
        public virtual int SaveSequence { get; set; }
        /// <summary>
        /// 添加和更新自定义属性
        /// </summary>

        public virtual IList<string> Properties { get; set; }
        /// <summary>
        /// 自定义条件
        /// </summary>

        public virtual string WhereExp { get; set; }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new EntityInfo SetParameter<T>(T value)
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
        public new EntityInfo SetParameter<T>(string name, T value)
        {
            base.SetParameter(name, value);
            return this;
        }

        /// <summary>
        /// 设置自定义属性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual EntityInfo SetProperty(string name)
        {
            Properties = Properties ?? new List<string>();
            if (!Properties.Contains(name))
                Properties.Add(name);
            return this;
        }
        /// <summary>
        /// 自定义条件
        /// </summary>
        /// <param name="where"></param>
        public virtual EntityInfo Where(string where)
        {
            WhereExp= where;
            return this;
        }


    }


 
    
}