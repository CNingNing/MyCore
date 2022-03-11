using System;
using System.IO;

namespace Winner.Storage.Image
{
    public class ThumbnailInfo
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 输出类型
        /// </summary>
        public byte[] Output { get; set; }
        /// <summary>
        /// 类型
        /// </summary>

        public string ContentType { get; set; }
    }
}
