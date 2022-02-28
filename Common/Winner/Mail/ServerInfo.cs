
using System;

namespace Winner.Mail
{
    [Serializable]
    public class ServerInfo
    {
        public string DisplayName { get; set; }
        /// <summary>
        /// 发送邮箱
        /// </summary>
        public string FromMail { get; set; }
        public int Port { get; set; }
       /// <summary>
       /// 
       /// </summary>
        public bool EnableSsl { get; set; }
        /// <summary>
        /// Smtp
        /// </summary>
        public string SmtpHost { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }
}
