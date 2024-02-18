using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Khalid.Core.Framework
{
    public static class EnumExtensions
    {
        public static string ToEnumDisplay(this Enum enumValue)
        {
            string text = string.Empty;

            Type type = enumValue.GetType();

            if (!type.IsEnum)
            {
                throw new ArgumentException("enumValue must be of Enum type", "enumValue");
            }

            FieldInfo fieldInfo = type.GetField(enumValue.ToString());

            if (fieldInfo == null || fieldInfo.GetCustomAttribute(typeof(EnumDisplayAttribute)) == null)
            {
                throw new ArgumentException("enumValue must contains the EnumDisplayAttribute", "enumValue");
            }

            EnumDisplayAttribute attribute = (EnumDisplayAttribute)fieldInfo.GetCustomAttribute(typeof(EnumDisplayAttribute));

            if (!String.IsNullOrEmpty(attribute.Text))
            {
                text = attribute.Text;
            }

            return text;
        }

        
        public static IEnumerable<T> GetEnumValues<T>(bool excludeDefualt = false)
          where T : struct, IConvertible
        {
            // Can't use type constraints on value types, so have to do check like this
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            var enumValues = Enum.GetValues(typeof(T)).Cast<T>();

            return excludeDefualt ? enumValues.Where(e => Convert.ToInt32(e) != 0) : enumValues;
        }
    }
}
