namespace Sentinel.Support.Converters
{
    using System;
    using System.Collections.Generic;

    using Sentinel.Interfaces;

    public static class MatchModeConverter
    {
        public static int Convert(MatchMode matchMode, IEnumerable<string> list)
        {
            string search = matchMode == MatchMode.Exact
                                ? "exact"
                                : matchMode == MatchMode.RegularExpression
                                      ? "regularexpression"
                                      : "substring";

            return Math.Max(0, list.IndexOf(search, new CaseInsensitiveComparer<string>()));
        }

        public static MatchMode ConvertFrom(string asString)
        {
            string matchString = asString.Replace(" ", string.Empty).ToLower();

            switch (matchString)
            {
                case "substring":
                    return MatchMode.CaseSensitive;
                case "regularexpression":
                case "regex":
                    return MatchMode.RegularExpression;
                default:
                    return MatchMode.Exact;
            }
        }
    }
}