using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using mylibrary.DTOs;

namespace mylibrary.Utility;

public interface ICommunicationManager
{
    public void  SendEmailAsync (MailRequest mailRequest);
}

public class CommunicationManager: ICommunicationManager
{
    private readonly MailSetting _mailSettings;
    public CommunicationManager(IOptions<MailSetting> mailSettings)
	{
        _mailSettings = mailSettings.Value;
    }

    public void SendEmailAsync(MailRequest mailRequest)
    {

        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        email.To.Add(MailboxAddress.Parse(AESCryptography.Decrypt(mailRequest.ToEmail)));
        email.Subject = mailRequest.Subject;

        ICredentialsByHost credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);

        var builder = new BodyBuilder();
        if (mailRequest.Attachments != null)
        {
            byte[] fileBytes;
            foreach (var file in mailRequest.Attachments)
            {
                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                    builder.Attachments.Add(file.FileName, fileBytes, MimeKit.ContentType.Parse(file.ContentType));
                }
            }
        }
        builder.HtmlBody = mailRequest.Body;
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, false);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        smtp.Send(email);
        smtp.Disconnect(true);
    }

}

