using System;
using System.Collections.Generic;

namespace Winner.Creation
{
    [Serializable]
    public class FactoryPropertyInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 属性
        /// </summary>
        public IList<FactoryPropertyInfo>  Properties { get; set; } 
 
    }
}
