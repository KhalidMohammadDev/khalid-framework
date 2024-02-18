using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Khalid.Core.Framework
{
    public class EmailConfiguration
    {
        /// <summary>
        ///  The email address.
        /// </summary>
        public string EmailAddress
        {
            get; set;
        }

        public string Password { get; set; }

        public string SupportEmail { get; set; }

        public string DebuggerEmail { get; set; }


        public string SMTPServer { get; set; }

        public int SMTPPort { get; set; }

        public bool EnableSsl { get; set; }

    }


}
