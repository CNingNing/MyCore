using System;

namespace Winner.Creation
{
    public class AopInfo
    {
        public string Name { get; set; }
        /// <summary>
        /// 方法
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public AopType Type { get; set; }
        /// <summary>
        /// 是否异步
        /// </summary>
        public bool IsAsync { get; set; }
        /// <summary>
        /// 处理
        /// </summary>
        public Action<AopArgsInfo> Handle { get; set; }
    }
}
