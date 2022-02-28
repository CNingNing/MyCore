using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Winner.Log;

namespace Winner.Channel
{
    public class ChannelInfo
    {


        /// <summary>
        /// 是否为异常
        /// </summary>
        public virtual bool IsException { get; set; }

        /// <summary>
        /// 客户端连接
        /// </summary>
        public virtual Socket Socket { get; set; }
        protected int SendId { get; set; }

        protected virtual int GetSendId()
        {
            if (SendId >= int.MaxValue)
                SendId = 1;
            else
                SendId = SendId + 1;
            return SendId;
        }

        #region 压缩和解压缩
        /// <summary>
        /// 设置压缩
        /// </summary>
        public virtual void CompressArgs(ChannelArgsInfo args)
        {
            args.IsCompress = args.Args != null && args.Args.Length > 100;
            if (args.IsCompress)
                args.Args = args.Args.Compress();
        }
        /// <summary>
        /// 设置压缩
        /// </summary>
        public virtual void DecompressResult(ChannelArgsInfo args)
        {
            if (args.IsCompress)
                args.Result = args.Result.Decompress();
        }
        #endregion


        #region 发送
        /// <summary>
        /// 得到发送包
        /// </summary>
        public MemoryStream SendPackageStream { get; set; }
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="args"></param>
        public virtual void Send(ChannelArgsInfo args)
        {
            try
            {
                args.CompressArgs();
                SetSendPackage(args);
                Socket.NoDelay = true;
                byte[] package;
                do
                {
                    if (Socket == null || IsException)
                        throw new Exception("Send Error");
                    package = GetSendPackage();
                    if (package == null)
                        break;
                    Socket.Send(package);

                } while (SendPackageStream!=null && SendPackageStream.Position < SendPackageStream.Length);

            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Send:IsException:{IsException},Message:{ex.Message}",
                    ex);
            }
            finally
            {
                if(SendPackageStream!=null)
                    SendPackageStream.Dispose();
                SendPackageStream = null;
            }
       
        }
        /// <summary>
        /// 得到发送包
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetSendPackage()
        {
            if (SendPackageStream == null)
                return null;
            var len = PackageBufferSize > SendPackageStream.Length - SendPackageStream.Position
                ? SendPackageStream.Length - SendPackageStream.Position
                : PackageBufferSize;
            var buffer = new byte[len];
            SendPackageStream.Read(buffer, 0, (int)len);
            return buffer;
        }

        /// <summary>
        /// 设置接受
        /// </summary>
        public virtual void SetSendPackage(ChannelArgsInfo args)
        {
            if (args.Args == null)
                return;
            SendPackageStream = new MemoryStream();
            if (args.Args.Length == 2 && args.Args[0] == 0x02 && args.Args[1] == 0x03)
            {
                SendPackageStream.Write(args.Args, 0, 2);
                SendPackageStream.Position = 0;
                return;
            }          
            args.Length = args.Args.Length + 12;
            var keyBytes = BitConverter.GetBytes(args.SendId);
            SendPackageStream.WriteByte(0x02);
            SendPackageStream.WriteByte(args.IsCompress ? (byte)0x01 : (byte)0x00);
            SendPackageStream.Write(BitConverter.GetBytes(args.Length), 0, 4);
            SendPackageStream.Write(keyBytes, 0, 4);
            SendPackageStream.WriteByte((byte)args.Method);
            SendPackageStream.Write(args.Args, 0, args.Args.Length);
            SendPackageStream.WriteByte(0x03);
            SendPackageStream.Position = 0;
        }

        #endregion

        #region 处理接受
        public ChannelArgsInfo ReceiveArgs { get; set; }
        /// <summary>
        /// 得到发送包
        /// </summary>
        public MemoryStream ReceivePackageStream { get; set; }
        public bool IsRunReceive { get; set; }
        /// <summary>
        /// 发送
        /// </summary>
        public virtual void Receive(Action<ChannelArgsInfo> handle)
        {
            if(IsRunReceive)
                return;
            IsRunReceive = true;
            try
            {
                Socket.NoDelay = true;
                byte[] package;
                while (true)
                {
                    if (Socket == null || IsException)
                        throw new Exception("Receive Error");
                    package = new byte[PackageBufferSize];
                    var length = Socket.Receive(package);
                    if (length == 0)
                        throw new Exception("Length Is 0");
                    ReceivePackage(handle, package, length, 0);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Receive:IsException:{IsException},Message:{ex.Message}",
                    ex);
            }
            finally
            {
                IsRunReceive = false;
                if(ReceivePackageStream != null)
                    ReceivePackageStream.Dispose();
                ReceivePackageStream = null;
            }
           
        }

        /// <summary>
        /// 得到发送包
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="package"></param>
        /// <param name="length"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public virtual void ReceivePackage(Action<ChannelArgsInfo> handle,byte[] package, int length,int startIndex)
        {
            if (package == null || length == 0 || startIndex>= length)
                return ;
            if (ReceiveArgs == null && package[startIndex]== 0x02)
            {
                while (startIndex + 1 < length && package[startIndex] == 0x02 && package[startIndex + 1] == 0x03)//过滤心跳包
                {
                    startIndex += 2;
                }
                if (startIndex >= length || package[startIndex] != 0x02)
                    return ;
                ReceivePackageStream = new MemoryStream();
                
            }

            if (ReceivePackageStream != null)
            {
                while (startIndex < length)
                {
                    ReceivePackageStream.WriteByte(package[startIndex]);
                    if (CheckReceiveArgs(handle))
                    {
                        ReceivePackage(handle, package, length, startIndex+1);
                        break;
                    }
                    startIndex++;
                }
            }
       
        }
        /// <summary>
        /// 设置接受
        /// </summary>
        public virtual bool CheckReceiveArgs(Action<ChannelArgsInfo> handle)
        {
            if (ReceivePackageStream != null && ReceivePackageStream.Length >= 11 && ReceiveArgs == null)
            {
                var args = new ChannelArgsInfo();
                ReceivePackageStream.Position = 1;
                args.IsCompress = ReceivePackageStream.ReadByte() == 0x01;
                var lenbs = new byte[4];
                ReceivePackageStream.Read(lenbs, 0, 4);
                args.Length = BitConverter.ToInt32(lenbs, 0);
                var bs = new byte[4];
                ReceivePackageStream.Read(bs, 0, 4);
                args.SendId = BitConverter.ToInt32(bs, 0);
                args.Method = (char)ReceivePackageStream.ReadByte();
                args.Result = new byte[args.Length - 12];
                ReceiveArgs = args;
                return false;
            }
            if (ReceiveArgs != null && ReceivePackageStream != null && ReceivePackageStream.Length == ReceiveArgs.Length)
            {
                ReceivePackageStream.Position = 11;
                ReceivePackageStream.Read(ReceiveArgs.Result, 0, ReceiveArgs.Result.Length);
                DecompressResult(ReceiveArgs);
                handle(ReceiveArgs);
                if (ReceivePackageStream != null)
                    ReceivePackageStream.Dispose();
                ReceivePackageStream = null;
                ReceiveArgs = null;
                return true;
            }
            return false;
        }

        #endregion


        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        public static byte[] GetSniffPackage()
        {
            //0开始符，1是否压缩，2-5包长度,6结束符
            return new byte[] { 0x02, 0x03 };
        }

        public const int PackageBufferSize =128;

     
    
    }
}
