
using System;
using System.Collections.Generic;

namespace Winner.Mail
{
    [Serializable]
    public class MailInfo
    {
   
        /// <summary>
        /// 发送的邮箱地址
        /// </summary>
        public string[] ToMails { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string  Subject { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 是否HTML
        /// </summary>
        public bool IsBodyHtml { get; set; }
        /// <summary>
        /// 服务器设置
        /// </summary>
        public IList<ServerInfo> Servers { get; set; }
        /// <summary>
        /// 发送服务
        /// </summary>
        public ServerInfo SendServer { get; set; }
    }
}
