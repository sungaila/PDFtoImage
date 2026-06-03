#if !NET6_0_OR_GREATER
#pragma warning disable IDE0130
#pragma warning disable CS9113
using System;

namespace AOT
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class MonoPInvokeCallbackAttribute(Type type) : Attribute { }
}
#endif