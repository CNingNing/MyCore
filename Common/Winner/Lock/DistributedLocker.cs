using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Winner.Channel;
using Winner.Log;


namespace Winner.Lock
{

    public class DistributedLocker : ILocker
    {
        #region 属性
   
        /// <summary>
        /// 服务实例
        /// </summary>
        public IChannelClient ChannelClient { get; set; }
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
        #endregion

        #region 接口的实现
        public virtual bool Create(LockerInfo info)
        {
            var value = Handle('c', new LockerProtocolInfo { Key = info.Key,ExpireSecond=info.ExpireSecond , IsOptimisticLock = info.IsOptimisticLock });
            if (value == null)
                return info.IsOptimisticLock ? true: false;
            return bool.Parse(value);
        }


        /// <summary>
        /// 得到对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool Release(string key)
        {
            var value = Handle('r', new LockerProtocolInfo { Key = key });
            if (value == null)
                return false;
            return bool.Parse(value);
        }

      
        #endregion

        #region 方法

        private const string ChannelName = "LockerService";

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        protected virtual string Handle(char method, LockerProtocolInfo protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol.Key))
                return null;
            var endPoints = ChannelClient.GetEndPoints(ChannelName);
            var endPoint = GetEndPoint(endPoints, protocol.Key);
            var args=new ChannelArgsInfo{Method=method};
            var value= SerializeJson(protocol);
            args.SetArgs(value);
            ChannelClient.Send(endPoint, args);
            var result = args.GetResult();
            return result;
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

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        protected virtual object DeserializeJson(string input, Type type)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return null;
                return Newtonsoft.Json.JsonConvert.DeserializeObject(input, type);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                return null;
            }
        }
        #endregion

        #region 计算Key值
        /// <summary>
        /// 根据key得到要存放的服务器
        /// </summary>
        /// <param name="endPoints"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual EndPointInfo GetEndPoint(IList<EndPointInfo> endPoints, string key)
        {
            if (endPoints == null || endPoints.Count == 0)
                return null;
            long hash = GenerateLongId(key);
            var index = (int) (hash % endPoints.Count);
            return endPoints[index];
        }
        /// <summary>
        /// 得到缓存值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual long GenerateLongId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            byte[] buffer = Encoding.Default.GetBytes(EncryptMd5(input));
            var i = BitConverter.ToInt64(buffer, 0);
            return i;
        }
        /// <summary>
        /// 得到MD5加密
        /// </summary>
        /// <returns></returns>
        protected virtual string EncryptMd5(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var md5 = MD5.Create();
            byte[] bytValue = Encoding.UTF8.GetBytes(input);
            byte[] bytHash = md5.ComputeHash(bytValue);
            var sTemp = new StringBuilder();
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp.Append(bytHash[i].ToString("X").PadLeft(2, '0'));
            }
            return sTemp.ToString().ToLower();
        }
        #endregion
    }
}
