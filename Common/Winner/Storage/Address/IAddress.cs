using System.Collections.Generic;

namespace Winner.Storage.Address
{
    public interface IAddress
    {
        /// <summary>
        /// 得到全路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        AddressInfo GetAddress(string fileName);

        /// <summary>
        /// 得到全路径
        /// </summary>
        /// <returns></returns>
        IList<AddressInfo> GetAddresses();
    }
}
