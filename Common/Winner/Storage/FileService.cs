using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Winner.Channel;
using Winner.Log;
using Winner.Storage.Cache;
using Winner.Storage.Distributed;

namespace Winner.Storage
{
    public class FileService
    {
        #region 属性
   
        private IFile _file;

        /// <summary>
        /// 实例
        /// </summary>
        public IFile File
        {
            get
            {
                if (_file == null)
                    _file = new FileStore{Cache = Creator.Get<ICache>()};
                return _file;
            }
            set { _file = value; }
        }


        private IMaster _master;

        /// <summary>
        /// 实例
        /// </summary>
        public IMaster Master
        {
            get
            {
                if (_master == null)
                    _master = Creator.Get<IMaster>();
                return _master;
            }
            set { _master = value; }
        }
        private ILog _log;

        /// <summary>
        /// 实例
        /// </summary>
        public ILog Log
        {
            get
            {
                if (_log == null)
                    _log = Creator.Get<ILog>();
                return _log;
            }
            set { _log = value; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public FileService()
        { 
        }

        /// <summary>
        /// WCF服务端配置文件路径，文件存储实例，错误日志实例
        /// </summary>
        /// <param name="file"></param>
        public FileService(IFile file)
        {
            File = file;
        }
        #endregion

        #region 接口的实现
        public virtual void Handle(ChannelArgsInfo args)
        {
            try
            {

                switch (args.Method)
                {
                    case 's':
                    {
                        if (args.Result == null || args.Result.Length < 4)
                            return;
                        var len = BitConverter.ToInt32(args.Result.Take(4).ToArray(), 0);
                        if (len == 0 || len + 4 > args.Result.Length)
                            return;
                        var fileBytes = args.Result.Skip(4).Take(len).ToArray();
                        var valueBytes = args.Result.Skip(4 + len).Take(args.Result.Length - len - 4).ToArray();
                        var value = Encoding.UTF8.GetString(valueBytes, 0, valueBytes.Length);
                        if (string.IsNullOrWhiteSpace(value))
                            return;
                        var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<StorageProtocolInfo>(value);
                        if (protocol == null || string.IsNullOrWhiteSpace(protocol.FileName))
                            return;
                        args.SetArgs(File.Save(new StorageInfo { FileName = protocol.FileName,FileBytes=fileBytes }).ToString());
                        if (Master.IsBack(protocol.GroupName))
                        {
                            var task = new Thread(() => { Master.Save(protocol, fileBytes); });
                            task.Start();
                        }
                    }
                        break;
                    case 'r':
                    {
                        var value = args.GetResult();
                        if (string.IsNullOrWhiteSpace(value))
                            return;
                        var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<StorageProtocolInfo>(value);
                        if (protocol == null || string.IsNullOrWhiteSpace(protocol.FileName))
                            return;
                        args.SetArgs(File.Remove(new StorageInfo { FileName = protocol.FileName }).ToString());
                        if (Master.IsBack(protocol.GroupName))
                        {
                            var task = new Thread(() => { Master.Remove(protocol); });
                            task.Start();
                        }
                    }
                        break;
                    case 'd':
                    {
                        var value = args.GetResult();
                        if (string.IsNullOrWhiteSpace(value))
                            return;
                        var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<StorageProtocolInfo>(value);
                        if (protocol == null || string.IsNullOrWhiteSpace(protocol.FileName))
                            return;
                        args.Args = File.Download(new StorageInfo { FileName = protocol.FileName ,DownSeek=protocol.DownSeek,DownLength=protocol.DownLength});

                    }
                        break;
                    case 'u':
                    {
                        if (args.Result == null || args.Result.Length < 4)
                            return;
                        var len = BitConverter.ToInt32(args.Result.Take(4).ToArray(), 0);
                        if (len == 0 || len + 4 > args.Result.Length)
                            return;
                        var fileBytes = args.Result.Skip(4).Take(len).ToArray();
                        var valueBytes = args.Result.Skip(4 + len).Take(args.Result.Length - len - 4).ToArray();
                        var value = Encoding.UTF8.GetString(valueBytes, 0, valueBytes.Length);
                        if (string.IsNullOrWhiteSpace(value))
                            return;
                        var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<StorageProtocolInfo>(value);
                        if (protocol == null || string.IsNullOrWhiteSpace(protocol.FileName))
                            return;
                        args.SetArgs(File.Upload(new StorageInfo{FileBytes = fileBytes,FileName=protocol.FileName,Key=protocol.Key,Index=protocol.Index }).ToString());
                        if (Master.IsBack(protocol.GroupName))
                        {
                            var task = new Thread(() => { Master.Upload(protocol, fileBytes); });
                            task.Start();
                        }
                    }
                        break;
                    case 'm':
                    {
                        var value = args.GetResult();
                        if (string.IsNullOrWhiteSpace(value))
                            return;
                        var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<StorageProtocolInfo>(value);
                        if (protocol == null || string.IsNullOrWhiteSpace(protocol.FileName))
                            return;
                        File.Merge(new StorageInfo { FileName = protocol.FileName, Key = protocol.Key, Index = protocol.Index });
                        args.IsReturn = false;
                        if (Master.IsBack(protocol.GroupName))
                        {
                            var task = new Thread(() => { Master.Merge(protocol); });
                            task.Start();
                        }
                        }
                        break;
                    case 'c':
                    {
                        var value = args.GetResult();
                        if (string.IsNullOrWhiteSpace(value))
                            return;
                        var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<StorageProtocolInfo>(value);
                        if (protocol == null || string.IsNullOrWhiteSpace(protocol.FileName))
                            return;
                        args.SetArgs(File.Check(protocol.FileName).ToString());
                    }
                        break;
                    case 'g':
                    {
                        var value = args.GetResult();
                        if (string.IsNullOrWhiteSpace(value))
                            return;
                        var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<StorageProtocolInfo>(value);
                        if (protocol == null || string.IsNullOrWhiteSpace(protocol.FileName))
                            return;
                        args.SetArgs(File.GetSize(new StorageInfo{FileName= protocol.FileName }).ToString());
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
            }
        }
        #endregion
 




    }
}
