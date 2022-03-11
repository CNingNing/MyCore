using System.Text;

namespace Winner.Channel
{
    public class ChannelArgsInfo
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public int SendId { get; set; } = int.MinValue;
        /// <summary>
        /// 方法
        /// </summary>
        public char Method { get; set; }
        /// <summary>
        /// 接收结果
        /// </summary>
        public byte[] Result { get; set; }
        /// <summary>
        /// 发送参数
        /// </summary>
        public byte[] Args { get; set; }
        /// <summary>
        /// 是否压缩
        /// </summary>
        public bool IsCompress { get; set; }
        /// <summary>
        /// 包总长度
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 是否返回
        /// </summary>
        public bool IsReturn { get; set; } = true;
        /// <summary>
        /// 缓存大小
        /// </summary>
        public virtual int Timeout { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReceiveInfo Receive { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetResult()
        {
            if (Result == null)
                return null;
            return Encoding.UTF8.GetString(Result, 0, Result.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual void SetArgs(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return;
            Args = Encoding.UTF8.GetBytes(value);
        }
        /// <summary>
        /// 设置压缩
        /// </summary>
        public virtual void CompressArgs()
        {
            IsCompress = Args != null && Args.Length > 100;
            if (IsCompress)
                Args = Args.Compress();
        }
        /// <summary>
        /// 设置压缩
        /// </summary>
        public virtual void DecompressResult()
        {
            if (IsCompress)
                Result = Result.Decompress();
        }
    
    
    }
}
