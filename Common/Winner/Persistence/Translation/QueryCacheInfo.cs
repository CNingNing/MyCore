using System;

namespace Winner.Persistence.Translation
{
    [Serializable]
    public class QueryCacheInfo<T>
    {
        /// <summary>
        /// 结果
        /// </summary>
        public T Result { get; set; }
        /// <summary>
        /// 数据数量
        /// </summary>
        public int DataCount { get; set; }
    }



 
    
}