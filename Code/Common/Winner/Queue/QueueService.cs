using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Winner.Channel;
using Winner.Log;

namespace Winner.Queue
{
    public class QueueService
    {
        #region 属性

        /// <summary>
        /// 缓存实例
        /// </summary>
        public IQueue Queue { get; set; } = new LocalQueue();
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
        private IChannelService _channelService;

        /// <summary>
        /// 实例
        /// </summary>
        public IChannelService ChannelService
        {
            get
            {
                if (_channelService == null)
                    _channelService = Creator.Get<IChannelService>();
                return _channelService;
            }
            set { _channelService = value; }
        }
        /// <summary>
        /// 消息
        /// </summary>
        public static IDictionary<string, IList<string>> Subscribes = new ConcurrentDictionary<string, IList<string>>();

        static object Locker=new object();
        #endregion

        public virtual void Handle(ChannelArgsInfo args)
        {
            try
            {
                var value = args.GetResult();
                if (string.IsNullOrWhiteSpace(value))
                    return;
                var protocol = Newtonsoft.Json.JsonConvert.DeserializeObject<QueueProtocolInfo>(value);
                if (protocol == null || string.IsNullOrWhiteSpace(protocol.Name))
                    return;
                switch (args.Method)
                {
                    case 'o':
                        args.SetArgs(Queue.Open(protocol.Name, new QueueInfo{ExpireSecond= protocol.ExpireSecond,MaxCount= protocol.MaxCount}).ToString());
                        break;
                    case 'c':
                        args.SetArgs(Queue.Close(protocol.Name).ToString());
                        break;
                    case 's':
                        args.SetArgs(Queue.Push(protocol.Name,protocol.Value).ToString());
                        lock (Locker)
                        {
                            if (Subscribes.ContainsKey(protocol.Name))
                            {
                                var recivieNames = Subscribes[protocol.Name];
                                var task = new Thread(() =>
                                {
                                    var receives = ChannelService.GetReceives();
                                    if (receives == null)
                                        return;
                                    foreach (var recivieName in recivieNames)
                                    {
                                        if (!receives.ContainsKey(recivieName))
                                        {
                                            lock (Locker)
                                            {
                                                if (Subscribes.ContainsKey(protocol.Name) && Subscribes[protocol.Name].Contains(recivieName))
                                                {
                                                    Subscribes.Remove(protocol.Name);
                                                }
                                            }
                                            continue;
                                        }
                                        var channelArgs = new ChannelArgsInfo { IsCompress = args.IsCompress, Receive = receives[recivieName], IsReturn = args.IsReturn, SendId = EndPointInfo.UnSetArgsSendId, Method = args.Method };
                                        channelArgs.SetArgs(value);
                                        ChannelService.Send(channelArgs);
                                    }
                                });
                                task.Start();
                            }
                        }
                        
                        break;
                    case 'p':
                        args.SetArgs(Queue.Pop<string>(protocol.Name));
                        break;
                    case 'b':
                        lock (Locker)
                        {
                            if (!Subscribes.ContainsKey(protocol.Name))
                            {
                                Subscribes.Add(protocol.Name,new List<string>());
                            }
                            if (!string.IsNullOrWhiteSpace(args.Receive.Name) && !Subscribes[protocol.Name].Contains(args.Receive.Name))
                            {
                                Subscribes[protocol.Name].Add(args.Receive.Name);
                            }
                            args.SetArgs(true.ToString());
                        }
                        break;
                    case 'u':
                        lock (Locker)
                        {
                            if (Subscribes.ContainsKey(protocol.Name))
                            {
                                args.SetArgs(true.ToString());
                                return;
                            }
                            Subscribes.Remove(protocol.Name);
                            args.SetArgs(true.ToString());
                        }
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
