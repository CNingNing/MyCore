namespace Winner.Message
{
    public interface IMessage
    {
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        bool SetCount(MessageInfo info);
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        int AddCount(MessageInfo info);
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        int RemoveCount(MessageInfo info);
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        int GetCount(string key);
    }
}
