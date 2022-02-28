namespace Winner.Storage.Distributed
{
    public interface IMaster
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        bool IsBack(string groupName);
        /// <summary>
        /// 存储文件,返回存储后的文件路径
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        bool Save(StorageProtocolInfo protocol,byte[] fileBytes);

        /// <summary>
        /// 删除文件或者目录
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        bool Remove(StorageProtocolInfo protocol);

        /// <summary>
        /// 删除文件或者目录
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        bool Upload(StorageProtocolInfo protocol, byte[] fileBytes);
        /// <summary>
        /// 删除文件或者目录
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        bool Merge(StorageProtocolInfo protocol);
        /// <summary>
        /// 开启异常处理
        /// </summary>
        void StartException();

    }
}
