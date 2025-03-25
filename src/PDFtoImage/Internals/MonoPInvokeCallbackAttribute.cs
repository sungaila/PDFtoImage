#if !NET6_0_OR_GREATER
using System;

#pragma warning disable IDE0130
namespace Mono.Util
#pragma warning restore IDE0130
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class MonoPInvokeCallbackAttribute : Attribute { }
}
#endif