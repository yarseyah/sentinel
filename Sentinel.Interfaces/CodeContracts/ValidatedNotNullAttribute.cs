namespace Sentinel.Interfaces.CodeContracts
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}