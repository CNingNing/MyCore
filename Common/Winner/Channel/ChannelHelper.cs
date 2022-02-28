using System;
using System.IO;
using System.IO.Compression;

namespace Winner.Channel
{
    public static class ChannelHelper
    {
        #region 压缩与解压缩
        /// <summary>
        /// 压缩二进制流
        /// </summary>
        /// <param name="data">需要压缩的二进制流</param>
        /// <see cref="byte"/>
        /// <returns>压缩后二进制流</returns>
        public static byte[] Compress(this byte[] data)
        {
            if (data == null)
                return null;
            using (var ms = new MemoryStream())
            {
                try
                {
                    using (var zip = new DeflateStream(ms, CompressionMode.Compress, true))
                    {
                        zip.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
                catch (Exception ex)
                {
                    return data;
                }

            }
        }
        /// <summary>
        /// 解压缩二进制流
        /// </summary>
        /// <param name="data">需要解压缩的二进制流</param>
        /// <see cref="byte"/>
        /// <returns>解压缩后二进制流</returns>
        public static byte[] Decompress(this byte[] data)
        {
            if (data == null)
                return null;
            using (var ms = new MemoryStream())
            {
                try
                {
                    ms.Write(data, 0, data.Length);
                    ms.Flush();
                    ms.Position = 0;
                    using (var zip = new DeflateStream(ms, CompressionMode.Decompress, true))
                    {
                        using (var os = new MemoryStream())
                        {
                            int size = 1024;
                            byte[] buf = new byte[size];
                            int l = 0;
                            do
                            {
                                l = zip.Read(buf, 0, size);
                                if (l == 0) l = zip.Read(buf, 0, size);
                                os.Write(buf, 0, l);
                            }
                            while (l != 0);
                            return os.ToArray();
                        }

                    }
                }
                catch(Exception ex)
                {
                    return data;
                }

            }
        }
        #endregion
    }
}
