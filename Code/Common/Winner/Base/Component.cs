using System;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace Winner.Base
{
    public class Component : IComponent
    {


        #region 接口的实现

    
        /// <summary>
        /// 得到页面内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual string GetPageContent(string url, string encoding)
        {

            WebRequest request = WebRequest.Create(url);
            request.Method = "Get";
            return GetPageContent(request, encoding);
        }
        /// <summary>
        /// 得到文件内容
        /// </summary>
        /// <param name="request"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual string GetPageContent(WebRequest request, string encoding)
        {
            using (WebResponse response = request.GetResponse())
            {
                var stream = response.GetResponseStream();
                if (stream == null) return null;
                using (var reader = new StreamReader(stream, Encoding.GetEncoding(encoding)))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 得到验证码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public virtual byte[] CreateCodeImage(string code)
        {
            var image = new Bitmap((int)Math.Ceiling(code.Length * 16.0), 24);
            Graphics g = Graphics.FromImage(image);
            try
            {
                var random = new Random();
                g.Clear(Color.White);
                var penColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                for (int i = 0; i < 5; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(penColor), x1, y1, x2, y2);
                }
                var font = new Font("Arial", 16, (FontStyle.Bold | FontStyle.Italic));
                var color1 = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                var color2 = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                var brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                                                    color1, color2, 1.2f, true);
                g.DrawString(code, font, brush, 3, 2);
                for (int i = 0; i < 40; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                g.DrawRectangle(new Pen(Color.FromArgb(random.Next(256), random.Next(256), random.Next(256))), 0, 0, image.Width - 1, image.Height - 1);
                var stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        #endregion

     
    }
}
