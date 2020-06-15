using MailService.WebApi.Models;
using MailService.WebApi.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MailService.WebApi.Services
{
    // if this is from gmail you will need to turn on LSA for your goggle acct
    // https://myaccount.google.com/lesssecureapps

    public class MailService : IMailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings> mailSettings, ILogger<MailService> logger)
        {
            _logger = logger;
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            try {
                _logger.LogInformation("In SendEmailAsync with");
                _logger.LogInformation("{@MailRequest}", mailRequest);
                _logger.LogInformation("{@MailSettings}", _mailSettings);
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(_mailSettings.Mail);
                message.To.Add(new MailAddress(mailRequest.ToEmail));
                message.Subject = mailRequest.Subject;
                if (mailRequest.Attachments != null)
                {
                    foreach (var file in mailRequest.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                Attachment att = new Attachment(new MemoryStream(fileBytes), file.FileName);
                                message.Attachments.Add(att);
                            }
                        }
                    }
                }
                message.IsBodyHtml = false;
                message.Body = mailRequest.Body;
                smtp.Port = _mailSettings.Port;
                smtp.Host = _mailSettings.Host;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                await smtp.SendMailAsync(message);
            } 
            catch (Exception ex) {
                    _logger.LogInformation("well this was clearly unplanned :(");
                    _logger.LogInformation(ex.Message);
            }

        }

        public async Task SendWelcomeEmailAsync(WelcomeRequest request)
        {
            string FilePath = Directory.GetCurrentDirectory()+"\\Templates\\WelcomeTemplate.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[username]", request.UserName).Replace("[email]", request.ToEmail);
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName);
            message.To.Add(new MailAddress(request.ToEmail));
            message.Subject = $"Welcome {request.UserName}";
            message.IsBodyHtml = true;
            message.Body = MailText;
            smtp.Port = _mailSettings.Port;
            smtp.Host = _mailSettings.Host;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            await smtp.SendMailAsync(message);
        }
    }
}
