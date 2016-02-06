using System;
using System.Reflection;

namespace RouteHandlerHttpModule
{
    internal class ControllerLocatorResult
    {
        public Type ControllerType { get; set; }
        public MethodInfo ActionMethod { get; set; }
    }
}
