using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PragathiApi.Application.Common
{
    public static partial class Phones
    {
        /// <summary>
        /// Turns what the user typed into international form (+919876543210),
        /// or returns null if it is not a valid number.
        /// </summary>
        public static string? Normalize(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var s = raw.Trim();
            var digits = new string(s.Where(char.IsDigit).ToArray());
            if (digits.Length == 0) return null;

            string candidate;
            if (s.StartsWith('+')) candidate = "+" + digits;                                 // +91 98765 43210
            else if (digits.StartsWith("00")) candidate = "+" + digits[2..];                 // 0091 98765 43210
            else if (digits.Length == 10) candidate = "+91" + digits;                        // 98765 43210
            else if (digits.Length == 11 && digits[0] == '0') candidate = "+91" + digits[1..]; // 098765 43210
            else if (digits.Length == 12 && digits.StartsWith("91")) candidate = "+" + digits; // 91 98765 43210
            else return null;

            if (!International().IsMatch(candidate)) return null;

            // Indian mobile numbers are 10 digits and start with 6, 7, 8 or 9
            if (candidate.StartsWith("+91") && !IndianMobile().IsMatch(candidate)) return null;

            return candidate;
        }

        [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
        private static partial Regex International();

        [GeneratedRegex(@"^\+91[6-9]\d{9}$")]
        private static partial Regex IndianMobile();
    }

}
