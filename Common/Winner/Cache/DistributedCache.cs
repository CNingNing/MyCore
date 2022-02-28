using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Winner.Channel;
using Winner.Log;


namespace Winner.Cache
{

    public class DistributedCache :  ICache
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

        /// <summary>
        /// 得到缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T Get<T>(string key)
        {
            var value =Handle('g',new CacheProtocolInfo{Key=key});
            if (string.IsNullOrWhiteSpace(value))
                return default(T);
            if (typeof (T).IsValueType || typeof (T) == typeof (string))
            {
                return  (T)(object)value ;
            }
            return DeserializeJson<T>(value);
        }

        /// <summary>
        /// 得到对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object Get(string key, Type type)
        {
            var value = Handle('g',new CacheProtocolInfo {Key = key });
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (type.IsValueType || type == typeof (string))
            {
                return value;
            }
            return DeserializeJson(value, type);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual bool Set<T>(string key, T value, DateTime time)
        {
            if (value==null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;
            string rev;
            if (value.GetType().IsValueType || value is string)
            {
             
                rev = Handle('d',new CacheProtocolInfo {Key = key,Value= value.ToString(),DateTime= time });
            }
            else
            {
                var val = SerializeJson(value);
                rev = Handle('d', new CacheProtocolInfo { Key = key, Value = val, DateTime = time });
            }
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }


        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public virtual bool Set<T>(string key, T value, long timeSpan)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;
            string rev;
            if (value.GetType().IsValueType || value is string)
            {

                rev = Handle('p', new CacheProtocolInfo { Key = key, Value = value.ToString(),Timespan= timeSpan });
            }
            else
            {
                var val = SerializeJson(value);
                rev = Handle('p', new CacheProtocolInfo { Key = key, Value = val, Timespan = timeSpan });
            }
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool Set<T>(string key, T value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;
            string rev;
            if (value.GetType().IsValueType || value is string)
            {

                rev = Handle('d',
                    new CacheProtocolInfo {Key = key, Value = value.ToString(), DateTime = DateTime.Now.AddYears(1)});
            }
            else
            {
                var val = SerializeJson(value);
                rev = Handle('d',
                    new CacheProtocolInfo {Key = key, Value = val, DateTime = DateTime.Now.AddYears(1)});
            }

            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public virtual bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            string rev = Handle('r', new CacheProtocolInfo { Key = key});
            if (string.IsNullOrWhiteSpace(rev))
                return false;
            bool revValue;
            bool.TryParse(rev, out revValue);
            return revValue;

        }


        #endregion

        #region 方法

        private const string ChannelName = "CacheService";

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        protected virtual string Handle(char method, CacheProtocolInfo protocol)
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
