using System;

namespace Winner.Persistence.Compiler.Analysis
{
    /// <summary>
    /// 词元
    /// </summary>
    [Serializable]
    public class WordInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
  
    }
}
