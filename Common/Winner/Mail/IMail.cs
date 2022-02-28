
namespace Winner.Mail
{

    public interface IMail
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="info"></param>
        bool Send(MailInfo info);
    }
}
