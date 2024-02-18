using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{
    /// <summary>
    /// SMTP email class, derived from Email
    /// </summary>
    internal class SMTP : Email, IDisposable
    {
        private List<string> _To;
        private List<string> _CC;
        private List<string> _Bcc;
        private string _From;
        private string _SMTPServer;
        private int _port { get; set; }
        private string _Subject;
        private string _Body;
        private string _SMTPUser;
        private string _SMTPPassword;
        private string _SMTPDomain;
        private List<MyEmailAttachment> Attachments;
        private bool _UsingSMTPAuth;
        private MailPriority _Priority;

        /// <summary>
        /// The email address to send the message to
        /// </summary>
        internal List<string> To
        {
            get { return this._To; }
        }

        /// <summary>
        /// Email address to send the message from
        /// </summary>
        internal string From
        {
            get { return this._From; }
            set { this._From = value; }
        }

        /// <summary>
        /// SMTP server to use for relaying the email message
        /// </summary>
        internal string SMTPServer
        {
            get { return this._SMTPServer; }
            set { this._SMTPServer = value; }
        }

        /// <summary>
        /// The subject line of the message
        /// </summary>
        internal string Subject
        {
            get { return this._Subject; }
            set { this._Subject = value; }
        }

        /// <summary>
        /// The body of the message
        /// </summary>
        internal string Body
        {
            get { return this._Body; }
            set { this._Body = value; }
        }

        /// <summary>
        /// Gets or sets the carbon-copy address on the email
        /// </summary>
        internal List<string> CC
        {
            get { return this._CC; }
        }

        /// <summary>
        /// Gets or sets the blind carbon-copy addres on the email
        /// </summary>
        internal List<string> Bcc
        {
            get { return this._Bcc; }
            set { this._Bcc = value; }
        }

        /// <summary>
        /// Gets or sets the message to be high priority
        /// </summary>
        internal EmailPriority Priority
        {
            get
            {
                switch (_Priority)
                {
                    case MailPriority.Low:
                        return EmailPriority.Low;
                    case MailPriority.High:
                        return EmailPriority.High;
                    default:
                        return EmailPriority.Normal;
                }
            }
            set
            {
                switch (value)
                {
                    case EmailPriority.Low:
                        this._Priority = MailPriority.Low;
                        break;
                    case EmailPriority.High:
                        this._Priority = MailPriority.High;
                        break;
                    default:
                        this._Priority = MailPriority.Normal;
                        break;
                }
            }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        internal SMTP()
            : this("", "", "", "", "")
        {
        }

        /// <summary>
        /// Constructor which can setup the entire mail object
        /// </summary>
        /// <param name="SMTPServer">SMTP server to use for relaying</param>
        /// <param name="From">Email address the message is from</param>
        /// <param name="To">Email address the message is to</param>
        /// <param name="Subject">The subject line of the message</param>
        /// <param name="Body">The body of the message</param>
        internal SMTP(string SMTPServer, string From, string To, string Subject, string Body)
            : this(SMTPServer, From, To, Subject, Body, "")
        { }

        internal SMTP(string SMTPServer, string From, string To, string Subject, string Body, string CC)
            : this(SMTPServer, From, To, Subject, Body, CC, "")
        { }

        internal SMTP(string SMTPServer, string From, string To, string Subject, string Body, string CC, string Bcc, int port = 587)
        {
            this._To = new List<string>();
            this._CC = new List<string>();
            this._Bcc = new List<string>();

            this._SMTPServer = SMTPServer;
            this._port = port;
            this._From = From;
            if (!string.IsNullOrEmpty(To))
            {
                foreach (var address in To.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    this._To.Add(address);
                }
            }

            if (CC != "")
            {
                this._CC.Add(CC);
            }

            if (Bcc != "" && Bcc != null)
            {
                foreach (var value in Bcc.Split(';'))
                {
                    this._Bcc.Add(value);
                }
            }
            this._Subject = Subject;
            this._Body = Body;
            this._UsingSMTPAuth = false;

            Attachments = new List<MyEmailAttachment>();

            Priority = EmailPriority.Normal;
        }

        /// <summary>
        /// Sets SMTP connection credentials if required
        /// </summary>
        /// <param name="Username">Username to connect to the SMTP server</param>
        /// <param name="Password">Password to connect to the SMTP server</param>
        internal void SetSMTPCredentials(string Username, string Password)
        {
            SetSMTPCredentials(Username, Password, "");
        }

        /// <summary>
        /// Sets SMTP connection credentials if required
        /// </summary>
        /// <param name="Username">Username to connect to the SMTP server</param>
        /// <param name="Password">Password to connect to the SMTP server</param>
        /// <param name="Domain">Domain to connect to the SMTP server</param>
        internal void SetSMTPCredentials(string Username, string Password, string Domain)
        {
            if (Username != "" && Username != null)
            {
                this._SMTPUser = Username;
                this._SMTPPassword = Password;
                if (Domain != "" && Domain != null)
                    this._SMTPDomain = Domain;
                else
                    this._SMTPDomain = "";
                this._UsingSMTPAuth = true;
            }
            else
                this._UsingSMTPAuth = false;
        }

        /// <summary>
        /// Adds an attachment by a filename
        /// </summary>
        /// <param name="FileName">The full path of the file to add</param>
        /// <returns>True on a successful add, false on failure</returns>
        internal bool AddAttachment(string FileName)
        {
            if (!System.IO. File.Exists(FileName))
                throw new MySMTPEmailException("Attachment file does not exist");

            try
            {
                Attachment data = new Attachment(FileName, MediaTypeNames.Application.Octet);
                ContentDisposition disposition = data.ContentDisposition;
                disposition.CreationDate = System.IO.File.GetCreationTime(FileName);
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(FileName);
                disposition.ReadDate = System.IO.File.GetLastAccessTime(FileName);

                MyEmailAttachment Attachment = new MyEmailAttachment();
                Attachment.AttachmentData = data;
                Attachment.Disposition = disposition;

                Attachments.Add(Attachment);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds an attachment by a filename
        /// </summary>
        /// <param name="FileName">The full path of the file to add</param>
        /// <returns>True on a successful add, false on failure</returns>
        internal bool AddExcelAttachment(Stream fileStream, string fileName)
        {
            if (fileStream != null)
                throw new MySMTPEmailException("Attachment Stream does not exist");

            try
            {
                //Attachment data = new Attachment(fileStream, MediaTypeNames.Application.;

                var attachment = new Attachment(fileStream, fileName);
                attachment.ContentType = new ContentType("application/vnd.ms-excel");

                ContentDisposition disposition = attachment.ContentDisposition;
                MyEmailAttachment Attachment = new MyEmailAttachment();
                Attachment.AttachmentData = attachment;
                Attachment.Disposition = disposition;

                Attachments.Add(Attachment);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends the message to its destination
        /// </summary>
        internal async Task SendAsync(bool isBodyHtml = true)
        {
            // Check the email object settings and make sure we're all set up before we even think about sending
            if (To.Count <= 0 && CC.Count <= 0 && Bcc.Count <= 0)
                throw new MySMTPEmailException("TO,CC and Bcc Email address is not set");
            if (From == "")
                throw new MySMTPEmailException("FROM Email address is not set");
            if (Subject == "")
                throw new MySMTPEmailException("Subject is not set");
            if (Body == "")
                throw new MySMTPEmailException("Body is not set");
            if (SMTPServer == "")
                throw new MySMTPEmailException("SMTP server is not set");

            MailMessage mail = new MailMessage();
            mail.Body = Body;
            mail.Subject = Subject;
            mail.From = new MailAddress(From);
            mail.Priority = _Priority;

            if (To.Count > 0)
            {
                foreach (string address in To)
                    mail.To.Add(address);
            }

            if (CC.Count > 0)
            {
                foreach (string address in CC)
                    mail.CC.Add(address);
            }

            if (Bcc.Count > 0)
            {
                foreach (string address in Bcc)
                    mail.Bcc.Add(address);
            }

            SmtpClient client = new SmtpClient(SMTPServer);

            // If using SMTP authentication, create the credential
            if (_UsingSMTPAuth)
            {
                if (_SMTPDomain != "" && _SMTPDomain != null)
                    client.Credentials = new NetworkCredential(_SMTPUser, _SMTPPassword, _SMTPDomain);
                else
                    client.Credentials = new NetworkCredential(_SMTPUser, _SMTPPassword);
            }

            // Add attachments
            foreach (MyEmailAttachment Attachment in Attachments)
            {
                mail.Attachments.Add(Attachment.AttachmentData);
            }

            mail.IsBodyHtml = isBodyHtml;

            client.SendCompleted += (s, e) =>
            {
                client.Dispose();
                mail.Dispose();
            };

            client.Port = this._port; //added by oday, to connect to gmail properly
            client.EnableSsl = EmailService.EmailConfiguration.EnableSsl;
            await client.SendMailAsync(mail);
        }


        /// <summary>
        /// Disposes the Email.SMTP object and clears up any memory used
        /// </summary>
        void IDisposable.Dispose()
        {
            Attachments.Clear();
            To.Clear();
            CC.Clear();
            Bcc.Clear();
        }
    }

    /// <summary>
    /// Custom exception for any problems with the SMTP class
    /// </summary>
    internal class MySMTPEmailException : Exception
    {
        internal MySMTPEmailException()
            : base()
        { }
        internal MySMTPEmailException(string Message)
            : base(Message)
        { }
        internal MySMTPEmailException(string Message, Exception Cause)
            : base(Message, Cause)
        { }
    }
}
