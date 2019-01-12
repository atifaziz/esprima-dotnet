using System;

namespace Esprima
{
    static class JitHelper
    {
        public static T Throw<T>(Exception exception)
        {
            throw exception;
#pragma warning disable 162 // unreachable code
            return default;
#pragma warning restore 162
        }
        
        public static T Throw<T>() where T : Exception, new()
        {
            throw new T();
#pragma warning disable 162 // unreachable code
            return default;
#pragma warning restore 162
        }

    }
}