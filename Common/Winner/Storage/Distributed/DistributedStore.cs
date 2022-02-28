using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Winner.Storage.Address;
using Winner.Storage.Route;

namespace Winner.Storage.Distributed
{
    public class DistributedStore : DistributedBase,IFile
    {

        #region 属性

        private static IList<DataServiceGroupInfo>  _dataServiceGroups = new List<DataServiceGroupInfo> ();

        /// <summary>
        /// 存储
        /// </summary>
        public static IList<DataServiceGroupInfo> DataServiceGroups
        {
            get { return _dataServiceGroups; }
            set { _dataServiceGroups = value; }
        }
        /// <summary>
        /// 文件路由
        /// </summary>
        public IFileRoute FileRoute { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public IAddress Address { get; set; }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public DistributedStore()
        { 
        }


        #endregion

        #region 接口的实现
        /// <summary>
        /// 得到文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual string GetFullFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;
            var hashValue = GenerateLongId(fileName);
            var dataServiceGroup = GetDataServiceGroup(fileName);
            if (dataServiceGroup == null || dataServiceGroup.Addresses == null || dataServiceGroup.Addresses.Length==0)
                return fileName;
            var name = dataServiceGroup.Addresses[hashValue%dataServiceGroup.Addresses.Length];
            var address = Address.GetAddress(name);
            if (address == null) return fileName;
            return string.Format("{0}{1}", address.Url, fileName);
        }
        /// <summary>
        /// 创建文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string CreateFileName(string fileName)
        {
           
            fileName = FileRoute.CreateFileName(fileName);
            if (string.IsNullOrEmpty(fileName))
                return fileName;
            var index = fileName.LastIndexOf('.');
            if (index == -1)
                index = fileName.Length;
            var hashValue = GetHashValue();
            var dataServiceGroup = GetDataServiceGroup(fileName,hashValue);
            if (dataServiceGroup == null)
                return fileName;
            return fileName.Insert(index, string.Format("_{0}",dataServiceGroup.Name));
        }

        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        /// <returns></returns>
        public virtual bool Save(StorageInfo info)
        {
            var hashValue = GetHashValue();
            var dataServiceGroup = GetDataServiceGroup(info.FileName, hashValue);
            if (dataServiceGroup == null || dataServiceGroup.DataServices == null)
                return false;
            var dataService = GetMasterDataService(dataServiceGroup, hashValue);
            var protocol = new StorageProtocolInfo {FileName = info.FileName,GroupName = dataServiceGroup.Name};
            var rev = Handle(dataService.EndPoint, 's', protocol, info.FileBytes);
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual bool Remove(StorageInfo info)
        {
            var hashValue = GetHashValue();
            var dataServiceGroup = GetDataServiceGroup(info.FileName);
            if (dataServiceGroup == null || dataServiceGroup.DataServices == null)
                return false;
            var dataService = GetMasterDataService(dataServiceGroup, hashValue);
            var protocol = new StorageProtocolInfo {FileName = info.FileName, GroupName = dataServiceGroup.Name };
            var rev = Handle(dataService.EndPoint, 'r', protocol,true);
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual long GetSize(StorageInfo info)
        {
            var hashValue = GetHashValue();
            var dataServiceGroup = GetDataServiceGroup(info.FileName);
            if (dataServiceGroup == null || dataServiceGroup.DataServices == null)
                return 0;
            var dataService = GetMasterDataService(dataServiceGroup, hashValue);
            var protocol = new StorageProtocolInfo { FileName = info.FileName };
            var rev = Handle(dataService.EndPoint, 'g', protocol, true);
            if (string.IsNullOrWhiteSpace(rev))
                return 0;
            long revValue;
            long.TryParse(rev, out revValue);
            return revValue;


        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual byte[] Download(StorageInfo info)
        {
            var dataServiceGroup = GetDataServiceGroup(info.FileName);
            if (dataServiceGroup == null || dataServiceGroup.DataServices == null)
                return null;
            var protocol = new StorageProtocolInfo { FileName = info.FileName,DownSeek=info.DownSeek,DownLength=info.DownLength };
            var args=Handle(dataServiceGroup.DataServices.Select(it => it.EndPoint).ToList(), 'd', protocol);
            return args.Result;
  
  
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public virtual bool Upload(StorageInfo info)
        {
            var hashValue = GenerateLongId(info.Key);
            var dataServiceGroup = GetDataServiceGroup(info.FileName, hashValue);
            if (dataServiceGroup == null || dataServiceGroup.DataServices == null)
                return false;
            var dataService = GetMasterDataService(dataServiceGroup, hashValue);
            var protocol = new StorageProtocolInfo { FileName = info.FileName,Index=info.Index,Key=info.Key, GroupName = dataServiceGroup.Name };
            var rev = Handle(dataService.EndPoint, 'u', protocol, info.FileBytes);
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual void Merge(StorageInfo info)
        {
            var hashValue = GenerateLongId(info.Key);
            var dataServiceGroup = GetDataServiceGroup(info.FileName, hashValue);
            if (dataServiceGroup == null || dataServiceGroup.DataServices == null)
                return;
            var dataService = GetMasterDataService(dataServiceGroup, hashValue);
            var protocol = new StorageProtocolInfo {FileName = info.FileName, Key = info.Key, GroupName = dataServiceGroup.Name };
            Handle(dataService.EndPoint, 'm', protocol, false);
        }
        /// <summary>
        /// 检查文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual bool Check(string fileName)
        {
            var hashValue = GetHashValue();
            var dataServiceGroup = GetDataServiceGroup(fileName);
            if (dataServiceGroup == null || dataServiceGroup.DataServices == null)
                return false;
            var dataService = GetMasterDataService(dataServiceGroup, hashValue);
            var protocol = new StorageProtocolInfo { FileName = fileName };
            var rev = Handle(dataService.EndPoint, 'c', protocol, true);
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }

        #endregion

        #region 方法


        /// <summary>
        /// 得到写的服务器
        /// </summary>
        /// <param name="dataServiceGroup"></param>
        /// <param name="hashValue"></param>
        /// <returns></returns>
        protected virtual DataServiceInfo GetMasterDataService(DataServiceGroupInfo dataServiceGroup,long hashValue)
        {
            var dataServices = dataServiceGroup.DataServices.Where(it=>it.Type==DataServiceType.Master).ToList();
            if (dataServices.Count == 0) return null;
            return dataServices[(int)(hashValue%dataServices.Count)];
        }

        /// <summary>
        /// 得到数据服务器
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="hashValue"></param>
        /// <returns></returns>
        protected virtual DataServiceGroupInfo GetDataServiceGroup(string fileName,long hashValue)
        {
            var dataServiceGroups =
                DataServiceGroups.Where(it => (string.IsNullOrEmpty(it.Path) || fileName.Contains(it.Path)) && !it.IsClose).ToList();
            var index = (int) (hashValue%(dataServiceGroups.Count == 0 ? 1 : dataServiceGroups.Count));
            if (dataServiceGroups.Count == 0)
                return null;
            return dataServiceGroups[index];
        }
        /// <summary>
        /// 得到hash值
        /// </summary>
        /// <returns></returns>
        protected virtual long GetHashValue()
        {
            return DateTime.Now.Ticks;
        }

        /// <summary>
        /// 得到数据服务器
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected virtual DataServiceGroupInfo GetDataServiceGroup(string fileName)
        {
            string name = null;
            var startIndex = fileName.LastIndexOf("_") + 1;
            if (startIndex > 0)
            {
                var endIndex = fileName.IndexOf(".", startIndex);
                if (startIndex > 0 && endIndex > -1 && endIndex > startIndex)
                {
                    name = fileName.Substring(startIndex, endIndex - startIndex);
                }
            }
            var dataServiceGroup =string.IsNullOrEmpty(name)?
                DataServiceGroups.FirstOrDefault(it => (string.IsNullOrEmpty(it.Path) || fileName.Contains(it.Path))):
                DataServiceGroups.FirstOrDefault(it=>it.Name.Equals(name));
            return dataServiceGroup;
        }



        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static long GenerateLongId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            byte[] buffer = Encoding.UTF8.GetBytes(EncryptMd5(input));
            return BitConverter.ToInt64(buffer, 0);
        }
        /// <summary>
        /// 得到MD5加密
        /// </summary>
        /// <returns></returns>
        private static string EncryptMd5(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var md5 = MD5.Create();
            byte[] bytValue = Encoding.UTF8.GetBytes(input);
            byte[] bytHash = md5.ComputeHash(bytValue);
            var sTemp = new StringBuilder();
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp.Append(bytHash[i].ToString("X").PadLeft(2, '0'));
            }
            return sTemp.ToString().ToLower();
        }
    }
}
