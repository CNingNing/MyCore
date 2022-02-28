namespace Winner.Lock
{
    public interface ILocker
    {
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        bool Create(LockerInfo info);
        
        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <returns></returns>
        bool Release(string key);
    }
}
