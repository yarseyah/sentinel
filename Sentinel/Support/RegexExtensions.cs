namespace Sentinel.Support
{
    using System;
    using System.Text.RegularExpressions;

    public static class RegexExtensions
    {
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">A regular expression parsing error occurred.</exception>
        /// <exception cref="RegexMatchTimeoutException">A time-out occurred. For more information about time-outs, see the Remarks section.</exception>
        public static bool IsRegexMatch(this string source, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentNullException("pattern");
            }

            return Regex.IsMatch(source, pattern);
        }
    }
}
