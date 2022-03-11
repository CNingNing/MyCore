using System;
using Winner.Channel;
using Winner.Log;

namespace Winner.Cache
{
    public class CacheService 
    {
     
        #region 属性

        /// <summary>
        /// 缓存实例
        /// </summary>
        public ICache Cache { get; set; }=new LocalCache();
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

        public virtual void Handle(ChannelArgsInfo args)
        {
            try
            {
                var value = args.GetResult();
                if (string.IsNullOrWhiteSpace(value))
                    return;
                var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<CacheProtocolInfo>(value);
                if (protocol == null || string.IsNullOrWhiteSpace(protocol.Key))
                    return;
                switch (args.Method)
                {
                    case 'g':
                        args.SetArgs(Cache.Get<string>(protocol.Key));
                        break;
                    case 'r':
                        args.SetArgs(Cache.Remove(protocol.Key).ToString())
                            ;
                        break;
                    case 'p':
                        args.SetArgs(Cache.Set(protocol.Key, protocol.Value, protocol.Timespan).ToString())
                            ;
                        break;
                    case 'd':
                        args.SetArgs(Cache.Set(protocol.Key, protocol.Value, protocol.DateTime).ToString())
                            ;
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
            }
        }



    }
}
