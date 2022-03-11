using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Winner.Storage.Distributed
{
    public class Master : DistributedBase,IMaster
    {




        #region 接口的实现

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public virtual bool IsBack(string groupName)
        {
            var dataServiceGroup =
                DistributedStore.DataServiceGroups.FirstOrDefault(it => it.Name.Equals(groupName));
            if (dataServiceGroup == null || dataServiceGroup.DataServices == null ||
                dataServiceGroup.DataServices.Count(it => it.Type == DataServiceType.Slave) == 0)
                return false;
            return true;
        }

        protected virtual DataServiceInfo GetSlaveDataService(string groupName)
        {
            var dataServiceGroup =
                DistributedStore.DataServiceGroups.FirstOrDefault(it => it.Name.Equals(groupName));
            if (dataServiceGroup == null)
                return null;
            var dataServices = dataServiceGroup.DataServices.Where(it => it.Type == DataServiceType.Slave).ToList();
            if (dataServices.Count == 0) return null;
            return dataServices[(int)(DateTime.Now.Ticks % dataServices.Count)];
        }
        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        public virtual bool Save(StorageProtocolInfo protocol, byte[] fileBytes)
        {
            if (string.IsNullOrEmpty(protocol.FileName) || fileBytes == null) return false;
            var groupName = protocol.GroupName;
            var dataService= GetSlaveDataService(groupName);
            if (dataService == null)
                return false;
            protocol.GroupName = null; 
            try
            {
                Handle(dataService.EndPoint, 's', protocol, fileBytes);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                AddException(protocol.FileName, groupName,SaveExceptionPath);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public virtual bool Remove(StorageProtocolInfo protocol)
        {
            if(string.IsNullOrEmpty(protocol.FileName))return false;
            var groupName = protocol.GroupName;
            var dataService = GetSlaveDataService(groupName);
            if (dataService == null)
                return false;
            protocol.GroupName = null;
            try
            {
                Handle(dataService.EndPoint, 'r', protocol);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                AddException(protocol.FileName, groupName,RemoveExceptionPath);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="fileBytes"></param>
        public virtual bool Upload(StorageProtocolInfo protocol, byte[] fileBytes)
        {
            if (string.IsNullOrEmpty(protocol.FileName) || fileBytes == null) return false;
            var groupName = protocol.GroupName;
            var dataService = GetSlaveDataService(groupName);
            if (dataService == null)
                return false;
            protocol.GroupName = null;
            try
            {
                Handle(dataService.EndPoint, 'u', protocol, fileBytes);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                AddException(protocol.FileName, groupName,UploadExceptionPath);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocol"></param>
        public virtual bool Merge(StorageProtocolInfo protocol)
        {
            if (string.IsNullOrEmpty(protocol.FileName)) return false;
            var groupName = protocol.GroupName;
            var dataService = GetSlaveDataService(groupName);
            if (dataService == null)
                return false;
            protocol.GroupName = null;
            try
            {
                Handle(dataService.EndPoint, 'm', protocol);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                AddException(protocol.FileName, groupName,MergeExceptionPath);
                return false;
            }
            return true;
        }


        /// <summary>
        /// 得到最新文件目录
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        protected virtual DirectoryInfo GetLastDirectory(DirectoryInfo directory)
        {
            var subDirectory = directory.GetDirectories().OrderByDescending(it => it.CreationTime).FirstOrDefault();
            if (subDirectory == null || !subDirectory.Exists)
                return directory;
            return GetLastDirectory(subDirectory);
        }

        #endregion

        #region 方法

        #region 异常处理
        /// <summary>
        /// 执行时间间隔
        /// </summary>
        public int TimmerInterval { get; set; }


        /// <summary>
        /// 开启异常处理
        /// </summary>
        public void StartException()
        {
            var times = TimmerInterval > 0 ? TimmerInterval : 60000;
            var task=new Thread(() =>
            {
                while (true)
                {
                    var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SaveExceptionPath);
                    var savedi = new DirectoryInfo(savePath);
                    if (!savedi.Exists)
                        savedi.Create();
                    var removePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RemoveExceptionPath);
                    var removedi = new DirectoryInfo(removePath);
                    if (!removedi.Exists)
                        removedi.Create();
                    StartSaveExceptionHandle();
                    StartRemoveExceptionHandle();
                    StartUploadExceptionHandle();
                    StartMergeExceptionHandle();
                    System.Threading.Thread.Sleep(times);
                }
            });
            task.Start();


        }

        /// <summary>
        /// 异常路径
        /// </summary>
        public string SaveExceptionPath { get; set; } = @"Exception/Save/";

        /// <summary>
        /// 异常路径
        /// </summary>
        public string RemoveExceptionPath { get; set; } = @"Exception/Remove/";

        /// <summary>
        /// 异常路径
        /// </summary>
        public string UploadExceptionPath { get; set; } = @"Exception/Upload/";

        /// <summary>
        /// 异常路径
        /// </summary>
        public string MergeExceptionPath { get; set; } = @"Exception/Merge/";

        private static readonly object SaveLocker=new object();

        /// <summary>
        /// 开启异常处理
        /// </summary>
        protected virtual void StartSaveExceptionHandle()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SaveExceptionPath);
            var di = new DirectoryInfo(path);
            if (!di.Exists) return;
            lock (SaveLocker)
            {
                var files = di.GetFiles();
                foreach (var file in files)
                {
                    byte[] fileByte;
                    var fileName = Path.GetFileName(file.FullName).Replace("%", "/").Replace(".txt", "");
                    var fullfileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                    using (var filestream = new FileStream(fullfileName, FileMode.Open))
                    {
                        fileByte = new byte[filestream.Length];
                        filestream.Read(fileByte, 0, fileByte.Length);
                    }
                    var groupName = File.ReadAllText(file.FullName);
                    var protocol = new StorageProtocolInfo {FileName = fileName, GroupName = groupName};
                    var rev=Save(protocol, fileByte);
                    if(rev)
                        File.Delete(file.FullName);
                }
            }
        }
        private static readonly object RemoveLocker = new object();
        /// <summary>
        /// 开启异常处理
        /// </summary>
        protected virtual void StartRemoveExceptionHandle()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RemoveExceptionPath);
            var di = new DirectoryInfo(path);
            if (!di.Exists) return;
            lock (RemoveLocker)
            {
                var files = di.GetFiles();
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FullName).Replace("%", "/").Replace(".txt", "");
                    var groupName = File.ReadAllText(file.FullName);
                    var protocol = new StorageProtocolInfo { FileName = fileName, GroupName = groupName };
                    var rev = Remove(protocol);
                    if (rev)
                        File.Delete(file.FullName);
                }
            }
        }



        private static readonly object UploadLocker = new object();

        /// <summary>
        /// 开启异常处理
        /// </summary>
        protected virtual void StartUploadExceptionHandle()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SaveExceptionPath);
            var di = new DirectoryInfo(path);
            if (!di.Exists) return;
            lock (UploadLocker)
            {
              
                var files = di.GetFiles();
                foreach (var file in files)
                {
                    byte[] fileByte;
                    var fileName = Path.GetFileName(file.FullName).Replace("%", "/").Replace(".txt", "");
                    var fullfileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                    using (var filestream = new FileStream(fullfileName, FileMode.Open))
                    {
                        fileByte = new byte[filestream.Length];
                        filestream.Read(fileByte, 0, fileByte.Length);
                    }
                    var groupName = File.ReadAllText(file.FullName);
                    var protocol = new StorageProtocolInfo { FileName = fileName, GroupName = groupName };
                    var rev = Upload(protocol, fileByte);
                    if (rev)
                        File.Delete(file.FullName);
                }
            }
        }

        private static readonly object MergeLocker = new object();
        /// <summary>
        /// 开启异常处理
        /// </summary>
        protected virtual void StartMergeExceptionHandle()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RemoveExceptionPath);
            var di = new DirectoryInfo(path);
            if (!di.Exists) return;
            lock (MergeLocker)
            { 
                var files = di.GetFiles();
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FullName).Replace("%", "/").Replace(".txt", "");
                    var groupName = File.ReadAllText(file.FullName);
                    var protocol = new StorageProtocolInfo { FileName = fileName, GroupName = groupName };
                    var rev = Merge(protocol);
                    if (rev)
                        File.Delete(file.FullName);
                }
            }
        }

        /// <summary>
        /// 添加异常信息
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="groupName"></param>
        protected virtual void AddException(string fileName, string groupName,string dirPath)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dirPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    string.Format("{0}{1}.txt", dirPath, fileName.Replace("/", "%")));
            File.WriteAllText(fileName, groupName);
        }

       
     
        #endregion


        #endregion




    }
}
