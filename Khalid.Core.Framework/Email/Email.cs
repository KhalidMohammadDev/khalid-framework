using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{
    /// <summary>
    /// Base class for email functions
    /// </summary>
    internal class Email
    {
        /// <summary>
        /// The priority of the email
        /// </summary>
        internal enum EmailPriority
        {
            Low = 0,
            Normal,
            High,
        };

        /// <summary>
        /// Represents an email attachment object
        /// </summary>
        protected struct MyEmailAttachment
        {
            /// <summary>
            /// The System.Net.Mail.Attachment object to attach
            /// </summary>
            internal Attachment AttachmentData;

            /// <summary>
            /// The System.Net.Mime.ContentDisposition object which describes the attachment
            /// </summary>
            internal ContentDisposition Disposition;
        }

        /// <summary>
        /// Validates an mail address based on its format
        /// </summary>
        /// <param name="EmailAddress">Email address to validate</param>
        /// <returns>True if valid, False if not</returns>
        internal static bool ValidateEmailAddressFormat(string EmailAddress)
        {
            string strRegex = @"^([a-zA-Z0-9_-.]+)@(([[0-9]{1,3}" +
                  @".[0-9]{1,3}.[0-9]{1,3}.)|(([a-zA-Z0-9-]+" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(EmailAddress))
                return true;
            // 0;          
            else
                return false;
        }

        /// <summary>
        /// Validates an email's domain
        /// </summary>
        /// <param name="EmailAddress">The email address to validate</param>
        /// <returns>True on valid (can connect to end mail server), False if not valid</returns>
        internal static bool ValidateEmailDomain(string EmailAddress)
        {
            bool ReturnVal = false;
            string[] Host = EmailAddress.Split('@');
            string HostName = Host[1];

            IPHostEntry IPHost = Dns.GetHostEntry(HostName);
            IPEndPoint EndPoint = new IPEndPoint(IPHost.AddressList[0], 25);
            Socket s = new Socket(EndPoint.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

            try
            {

                s.Connect(EndPoint);
                s.Disconnect(false);
                ReturnVal = true;
            }
            catch (Exception)
            {
                ReturnVal = false;
            }
            finally
            {
                if (s.Connected)
                    s.Disconnect(false);
            }

            return ReturnVal;

        }
    }
}
