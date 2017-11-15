using System.Globalization;

namespace Extensions.DataTypeHelpers
{
    /// <summary>Extension class to more easily parse Decimals.</summary>
    public static class DecimalHelper
    {
        private static NumberStyles _styles = NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint;

        /// <summary>Utilizes decimal.TryParse to easily parse a Decimal.</summary>
        /// <param name="text">Text to be parsed</param>
        /// <returns>Parsed Decimal</returns>
        public static decimal Parse(string text)
        {
            decimal.TryParse(text, _styles, CultureInfo.InvariantCulture, out decimal temp);
            return temp;
        }

        /// <summary>Utilizes decimal.TryParse to easily parse a Decimal.</summary>
        /// <param name="obj">Object to be parsed</param>
        /// <returns>Parsed Decimal</returns>
        public static decimal Parse(object obj)
        {
            decimal.TryParse(obj.ToString(), _styles, CultureInfo.InvariantCulture, out decimal temp);
            return temp;
        }
    }
}