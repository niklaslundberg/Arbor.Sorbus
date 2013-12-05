using System;

namespace Arbor.Sorbus.Core
{
    public static class StringExtensions
    {
        public static string NewLine(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            if (text.IndexOf("\r\n", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return "\r\n";
            }

            if (text.IndexOf("\n", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return "\n";
            }

            return null;
        }
    }
}