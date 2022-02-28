using System;

namespace Winner.Storage.Route
{
    [Serializable]
    public class FileRouteInfo
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }

 

        /// <summary>
        /// 全路径
        /// </summary>
        public string GetFullPath()
        {
            var dt = DateTime.Now;
            return string.Format("{0}{1}/", Path, dt.ToString("yyyy/MM/dd/HH/mm/ss"));
        
        }

   
    }
}
