using System;
using System.Collections.Generic;

namespace Winner.Channel
{
    public interface IChannelService
    {
        /// <summary>
        /// 开启
        /// </summary>
        /// <returns></returns>
        bool Start(string name,int port);

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <returns></returns>
        bool Stop(string name);

        /// <summary>
        /// 发送客户端
        /// </summary>
        /// <returns></returns>
        bool Send(ChannelArgsInfo args);

        /// <summary>
        /// 发送客户端
        /// </summary>
        /// <returns></returns>
        IDictionary<string, ReceiveInfo> GetReceives();
    }
}
