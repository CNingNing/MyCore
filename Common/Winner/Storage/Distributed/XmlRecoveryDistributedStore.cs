using System;
using System.Net;

namespace Winner.Storage.Distributed
{
    public class XmlRecoveryDistributedStore : XmlDistributedStore
    {
        public override string GetFullFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;
            var hashValue = GetHashValue();
            var dataServiceGroup = GetDataServiceGroup(fileName);
            if (dataServiceGroup == null || dataServiceGroup.Addresses == null || dataServiceGroup.Addresses.Length == 0)
                return fileName;
            var name = dataServiceGroup.Addresses[hashValue % dataServiceGroup.Addresses.Length];
            var address = Address.GetAddress(name);
            if (address == null) return fileName;
            var url= string.Format("{0}{1}", address.Url, fileName);
            if (CheckUrl(url))
                return url;
            foreach (var add in Address.GetAddresses())
            {
                if(add.Url==address.Url || add.GroupName!=address.GroupName)
                    continue;
                 url = string.Format("{0}{1}", add.Url, fileName);
                if (CheckUrl(url))
                    return url;
            }
            return fileName;
        }

        /// <summary>
        /// 检查地址是否存在
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected virtual bool CheckUrl(string url)
        {
            bool result;
            WebResponse response = null;
            try
            {
                WebRequest req = WebRequest.Create(url);
                response = req.GetResponse();
                result = response == null ? false : true;
            }
            catch 
            {
                result = false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return result;
        }



    }
}
