using System;

namespace RouteHandlerHttpModule.MvcHandlerLocator
{
    [Serializable]
    public class MvcHandlerNotFound : Exception
    {
        public MvcHandlerNotFound(string message) : base(message) { }
    }
}
