namespace Winner.Channel
{
    public class ReceiveInfo: ChannelInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
 
        /// <summary>
        /// 监听对象
        /// </summary>
        public ListenInfo Listen { get; set; }
   
    }

}
