using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Winner.Log;

namespace Winner.Channel
{
    public class ChannelService:IChannelService
    {
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
        private static readonly object Locker=new object();

        private static IDictionary<string, bool> ListenSwichers { get; set; }=new ConcurrentDictionary<string, bool>();

        private static IDictionary<string, ReceiveInfo> Receives { get; set; } = new ConcurrentDictionary<string, ReceiveInfo>();
        public static IList<ListenInfo> Listens;
        /// <summary>
        /// 开启
        /// </summary>
        /// <returns></returns>
        public virtual bool Start(string name, int port)
        {
            lock (Locker)
            {
                try
                {
                    ThreadPool.SetMinThreads(100, 100);
                    var listen = Listens.FirstOrDefault(it => it.Name == name);
                    if (listen == null)
                        return false;
                    if (ListenSwichers.ContainsKey(name))
                    {
                        ListenSwichers.Remove(name);
                    }
                    ListenSwichers.Add(name, true);
                    if (port == 0)
                    {
                        port = listen.Port;
                    }

                    switch (listen.Ip)
                    {
                        case "Lan":
                        {
                            var ips = GetLanIps();
                            foreach (var ip in ips)
                            {
                                Start(listen, ip, port);
                            }
                        }
                            break;
                        case "Net":
                        {
                            var ips = GetAllIps();
                            foreach (var ip in ips)
                            {
                                Start(listen, ip, port);
                            }
                        }
                            break;
                        default:
                            Start(listen, listen.Ip, port);
                            break;
                    }
                     
                }
                catch (Exception ex)
                {
                    Start(name,port);
                    Log.AddException(ex);
                }
            }
            return true;
        }

