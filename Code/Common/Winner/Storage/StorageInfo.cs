using System;
using System.Collections.Generic;

namespace Winner.Storage
{
    public class StorageInfo
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 顺序
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] FileBytes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DownSeek { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DownLength { get; set; }
        /// <summary>
        /// 分钟
        /// </summary>
        public DateTime DownTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string,object> Variables { get; set; }


    }
}
