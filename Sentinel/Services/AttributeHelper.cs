namespace Sentinel.Services
{
    using System;
    using System.Linq;

    public static class AttributeHelper
    {
        public static bool HasAttribute<T>(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var attributes = type.GetCustomAttributes(typeof(T), true);

            return attributes.Any();
        }

        public static bool HasAttribute<T>(this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return obj.GetType().HasAttribute<T>();
        }
    }
}