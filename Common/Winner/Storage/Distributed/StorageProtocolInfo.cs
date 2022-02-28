

namespace Winner.Storage.Distributed
{
    public class StorageProtocolInfo
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int DownSeek { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DownLength { get; set; }
    }
}
