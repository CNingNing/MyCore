using System.Collections.Generic;
using System.Linq;

namespace Winner.Storage.Address
{
    public class Address : IAddress
    {
        #region 属性
        private IDictionary<string, AddressInfo> _addresses = new Dictionary<string, AddressInfo>();
        public IDictionary<string, AddressInfo> Addresses
        {
            get { return _addresses; }
            set { _addresses = value; }
        }
        #endregion
   
        /// <summary>
        /// 得到域名信息
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual AddressInfo GetAddress(string fileName)
        {
            if (Addresses.ContainsKey(fileName))
                return Addresses[fileName];
            return null;
        }
        /// <summary>
        /// 得到全部地址
        /// </summary>
        /// <returns></returns>
        public virtual IList<AddressInfo> GetAddresses()
        {
            return Addresses.Values.ToList();
        }
    }
}
