using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RouteHandlerHttpModule
{
    internal class StaticControllerLocator
    {
        bool? _isSupported;

        public bool IsSupported
        {
            get
            {
                if (!_isSupported.HasValue)
                    _isSupported = CanBindToMvc();
                
                return _isSupported.Value;
            }
        }

        public ControllerLocatorResult Locate(HttpContextBase context, RouteData routeData)
        {
            // Implementation based on a post by VitalyB:
            // http://stackoverflow.com/a/19382567/27581

            var controller = GetController(context, routeData);
            if (controller == null)
                return null;

            return new ControllerLocatorResult
            {
                ControllerType = controller.GetType(),
                ActionMethod = GetActionMethod(context, routeData, controller),
            };
        }

        static bool CanBindToMvc()
        {
            try
            {
                TryCallMvc();
                return true;
            }
            catch (FileLoadException) { }       // Assembly load error
            catch (FileNotFoundException) { }   // Assembly load error

            return false;
        }

        static void TryCallMvc()
        {
            ControllerBuilder.Current.GetType();
        }

        static ControllerBase GetController(HttpContextBase context, RouteData routeData)
        {
            var name = routeData.Values["controller"] as string;
            if (string.IsNullOrEmpty(name))
                return null;

            var requestContext = new RequestContext(context, routeData);
            var controllerFactory = ControllerBuilder.Current.GetControllerFactory();

            return (ControllerBase)controllerFactory.CreateController(requestContext, name);
        }

        static MethodInfo GetActionMethod(HttpContextBase context, RouteData routeData, ControllerBase controller)
        {
            var actionName = routeData.Values["action"] as string;
            if (actionName == null)
                return null;

            var controllerContext = new ControllerContext(context, routeData, controller);
            var controllerDescriptor = new ReflectedControllerDescriptor(controller.GetType());
            var actionDescriptor = controllerDescriptor.FindAction(controllerContext, actionName) as ReflectedActionDescriptor;

            return actionDescriptor?.MethodInfo;
        }
    }
}
