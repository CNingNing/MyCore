using System;
using System.Collections.Generic;
using System.Text;
using MimeKit;
using MimeKit.Text;

namespace Winner.Mail
{
    public class Mail : IMail
    {
        /// <summary>
        /// 服务器
        /// </summary>
        public IList<ServerInfo> Servers { get; set; }

        #region 接口的实现

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="info"></param>
        public virtual bool Send(MailInfo info)
        {
            if(info==null)return false;
            var server = GetServer(info);
            if(server==null)return false;

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                if (server.EnableSsl)
                {
                    var port = server.Port == 0 ? 465 : server.Port;
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    smtp.Connect(server.SmtpHost, port, true);
                }
                else
                {
                    smtp.Connect(server.SmtpHost, 25, false);
                }
                smtp.Authenticate(server.UserName, server.Password);
                MimeMessage message = new MimeMessage();
                //发送方
                message.From.Add(new MailboxAddress(Encoding.UTF8,server.DisplayName, server.FromMail));
                //接受方
                foreach (string toMail in info.ToMails)
                    message.To.Add(new MailboxAddress(toMail));
                //标题
                message.Subject = info.Subject;
                var multipart = new Multipart("mixed");
                //文字内容
                if (info.IsBodyHtml)
                {
                    var textPart = new TextPart(TextFormat.Html)
                    {
                        Text = info.Body
                    };
                    multipart.Add(textPart);
                }
                else
                {
                    var textPart = new TextPart(TextFormat.Plain)
                    {
                        Text = info.Body
                    };
                    multipart.Add(textPart);
                }

                message.Body = multipart;
                smtp.Send(message);
                smtp.Disconnect(true);

            }
            return true;
        }

        /// <summary>
        /// 得到服务地址
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected virtual ServerInfo GetServer(MailInfo info)
        {
            var servers = info.Servers ?? Servers;
            if (servers == null || servers.Count == 0)
                return null;
            var random = new Random(DateTime.Now.Second);
            var index = random.Next(0, servers.Count - 1);
            info.SendServer = servers[index];
            return info.SendServer;
        }

        #endregion

     
    }
}
