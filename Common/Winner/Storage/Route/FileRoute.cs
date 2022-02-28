using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Winner.Storage.Distributed;

namespace Winner.Storage.Route
{
    public class FileRoute:IFileRoute
    {
        /// <summary>
        /// 文件路由
        /// </summary>
        public IList<FileRouteInfo> FileRoutes { get; set; }
   
        /// <summary>
        /// 同步
        /// </summary>
        public IMaster Master { get; set; }
        #region 接口实现



        /// <summary>
        /// 生成唯一文件名
        /// </summary>
        /// <returns></returns>
        public virtual string CreateFileName(string fileName)
        {

            var fileRoute = GetFileRouteInfo(fileName);
            if (fileRoute == null)
                return null;
            fileName = string.Format("{0}{1}{2}", fileRoute.GetFullPath(), Guid.NewGuid().ToString("N"), Path.GetExtension(fileName));
            return fileName.ToLower();
        }



        /// <summary>
        /// 得到路由
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected virtual FileRouteInfo GetFileRouteInfo(string fileName)
        {
            if (FileRoutes == null)
                return null;
            return FileRoutes.FirstOrDefault(it => fileName.Contains(it.Path));
        }
     

        #endregion

        
    }
}
