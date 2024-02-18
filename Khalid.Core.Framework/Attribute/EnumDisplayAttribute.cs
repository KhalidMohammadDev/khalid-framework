using System;
using System.Collections.Generic;
using System.Text;

namespace Khalid.Core.Framework
{
    public class EnumDisplayAttribute : Attribute
    {
        public string Text { get; set; }

        public EnumDisplayAttribute(string text)
        {
            this.Text = text;
        }
    }
}
