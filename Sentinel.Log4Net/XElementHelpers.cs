namespace Sentinel.Log4Net
{
    using System;
    using System.Xml.Linq;

    public static class XElementHelpers
    {
        public static string GetAttribute(this XElement element, string attributeName, string defaultValue)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (!element.HasAttributes)
            {
                return defaultValue;
            }

            var value = element.Attribute(attributeName);
            return value == null ? defaultValue : value.Value;
        }

        public static DateTime GetAttributeDateTime(this XElement element, string attributeName, DateTime defaultValue)
        {
            var value = element.GetAttribute(attributeName, string.Empty);

            var result = defaultValue;
            if (!string.IsNullOrWhiteSpace(value))
            {
                DateTime.TryParse(value, out result);
            }

            return result;
        }
    }
}