        protected virtual void Start(ListenInfo listen, string bindIp, int port)
        {
            IPAddress ip = IPAddress.Parse(bindIp);
            var serverSocket =
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, port));
            serverSocket.Listen(listen.Count);
            var taskListen = new Thread(() => { StartListen(listen, serverSocket); });
            taskListen.Start();
            var task = new Thread(() => { CheckKeepAlive(); });
            task.Start();
        }
        protected virtual string[] GetAllIps()
        {
            var hostIps = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(network => network.OperationalStatus == OperationalStatus.Up)
                .Select(network => network.GetIPProperties())
                .OrderByDescending(properties => properties.GatewayAddresses.Count)
                .SelectMany(properties => properties.UnicastAddresses)
                .Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(it => it.Address.ToString())
                .ToArray();
            return hostIps;
        }
        protected virtual string[] GetLanIps()
        {
            var hostIps = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(network => network.OperationalStatus == OperationalStatus.Up)
                .Select(network => network.GetIPProperties())
                .OrderByDescending(properties => properties.GatewayAddresses.Count)
                .SelectMany(properties => properties.UnicastAddresses)
                .Where(address => !IPAddress.IsLoopback(address.Address) && address.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(it => it.Address.ToString())
                .ToArray();
            return hostIps;
        }
        /// <summary>
        /// 停止 
        /// </summary>
        /// <returns></returns>
        public virtual bool Stop(string name)
        {
            lock (Locker)
            {
                ListenSwichers[name] = false;

            }
            return true;
        }

        public virtual bool Send(ChannelArgsInfo args)
        {
            try
            {
                var receiveName = args.Receive?.Name;
                if (string.IsNullOrWhiteSpace(receiveName))
                    return false;
                lock (GetReceiveLocker(receiveName))
                {
                    try
                    {
                        Receives[args.Receive.Name].Send(args);
                    }
                    catch (Exception e)
                    {
                        var localAddress = Receives[receiveName]?.Socket?.LocalEndPoint as IPEndPoint;
                        var remoteAddress = Receives[receiveName]?.Socket?.RemoteEndPoint as IPEndPoint;
                        Log.AddException(new Exception(
                            $"ReceiChannelService-Send:{localAddress?.Address},RemoteIP:{remoteAddress?.Address},Message:{e.Message}",
                            e));
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.AddException(new Exception(
                    $"ReceiChannelService-Send,Message:{ex.Message}",
                    ex));
            }

            return false;
        }

        public virtual IDictionary<string, ReceiveInfo> GetReceives()
        {
            return Receives;
        }

        #region 创建线程服务
        /// <summary>
        /// 开启监听
        /// </summary>
        /// <returns></returns>
        public virtual void StartListen(ListenInfo listen, Socket serverSocket)
        {
            try
            {
                while (ListenSwichers[listen.Name])
                {
                    var receive = new ReceiveInfo
                    {
                        Name = Guid.NewGuid().ToString("N"),
                        Listen = listen
                    };
                    receive.Socket = serverSocket.Accept();
                    if(listen.BufferSize>0)
                    {
                        receive.Socket.ReceiveBufferSize = listen.BufferSize;
                        receive.Socket.SendBufferSize = listen.BufferSize;
                    }
                    Receives.Add(receive.Name, receive);
                    var task = new Thread(() => { ReceiveMessage(receive); });
                    task.Start();
                }

    
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
            }
            finally
            {
                try
                {
                    if (serverSocket != null)
                    {
                        try
                        {
                            serverSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch (Exception e)
                        {
                            
                        }
                        try
                        {
                            serverSocket.Close();
                        }
                        catch (Exception e)
                        {

                        }
                        try
                        {
                            serverSocket.Dispose();
                        }
                        catch (Exception e)
                        {

                        }
                      
                    }
                }
                catch (Exception e)
                {
                }
            }
            
        }
 
        /// <summary>   
        /// 接收消息  
        /// </summary>  
        /// <param name="receive"></param>  
        protected virtual void ReceiveMessage(ReceiveInfo receive)
        {
            try
            {
                if (receive != null && receive.Socket!=null && receive.Socket.Connected)
                {
                    Action<ChannelArgsInfo> action = (args) => { HandleReceiveArgs(receive, args); };
                    receive.Receive(action);
                }
            }
            catch (Exception ex)
            {
                var localAddress = receive?.Socket?.LocalEndPoint == null ? null : receive?.Socket?.LocalEndPoint as IPEndPoint;
                var remoteAddress = receive?.Socket?.RemoteEndPoint == null ? null : receive?.Socket?.RemoteEndPoint as IPEndPoint;
                Log.AddException(new Exception(
                    $"ReceiveMessage-LocalIP:{localAddress?.Address}:{localAddress?.Port},RemoteIP:{remoteAddress?.Address}:{remoteAddress?.Port},Message:{ex.Message}",
                    ex));
            }
            finally
            {
                if (receive != null)
                    TryClose(receive);
            }
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="receive"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual void HandleReceiveArgs(ReceiveInfo receive, ChannelArgsInfo args)
        {
            if (args == null) return;
            var task = new Thread(() =>
            {
                if(!Receives.ContainsKey(receive.Name))
                    return;
                try
                {
                    if (receive.Listen.Handle != null)
                    {
                        args.Receive = receive;
                        receive.Listen.Handle(args);
                        if (args.IsReturn)
                        {
                            if (args.Args == null)
                                args.Args = new byte[0];
                            lock (GetReceiveLocker(receive.Name))
                            {
                                receive.Send(args);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var localAddress = receive?.Socket?.LocalEndPoint ==null?null: receive?.Socket?.LocalEndPoint as IPEndPoint;
                    var remoteAddress = receive?.Socket?.RemoteEndPoint == null ? null : receive?.Socket?.RemoteEndPoint as IPEndPoint;
                    var addEx = new Exception(
                        $"ReceiveCount:{Receives.Values.Count},ListenPort:{receive?.Listen?.Port},KeepAlive-LocalIP:{localAddress?.Address},RemoteIP:{remoteAddress?.Address},Message:{ex.Message}",
                        ex);
                    Log.AddException(addEx);
                }
            });
            task.Start();
        }

        static object ReceiveLocker=new object();
        protected virtual object GetReceiveLocker(string name)
        {
            lock (ReceiveLocker)
            {
                if (string.IsNullOrWhiteSpace(name) ||!Receives.ContainsKey(name))
                    return new object();
                return Receives[name];
            }
        }

        protected virtual void TryClose(ReceiveInfo receive)
        {
            lock (GetReceiveLocker(receive.Name))
            {
                try
                {
                    if (receive.Socket != null)
                    {
                        try
                        {
                            receive.Socket.Shutdown(SocketShutdown.Both);
                        }
                        catch (Exception e)
                        {

                        }

                        try
                        {
                            receive.Socket.Close();
                        }
                        catch (Exception e)
                        {

                        }

                        try
                        {
                            receive.Socket.Dispose();
                        }
                        catch (Exception e)
                        {

                        }

                    }
                }
                catch (Exception e)
                {

                }
                finally
                {
                    receive.Socket = null;
                    receive.IsException = true;
                    if (Receives.ContainsKey(receive.Name))
                    {
                        Receives.Remove(receive.Name);
                    }
                }
            }
        }
        /// <summary>
        /// 检查心跳包
        /// </summary>
        protected virtual void CheckKeepAlive()
        {
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    var tasks=new List<Task>();
                    var values = Receives.Values.ToArray();
                    foreach (var receive in values)
                    {
                        var task = new Task(() =>
                        {
                            try
                            {
                                if (receive == null)
                                    return;
                                if (Receives.ContainsKey(receive.Name))
                                {
                                    var args = new ChannelArgsInfo { Args = ChannelInfo.GetSniffPackage() };
                                    receive.Send(args);
                                }
                                Thread.Sleep(receive.Listen.KeepAliveTimes);
                            }
                            catch (Exception ex)
                            {
                                TryClose(receive);
                                var localAddress = receive?.Socket?.LocalEndPoint == null ? null : receive?.Socket?.LocalEndPoint as IPEndPoint;
                                var remoteAddress = receive?.Socket?.RemoteEndPoint == null ? null : receive?.Socket?.RemoteEndPoint as IPEndPoint;
                                var addEx = new Exception(
                                    $"ReceiveCount:{Receives.Values.Count},ListenPort:{receive?.Listen?.Port},KeepAlive-LocalIP:{localAddress?.Address},RemoteIP:{remoteAddress?.Address},Message:{ex.Message}",
                                    ex);
                                Log.AddException(addEx);
                              
                            }
                        }, TaskCreationOptions.LongRunning);
                        tasks.Add(task);
                    }
                    if (tasks.Count == 0)
                        continue;
                    foreach (var task in tasks)
                    {
                        task.Start();
                    }
                    Task.WaitAll(tasks.ToArray());
                    //while (true)
                    //{
                    //    Thread.Sleep(1000);
                    //    var rev = true;
                    //    foreach (var task in tasks)
                    //    {
                    //        rev = rev && task.IsCompleted;
                    //    }
                    //    if(rev)
                    //        break;
                    //}
                }
                catch (Exception ex)
                {
                    Log.AddException(ex);
                }
            }
        }
    }
    #endregion
}
