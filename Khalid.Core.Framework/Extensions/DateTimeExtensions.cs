using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Khalid.Core.Framework
{
    public static class DateTimeExtensions
    {
        public static string ToCustomDateFormat(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
        public static string ToCustomDateFormat(this DateTime? date)
        {
            return date?.ToCustomDateFormat();
        }
        public static string ToCustomDateAndTimeFormat(this DateTime date)
        {
            return date.ToString("tt hh:mm  yyyy-MM-dd");
        }
        public static string ToCustomDateAndTimeFormat(this DateTime? date)
        {
            return date?.ToCustomDateAndTimeFormat();
        }
        public static int CalculateAge(this DateTime dateOfBirth)
        {
            int age = 0;
            age = DateTime.Now.Year - dateOfBirth.Year;
            return age;
        }
        public static int CalculateDiffMonth(this DateTime date1, DateTime date2)
        {
            return ((date1.Year - date2.Year) * 12) + date1.Month - date2.Month;
        }
        public static string ConvertToGregorian(this string dateString)
        {
            var currentThreadCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ar-SA");
            var enCul = new CultureInfo("en-US");
            try
            {
                DateTime tempDate = DateTime.Parse(dateString);

                //  var result = tempDate.ToString("dd/MM/yyyy", enCul.DateTimeFormat);
                System.Threading.Thread.CurrentThread.CurrentCulture = currentThreadCulture;
                return tempDate.ToString("yyyy-MM-dd", enCul.DateTimeFormat); ;

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static string ConvertDateToHijri(this string input)
        {
            try
            {
                var arSA = CultureInfo.CreateSpecificCulture("ar-SA");
                var date = input.ParseCustomDate();
                return date.ToString("yyyy-MM-dd", arSA);
            }
            catch
            {
                return input;
            }
        }

        public static DateTime? ParseHijriDate(this string dateString)
        {
            return ParseHijriDate(dateString, "dd-MM-yyyy");
            //dd - mm - yyyy
        }

        public static DateTime? ParseHijriDate(this string dateString, string format)
        {
            DateTime dateValue;
            CultureInfo arSACulture = new CultureInfo("ar-SA");

            if (DateTime.TryParseExact(dateString, format, arSACulture, DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }

            return null;
        }

        public static string ToHijriString(this DateTime date)
        {
            return ToHijriString(date, "dd-MM-yyyy");
        }

        public static string ToHijriString(this DateTime date, string format)
        {
            return date.ToString(format, new CultureInfo("ar-SA"));
        }

        public static DateTime ParseCustomDate(this string dateString)
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(dateString,
                new string[] { "dd-MM-yyyy", "yyyy-MM-dd" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateTime))
                return dateTime;

            throw new FormatException(dateString);


        }

    }
}
