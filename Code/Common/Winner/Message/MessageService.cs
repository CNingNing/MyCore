using System;
using Winner.Channel;
using Winner.Log;

namespace Winner.Message
{
    public class MessageService
    {
     
        #region 属性

        /// <summary>
        /// 缓存实例
        /// </summary>
        public IMessage Message { get; set; }=new LocalMessage();
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
                var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageProtocolInfo>(value);
                if (protocol == null || string.IsNullOrWhiteSpace(protocol.Key))
                    return;
                switch (args.Method)
                {
                    case 's':
                        args.SetArgs(Message.SetCount(new MessageInfo{Key= protocol.Key,Count=protocol.Count,ExpireSecond=protocol.ExpireSecond }).ToString());
                        break;
                    case 'g':
                        args.SetArgs(Message.GetCount(protocol.Key).ToString());
                        break;
                    case 'r':
                        args.SetArgs(Message.RemoveCount(new MessageInfo { Key = protocol.Key, Count = protocol.Count, ExpireSecond = protocol.ExpireSecond }).ToString())
                            ;
                        break;
                    case 'a':
                        args.SetArgs(Message.AddCount(new MessageInfo { Key = protocol.Key, Count = protocol.Count, ExpireSecond = protocol.ExpireSecond }).ToString())
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
