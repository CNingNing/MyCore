namespace Winner.Storage.Image
{
    public class ImageThumbnailInfo
    {
        /// <summary>
        /// 标记
        /// </summary>
        public virtual string Flag { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public virtual int Width { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public virtual int Height { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public virtual bool IsUsed { get; set; }
    }
}
