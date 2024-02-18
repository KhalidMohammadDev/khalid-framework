using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace Khalid.Core.Framework
{
    /// <summary>
    /// Main Class for sending emails all over the system
    /// </summary>
    public class EmailService
    {







        public static EmailConfiguration EmailConfiguration;
        #region
        public EmailService(EmailConfiguration configuration)
        {

            ////TODO_khalid add file per language on configuration
            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            //                 "Files", "EmailTemplate", "GeneralTemplate.txt");

            //if (!System.IO.File.Exists(filePath))
            //{
            //    throw new FileNotFoundException(filePath, new FileNotFoundException("you must provide email template"));
            //}

            //LanguageToEmailTemplates = new Dictionary<string, string>
            //{
            //    {"en" , System.IO.File.ReadAllText(filePath) }
            //};

            EmailConfiguration = configuration;


        }

        #endregion

        /// <summary>
        /// Sends an email - Overload -
        /// </summary>
        /// <param name="message"><see cref="System.Net.Mail.MailMessage"/></param>
        /// <param name="enableSSl">true to enable SSL on smtpClient</param>

        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="toEmailReceiver"></param>
        /// <param name="emailSubject"></param>
        /// <param name="emailMessage"></param>
        /// <param name="isMessageContainHtml"></param>
        /// <param name="ccEmailReceiver"></param>
        /// <param name="bccEmailReceiver"></param>
        /// <param name="isAsync"></param>
        /// <param name="languageCode">language code {ar , en} must add template file and file path for language in configuration</param>
        /// <returns></returns>


        public async Task SendEmailAsync(string toEmailReceiver, string emailSubject, string emailMessage,
         bool isMessageContainHtml = false)
        {

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(EmailConfiguration.EmailAddress, EmailConfiguration.EmailAddress));
            message.To.Add(new MailboxAddress(toEmailReceiver, toEmailReceiver));
            message.Subject = emailSubject;

            message.Body = new TextPart(isMessageContainHtml ? "html" : "plain")
            {
                Text = emailMessage
            };



            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(EmailConfiguration.SMTPServer, EmailConfiguration.SMTPPort, EmailConfiguration.EnableSsl);

                // Note: only needed if the SMTP server requires authentication
                await client.AuthenticateAsync(EmailConfiguration.EmailAddress, EmailConfiguration.Password);

                var ss = await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
      
    }
}
