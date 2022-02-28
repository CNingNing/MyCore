using System;

namespace Winner.Storage
{
    [Serializable]
    public enum DataServiceType
    {
        /// <summary>
        /// 主
        /// </summary>
        Master=1,
        /// <summary>
        /// 从
        /// </summary>
        Slave=2
    }
}
