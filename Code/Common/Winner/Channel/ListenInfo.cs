using System;

namespace Winner.Channel
{
    public class ListenInfo
    {
        /// <summary>
        /// 处理函数
        /// </summary>
        public Action<ChannelArgsInfo> Handle { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 监听数量
        /// </summary>
        public int Count { get; set; } = 200;
        /// <summary>
        /// 缓存大小
        /// </summary>
        public int BufferSize { get; set; }
        /// <summary>
        /// 检查心跳包时间
        /// </summary>
        public int KeepAliveTimes { get; set; } = 1000*30;
        /// <summary>
        /// 缓存大小
        /// </summary>
        public int Timeout { get; set; } = 5 * 1000;

    }
}
