namespace Sentinel.Interfaces
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Runtime.CompilerServices;

    public static class CodeContractExtensions
    {
        [ContractAbbreviator]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull([ValidatedNotNull] this object value, string parameterName)
        {
            Contract.Requires(
                value != null,
                "The value '" + parameterName + "' cannot be null. ");

            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        [ContractAbbreviator]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullOrWhitespace([ValidatedNotNull] this string value, string parameterName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}