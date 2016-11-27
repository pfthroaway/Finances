using System;

namespace Finances
{
    /// <summary>Extension class to more easily parse Decimals.</summary>
    internal static class DecimalHelper
    {
        /// <summary>Utilizes decimal.TryParse to easily Parse a Decimal.</summary>
        /// <param name="text">Text to be parsed</param>
        /// <returns>Parsed Decimal</returns>
        internal static decimal Parse(string text)
        {
            decimal temp = 0;
            decimal.TryParse(text, out temp);
            return temp;
        }

        /// <summary>Utilizes decimal.TryParse to easily Parse a Decimal.</summary>
        /// <param name="obj">Object to be parsed</param>
        /// <returns>Parsed Decimal</returns>
        internal static decimal Parse(object obj)
        {
            decimal temp = 0;
            decimal.TryParse(obj.ToString(), out temp);
            return temp;
        }
    }

    /// <summary>Extension class to more easily parse DateTimes.</summary>
    internal static class DateTimeHelper
    {
        /// <summary>Utilizes DateTime.TryParse to easily Parse a DateTime.</summary>
        /// <param name="text">Text to be parsed</param>
        /// <returns>Parsed DateTime</returns>
        internal static DateTime Parse(string text)
        {
            DateTime temp = new DateTime();
            DateTime.TryParse(text, out temp);
            return temp;
        }

        /// <summary>Utilizes DateTime.TryParse to easily Parse a DateTime.</summary>
        /// <param name="obj">Object to be parsed</param>
        /// <returns>Parsed DateTime</returns>
        internal static DateTime Parse(object obj)
        {
            DateTime temp = new DateTime();
            DateTime.TryParse(obj.ToString(), out temp);
            return temp;
        }
    }

    public static class Extension
    {
        /// <summary>Determines if this character is a period.</summary>
        /// <param name="c">Character to be evaluated</param>
        /// <returns>Returns true if character is a period</returns>
        public static bool IsPeriod(this char c)
        {
            return c.Equals('.');
        }

        /// <summary>Determines if this character is a period or comma.</summary>
        /// <param name="c">Character to be evaluated</param>
        /// <returns>Returns true if character is a period or comma</returns>
        public static bool IsPeriodOrComma(this char c)
        {
            return c.Equals('.') || c.Equals(',');
        }
    }
}