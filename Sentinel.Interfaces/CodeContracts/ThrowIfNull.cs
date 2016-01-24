namespace Sentinel.Interfaces.CodeContracts
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Runtime.CompilerServices;

    public static partial class CodeContractExtensions
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
    }
}