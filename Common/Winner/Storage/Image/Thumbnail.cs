using System;
using System.DrawingCore;
using System.IO;

namespace Winner.Storage.Image
{
    public class Thumbnail: IThumbnail
    {
        protected virtual string RootPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public virtual void Set(ThumbnailInfo info)
        {
            if(info==null || string.IsNullOrWhiteSpace(info.FileName))
                return;
            OutputFile(info);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public virtual void Create(ThumbnailInfo info)
        {
            if(info.Output==null)
                return;
            using (var outputStream=new MemoryStream(info.Output))
            {
                using (var newImage = new Bitmap(info.Width, info.Height))
                {
                    using (var img = (Bitmap)Bitmap.FromStream(outputStream))
                    {
                        Graphics gs = Graphics.FromImage(newImage);
                        gs.InterpolationMode = System.DrawingCore.Drawing2D.InterpolationMode.Default;
                        gs.SmoothingMode = System.DrawingCore.Drawing2D.SmoothingMode.HighQuality;
                        gs.DrawImage(img, new Rectangle(0, 0, info.Width, info.Height), new Rectangle(0, 0, img.Width, img.Height),
                            GraphicsUnit.Pixel);
                        gs.Dispose();

                    }
                    var format = SetContentType(info);
                    using (var ms = new MemoryStream())
                    {
                        newImage.Save(ms, format);
                        ms.Position = 0;
                        info.Output = new byte[ms.Length];
                        ms.Read(info.Output, 0, info.Output.Length);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public virtual void SetRootPath(string path)
        {
            RootPath = path;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="info"></param>
        protected virtual void OutputFile(ThumbnailInfo info)
        {
            string fileName = Path.Combine(RootPath, info.FileName);
            if (!File.Exists(fileName))
                return;
            if (info.Width <= 0 || info.Height <= 0)
            {
                info.ContentType = "image/jpeg";
                using (var fileStream = new FileStream(fileName, FileMode.Open))
                {
                    info.Output = new byte[fileStream.Length];
                    fileStream.Read(info.Output, 0, info.Output.Length);
                }
            }
            else
            {
                using (var newImage = new Bitmap(info.Width, info.Height))
                {
                    using (var img = (Bitmap)Bitmap.FromFile(fileName))
                    {
                        Graphics gs = Graphics.FromImage(newImage);
                        gs.InterpolationMode = System.DrawingCore.Drawing2D.InterpolationMode.Default;
                        gs.SmoothingMode = System.DrawingCore.Drawing2D.SmoothingMode.HighQuality;
                        gs.DrawImage(img, new Rectangle(0, 0, info.Width, info.Height), new Rectangle(0, 0, img.Width, img.Height),
                            GraphicsUnit.Pixel);
                        gs.Dispose();

                    }
                    var format = SetContentType(info);
                    using (var ms = new MemoryStream())
                    {
                        newImage.Save(ms, format);
                        ms.Position = 0;
                        info.Output = new byte[ms.Length];
                        ms.Read(info.Output, 0, info.Output.Length);
                    }
                }
            }

        }
        
        /// <summary>
        /// 设置类型
        /// </summary>
        /// <param name="info"></param>
        protected virtual System.DrawingCore.Imaging.ImageFormat SetContentType(ThumbnailInfo info)
        {
            var contentType = "image/jpeg";
            string ext = Path.GetExtension(info.FileName).ToLower();
            System.DrawingCore.Imaging.ImageFormat format = System.DrawingCore.Imaging.ImageFormat.MemoryBmp;
            switch (ext)
            {
                case ".gif": contentType = "image/gif"; format = System.DrawingCore.Imaging.ImageFormat.Gif; break;
                case ".jpg": contentType = "image/jpg"; format = System.DrawingCore.Imaging.ImageFormat.Jpeg; break;
                case ".png": contentType = "image/png"; format = System.DrawingCore.Imaging.ImageFormat.Png; break;
                case ".bmp": contentType = "image/bmp"; format = System.DrawingCore.Imaging.ImageFormat.Bmp; break;
                case ".jpeg": contentType = "image/jpeg"; format = System.DrawingCore.Imaging.ImageFormat.Jpeg; break;
                case ".emf": contentType = "image/emf"; format = System.DrawingCore.Imaging.ImageFormat.Emf; break;
                case ".exif": contentType = "image/exif"; format = System.DrawingCore.Imaging.ImageFormat.Exif; break;
                case ".icon": contentType = "image/icon"; format = System.DrawingCore.Imaging.ImageFormat.Icon; break;
                case ".tiff": contentType = "image/tiff"; format = System.DrawingCore.Imaging.ImageFormat.Tiff; break;
                case ".wmf": contentType = "image/wmf"; format = System.DrawingCore.Imaging.ImageFormat.Wmf; break;
            }
            info.ContentType = contentType;
            return format;
        }
    }
}
