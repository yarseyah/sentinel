using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sentinel.Support
{
    public static class RegexExtensions
    {
        public static bool IsRegexMatch(this string source, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) throw new ArgumentNullException("pattern");
            return Regex.IsMatch(source, pattern);
        }
    }
}
