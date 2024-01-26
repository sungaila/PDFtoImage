#if !NET6_0_OR_GREATER
using System;

namespace Mono.Util
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class MonoPInvokeCallbackAttribute : Attribute { }
}
#endif