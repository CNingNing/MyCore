using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Winner.Channel;
using Winner.Log;

namespace Winner.Storage.Distributed
{
    public class DistributedBase
    {
        public const string ChannelName = "FileService";
        private ILog _log;

        /// <summary>
        /// 实例
        /// </summary>
        public ILog Log
        {
            get
            {
                if (_log == null)
                    _log = Creator.Get<ILog>();
                return _log;
            }
            set { _log = value; }
        }
        /// <summary>
        /// 服务实例
        /// </summary>
        public IChannelClient ChannelClient { get; set; }
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        protected virtual string Handle(EndPointInfo endPoint, char method, StorageProtocolInfo protocol, byte[] fileBytes)
        {
            if (string.IsNullOrWhiteSpace(protocol.FileName) || fileBytes == null || fileBytes.Length == 0)
                return null;
            var args = new ChannelArgsInfo { Method = method };
            using (var ms = new MemoryStream())
            {
                var lenBytes = BitConverter.GetBytes(fileBytes.Length);
                var protocolBytes = Encoding.UTF8.GetBytes(SerializeJson(protocol));
                ms.Write(lenBytes, 0, 4);
                ms.Write(fileBytes, 0, fileBytes.Length);
                ms.Write(protocolBytes, 0, protocolBytes.Length);
                args.Args = ms.ToArray();
            }
            ChannelClient.Send(endPoint, args);
            return args.GetResult();
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        /// <param name="isResult"></param>
        /// <returns></returns>
        protected virtual string Handle(EndPointInfo endPoint, char method, StorageProtocolInfo protocol, bool isResult=true)
        {
            if (string.IsNullOrWhiteSpace(protocol.FileName))
                return null;
            var args = new ChannelArgsInfo {Method = method,IsReturn = isResult };
            var value = SerializeJson(protocol);
            args.SetArgs(value);
            ChannelClient.Send(endPoint, args);
            if (isResult)
                return args.GetResult();
            return null;
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="endPoints"></param>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        protected virtual ChannelArgsInfo Handle(IList<EndPointInfo> endPoints, char method, StorageProtocolInfo protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol.FileName))
                return null;

            var args = new ChannelArgsInfo { Method = method };
            var value = SerializeJson(protocol);
            args.SetArgs(value);
            ChannelClient.Send(endPoints, args);
            return args;
        }
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        protected virtual string SerializeJson(object input)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(input);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                return null;
            }
        }
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        protected virtual T DeserializeJson<T>(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return default(T);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                return default(T);
            }
        }
    }
}
