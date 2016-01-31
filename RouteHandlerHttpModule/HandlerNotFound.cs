using System;

namespace RouteHandlerHttpModule
{
    [Serializable]
    internal class HandlerNotFound : Exception
    {
        public HandlerNotFound(string message) : base(message) { }
        public HandlerNotFound(Exception inner) : base(inner.Message, inner) { }
    }
}
