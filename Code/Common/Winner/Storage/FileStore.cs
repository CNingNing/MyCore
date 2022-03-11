using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Winner.Storage.Route;

namespace Winner.Storage
{
    public class FileStore : IFile 
    {
        #region 属性
      
        /// <summary>
        /// 缓存
        /// </summary>
        public Cache.ICache Cache { get; set; }
        /// <summary>
        /// 文件路由
        /// </summary>
        public IFileRoute FileRoute { get; set; }
       
        #endregion



        #region 接口的实现
      
        /// <summary>
        /// 得到全路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual string GetFullFileName(string fileName)
        {
            return fileName;
        }
        /// <summary>
        /// 创建文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual string CreateFileName(string fileName)
        {
            return FileRoute.CreateFileName(fileName);
        }

        /// <summary>
        /// 存储
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        /// <returns></returns>
        public virtual bool Save(StorageInfo info)
        {
            if (string.IsNullOrEmpty(info.FileName) || info.FileBytes == null || info.FileBytes.Length==0) return false;
            SaveFile(info.FileName, info.FileBytes);
            return true;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual bool Remove(StorageInfo info)
        {
            if (string.IsNullOrEmpty(info.FileName)) return false;
            DeleteFileOrDirectory(GetAbsoluteFileName(info.FileName));
            return true;
        }

        /// <summary>
        /// 存储文件,返回存储后的文件路径
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual long GetSize(StorageInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.FileName))
                return 0;
            var file = new FileInfo(GetAbsoluteFileName(info.FileName));
            if (!file.Exists)
                return 0;
            return file.Length;
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] Download(StorageInfo info)
        {
            if (string.IsNullOrEmpty(info.FileName))
                return null;
            var fileByte = Cache.Get<byte[]>(info.FileName) ?? GetFileByte(info.FileName);
            if (fileByte != null)
                Cache.Set(info.FileName, fileByte);
            if (info.DownSeek == 0 || info.DownLength == 0)
                return fileByte;
            if (info.DownSeek >= fileByte.Length - 1)
                return null;
            var length = info.DownSeek + info.DownLength > fileByte.Length ? fileByte.Length - info.DownSeek : info.DownLength;
            return fileByte.Skip(info.DownSeek).Take(length).ToArray();
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="info"></param>
        public virtual bool Upload(StorageInfo info)
        {
            SaveFile($"/temp/{info.Key}/{info.Index}.temp", info.FileBytes);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public virtual void Merge(StorageInfo info)
        {
            var dir =new DirectoryInfo(GetAbsoluteFileName($"/temp/{info.Key}"));
            var fileName = GetAbsoluteFileName(info.FileName);
            var files = dir.GetFiles().OrderBy(it =>int.Parse(it.Name.Replace(".temp", ""))).ToList();
            var fileInfo=new FileInfo(fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
                Directory.CreateDirectory(fileInfo.DirectoryName??"/");
            using (var s = new FileStream(fileName, FileMode.Create))
            {
                using (var bw = new BinaryWriter(s))
                {
                    foreach (var file in files)
                    {
                        using (var tempFile=new FileStream(file.FullName,FileMode.Open))
                        {
                            var filebytes = new byte[tempFile.Length];
                            tempFile.Read(filebytes, 0, filebytes.Length);
                            bw.Write(filebytes);
                        }
                    }
                }
            }
            DeleteFileOrDirectory(dir.FullName);
           
        }

        /// <summary>
        /// 检查文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Check(string fileName)
        {
            fileName = GetAbsoluteFileName(fileName);
            return File.Exists(fileName);
        }

        #endregion

        #region 方法
 

        /// <summary>
        /// 得到文件二进制
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected virtual byte[] GetFileByte(string fileName)
        {
            fileName = GetAbsoluteFileName(fileName);
            if (!File.Exists(fileName)) return null;
            using (var s = new FileStream(fileName, FileMode.Open))
            {
                var buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// 删除文件或者路径
        /// </summary>
        /// <param name="fileName"></param>
        protected virtual void DeleteFileOrDirectory(string fileName)
        {
            if (File.Exists(fileName))
            {
                var index = fileName.LastIndexOf(@"/");
                string dir = fileName.Substring(0, index);
                var directory = new DirectoryInfo(dir);
                var pattern = fileName.Substring(index + 1, fileName.Length - index - 1);
                var extension = Path.GetExtension(pattern);
                if (!string.IsNullOrEmpty(extension))
                    pattern = pattern.Replace(extension, "");
                var files = directory.GetFiles(string.Format("{0}.*", pattern));
                foreach (var file in files)
                {
                    if (file.Exists)
                        file.Delete();
                }
                if(directory.GetFiles().Length == 0 && directory.GetDirectories().Length == 0)
                {
                    directory.Delete();
                }
            }
            DeleteDirectory(fileName);
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="path"></param>
        protected virtual void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            var fNames = Directory.GetFiles(path);
            foreach (var fName in fNames)
            {
                File.Delete(fName);
            }
            var dNames = Directory.GetDirectories(path);
            foreach (var dName in dNames)
            {
                DeleteDirectory(dName);
            }
            Directory.Delete(path);
        }


        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        protected virtual void SaveFile(string fileName,byte[] fileByte)
        {
            if (string.IsNullOrEmpty(fileName) || fileByte == null)return;
            fileName = GetAbsoluteFileName(fileName);
            CheckDirectoryAndDeleteFile(fileName);
            SaveByteToFile(fileName, fileByte);
        }
        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        protected virtual void SaveByteToFile(string fileName, byte[] fileByte)
        {
            using (var s = new FileStream(fileName, FileMode.Create))
            {
                using (var bw = new BinaryWriter(s))
                {
                    bw.Write(fileByte);
                }
            }
        }
        /// <summary>
        /// 得到文件绝对路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual string GetAbsoluteFileName(string file)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            return string.Format("{0}{1}",dir, file);
        }
        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        /// <param name="fileName"></param>
        protected virtual void CheckDirectoryAndDeleteFile(string fileName)
        {
            var index = fileName.LastIndexOf(@"/");
            string dir = fileName.Substring(0, index);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            else
            {
                var directory = new DirectoryInfo(dir);
                var pattern = fileName.Substring(index + 1, fileName.Length - index - 1);
                var extension = Path.GetExtension(pattern);
                if(!string.IsNullOrEmpty(extension))
                    pattern = pattern.Replace(extension, "");
                var files = directory.GetFiles(string.Format("{0}.*", pattern));
                foreach (var file in files)
                {
                    if(file.Exists)
                        file.Delete();
                }
            }
        }

        #endregion
        static FileStore()
        {
            StartTask($"{AppDomain.CurrentDomain.BaseDirectory}/temp");
        }
        private static void StartTask(string path)
        {
            var task = new Thread(() => { ExecuteRemoveTemp(path); });
            task.Start();
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        private static void ExecuteRemoveTemp(string path)
        {
            RemoveFiles(path);
            System.Threading.Thread.Sleep(1000 * 60 * 60 * 24);
            ExecuteRemoveTemp(path);
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        private static void RemoveFiles(string path)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
                return;
            var files = di.GetFiles();
            foreach (var file in files)
            {
                if (file.Extension=="temp" && (DateTime.Now - file.CreationTime).TotalDays > 10)
                    File.Delete(file.FullName);
            }
            var dis = di.GetDirectories();
            foreach (var info in dis)
            {
                RemoveFiles(info.FullName);
            }
            if(di.GetFiles().Length==0 && di.GetDirectories().Length==0)
                di.Delete();
        }

    }
}
