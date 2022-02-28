using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Winner.Log;

namespace Winner.Channel
{
    public class ChannelClient : IChannelClient
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


        /// <summary>
        /// 节点
        /// </summary>
        public static IDictionary<string, IList<EndPointInfo>> EndPoints { get; set; }

        /// <summary>
        /// 得到节点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual IList<EndPointInfo> GetEndPoints(string name)
        {
            if (EndPoints==null || !EndPoints.ContainsKey(name))
                return null;
            return EndPoints[name];
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual ChannelArgsInfo Send(string name, ChannelArgsInfo args)
        {
            if (EndPoints == null || !EndPoints.ContainsKey(name))
                return null;
            return Invoke(EndPoints[name].GetBestEndPoint().GetFailoverEndPoint(), args);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoints"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ChannelArgsInfo Send(IList<EndPointInfo> endPoints, ChannelArgsInfo args)
        {
            if (endPoints == null || args.Args==null)
                return null;
            EndPointInfo endPoint = endPoints.GetBestEndPoint().GetFailoverEndPoint();
            return Invoke(endPoint, args);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ChannelArgsInfo Send(EndPointInfo endPoint, ChannelArgsInfo args)
        {
            if (endPoint == null || args.Args == null)
                return null;
            return Invoke(endPoint.GetFailoverEndPoint(), args);
        }

        public static object Locker = new object();
        public static IDictionary<string, object> Lockers = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// 得到锁
        /// </summary>
        /// <returns></returns>
        protected virtual object GetLocker(string name)
        {
            if (Lockers.ContainsKey(name))
                return Lockers[name];
            lock (Locker)
            {
                if (Lockers.ContainsKey(name))
                    return Lockers[name];
                Lockers.Add(name,new object());
                return Lockers[name];
            }
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual ChannelArgsInfo Invoke(EndPointInfo endPoint, ChannelArgsInfo args)
        {
            var locker = GetLocker(endPoint.ConnectName);
            try
            {
                string key = null;
                lock (locker)
                {
                    key = args.IsReturn ? endPoint.TrySetGetKey(args) : null;
                    endPoint.IsException = false;
                    endPoint.Use();
                    SetEndPoint(endPoint);
                    endPoint.Send(args);
                }
                if (args.IsReturn && !string.IsNullOrWhiteSpace(key))
                {
                    var rev = endPoint.Get(args, key);
                    if (rev != null)
                        args.Result = rev.Result;
                }
                return args;

            }
            catch (Exception ex)
            {
                lock (locker)
                {
                    TryClose(endPoint);
                }
                var localAddress = endPoint?.Socket?.LocalEndPoint == null ? null : endPoint?.Socket?.LocalEndPoint as IPEndPoint;
                var clientIp = localAddress?.Address;
                var clientPort = localAddress?.Port;
                Log.AddException(new Exception($"{endPoint.Name}-{endPoint.Ip}:{endPoint.Port},Client:{clientIp}:{clientPort} {ex.Message}", ex));
                //if (!endPoint.IsStartKeepAlive)
                //{
                //    var task = new Task(() => { CheckKeepAlive(endPoint); });
                //    task.Start();
                //}

                return null;
            }
            finally
            {
                endPoint.Release();
            }
        }

        /// <summary>
        /// 设置节点
        /// </summary>
        /// <param name="endPoint"></param>
        protected virtual void SetEndPoint(EndPointInfo endPoint)
        {
            if (endPoint.Socket != null)
                return;
            try
            {
                CreateClientSocket(endPoint);
                StartReceive(endPoint);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
         
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        protected virtual void TryClose(EndPointInfo endPoint)
        {
            try
            {
                endPoint.IsException = true;
                if (endPoint.Socket != null)
                {
                    try
                    {
                        endPoint.Socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception e)
                    {

                    }

                    try
                    {
                        endPoint.Socket.Close();
                    }
                    catch (Exception e)
                    {

                    }

                    try
                    {
                        endPoint.Socket.Dispose();
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
                endPoint.Socket = null;
                endPoint.ClearCacheInstance();
            }
         
        }
        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="endPoint"></param>

        protected virtual void CreateClientSocket(EndPointInfo endPoint)
        {
            IPAddress ip = IPAddress.Parse(endPoint.Ip);
            endPoint.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPoint.Socket.Connect(new IPEndPoint(ip, endPoint.Port));
            if(endPoint.BufferSize>0)
            {
                endPoint.Socket.ReceiveBufferSize = endPoint.BufferSize;
                endPoint.Socket.SendBufferSize = endPoint.BufferSize;
            }
        }
        /// <summary>
        /// 开启接受
        /// </summary>
        public virtual void StartReceive(EndPointInfo endPoint)
        {
            var task = new Thread(() =>
            {
                try
                {
                    endPoint.Receive(endPoint.Set);
                }
                catch (Exception ex)
                {
                    Log.AddException(ex);
                    //if (!endPoint.IsStartKeepAlive)
                    //{
                    //    CheckKeepAlive(endPoint);
                    //}
                }
            });
            task.Start();
        }
        //static private readonly object CheckAliveLocker = new object();
        ///// <summary>
        ///// 检查连接一次
        ///// </summary>
        ///// <param name="endPoint"></param>
        //protected virtual void CheckKeepAlive(EndPointInfo endPoint)
        //{
        //    lock (CheckAliveLocker)
        //    {
        //        if (endPoint.IsStartKeepAlive || !endPoint.IsException)
        //            return;
        //        endPoint.IsStartKeepAlive = true;
        //    }
        //    Thread.Sleep(endPoint.KeepAliveTimes);
        //    lock (GetLocker(endPoint.ConnectName))
        //    {
        //        try
        //        {

        //            CreateClientSocket(endPoint);
        //            endPoint.Socket.Send(ChannelInfo.GetSniffPackage());
        //            endPoint.IsException = false;
        //            endPoint.IsStartKeepAlive = false;
        //        }
        //        catch (Exception ex)
        //        {
        //            TryClose(endPoint);
        //            Log.AddException(ex);
        //            endPoint.IsStartKeepAlive = false;
        //            var task = new Task(() => { CheckKeepAlive(endPoint); });
        //            task.Start();
        //        }
        //    }
        //}

    }
}
