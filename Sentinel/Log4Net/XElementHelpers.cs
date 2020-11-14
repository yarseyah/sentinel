namespace Sentinel.Log4Net
{
    using System;
    using System.Globalization;
    using System.Xml.Linq;

    using Common.Logging;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;

    public static class XElementHelpers
    {
        private static readonly ILog Log = LogManager.GetLogger("XElementHelpers");

        public static string GetAttribute(this XElement element, string attributeName, string defaultValue)
        {
            element.ThrowIfNull(nameof(element));

            if (!element.HasAttributes)
            {
                return defaultValue;
            }

            var value = element.Attribute(attributeName);
            return value?.Value ?? defaultValue;
        }

        public static DateTime GetAttributeDateTime(this XElement element, string attributeName, DateTime defaultValue)
        {
            var value = element.GetAttribute(attributeName, string.Empty);

            var result = defaultValue;
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (!DateTime.TryParse(value, null, DateTimeStyles.AdjustToUniversal, out result))
                {
                    Log.Warn($"Unable to parse DateTime of '{value}' to a valid date");
                }
            }

            return result;
        }
    }
}