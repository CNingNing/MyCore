namespace Winner.Storage.Image
{
    public interface IThumbnail
    {
        /// <summary>
        /// 跟进output创建缩略图
        /// </summary>
        /// <param name="info"></param>
        void Create(ThumbnailInfo info);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        void Set(ThumbnailInfo info);
        /// <summary>
        /// 设置根路径
        /// </summary>
        /// <param name="path"></param>
        void SetRootPath(string path);
    }
}
