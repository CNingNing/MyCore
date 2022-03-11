namespace Winner.Storage
{
    /// <summary>
    /// 文件存储接口
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// 得到全路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string GetFullFileName(string fileName);
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string CreateFileName(string fileName);
        /// <summary>
        /// 存储文件,返回存储后的文件路径
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        long GetSize(StorageInfo info);
        /// <summary>
        /// 存储文件,返回存储后的文件路径
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool Save(StorageInfo info);
        /// <summary>
        /// 删除文件或者目录
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool Remove(StorageInfo info);
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        byte[] Download(StorageInfo info);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool Upload(StorageInfo info);
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        void Merge(StorageInfo info);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool Check(string fileName);
    }
}
