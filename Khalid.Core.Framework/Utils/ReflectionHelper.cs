using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{
    public static class ReflectionHelper
    {
        public static string ConvertToTitleCase(string input)
        {
            // Insert spaces before uppercase letters
            string result = Regex.Replace(input, "(\\B[A-Z])", " $1");
            // Convert to title case
            result = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(result);
            return result;
        }

        public static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type == typeof(string) ||
                   type == typeof(decimal) || type == typeof(DateTime);
        }
        public static List<string> GetReadablePropertyNames(Type type)
        {
            var readableProperties = type.GetProperties()
                .Where(p => p.CanRead && p.GetGetMethod(true).IsPublic && !p.GetGetMethod(true).IsStatic && IsPrimitive(p.PropertyType))
                .Select(p => ConvertToTitleCase(p.Name))
                .ToList();

            return readableProperties;
        }


        public static void SetPropertyIgnoreCase(object obj, string propertyName, object value)
        {
            propertyName = propertyName.Replace(" ", "");
            // Get the PropertyInfo object for the property (case insensitive)
            PropertyInfo property = obj.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property != null && property.CanWrite)
            {
                Type propertyType = property.PropertyType;

                if (value == null)
                {
                    bool allowsNull = Nullable.GetUnderlyingType(propertyType) != null || !propertyType.IsValueType;

                    if (!allowsNull) throw new NullReferenceException(propertyName);
                }
                else
                {
                    // Convert value to the correct type if necessary
                    value = Convert.ChangeType(value, property.PropertyType);
                }
                // Set the value of the property
                property.SetValue(obj, value);
            }
            else
            {
                throw new ArgumentException($"Property {propertyName} not found or not writable.");

            }
        }
        public static object GetPropertyValueIgnoreCase(object obj, string propertyName)
        {
            propertyName = propertyName.Replace(" ", "");
            // Get the PropertyInfo object for the property (case insensitive)
            PropertyInfo property = obj.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property != null)
            {
                // Get the value of the property
                return property.GetValue(obj);
            }
            else
            {
                throw new ArgumentException($"Property '{propertyName}' not found.");
            }
        }
    }
}
