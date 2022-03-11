using System;
using System.Collections.Generic;

namespace Winner.Channel
{
    public interface IChannelClient
    {
        /// <summary>
        /// 发送
        /// </summary>
        /// <returns></returns>
        IList<EndPointInfo> GetEndPoints(string name);
        /// <summary>
        /// 发送
        /// </summary>
        /// <returns></returns>
        ChannelArgsInfo Send(string name, ChannelArgsInfo args);
        /// <summary>
        /// 发送
        /// </summary>
        /// <returns></returns>
        ChannelArgsInfo Send(IList<EndPointInfo> endPoints, ChannelArgsInfo args);
        /// <summary>
        /// 发送
        /// </summary>
        /// <returns></returns>
        ChannelArgsInfo Send(EndPointInfo endPoint, ChannelArgsInfo args);

    }
}
