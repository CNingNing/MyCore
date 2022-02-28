using System;
using Winner.Channel;
using Winner.Log;

namespace Winner.Lock
{
    public class LockerService
    {
     
        #region 属性

        /// <summary>
        /// 缓存实例
        /// </summary>
        public ILocker Locker { get; set; }=new LocalLocker();
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
                var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<LockerProtocolInfo>(value);
                if (protocol == null || string.IsNullOrWhiteSpace(protocol.Key))
                    return;
                switch (args.Method)
                {
                    case 'c':
                        args.SetArgs(Locker.Create(new LockerInfo{Key= protocol.Key,ExpireSecond=protocol.ExpireSecond,IsOptimisticLock = protocol.IsOptimisticLock }).ToString());
                        break;
             
                    case 'r':
                        args.SetArgs(Locker.Release(protocol.Key).ToString());
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
