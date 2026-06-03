#if !NET6_0_OR_GREATER || BROWSER
using System;

namespace AOT
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type type) { }
        public MonoPInvokeCallbackAttribute() { }
    }
}
#endif