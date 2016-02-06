using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace RouteHandlerHttpModule
{
    internal static class DynamicControllerLocator
    {
        static Assembly _mvc;
        static bool _initialized = false;

        public static ControllerLocatorResult Locate(HttpContextBase context, RouteData routeData)
        {
            Init();

            if (_mvc == null)
                return null;

            var controller = GetController(context, routeData);
            if (controller == null)
                return null;

            return new ControllerLocatorResult
            {
                ControllerType = (Type)controller.GetType(),
                ActionMethod = GetActionMethod(context, routeData, controller),
            };
        }

        static void Init()
        {
            if (_initialized)
                return;

            try
            {
                _mvc = Assembly.Load("System.Web.Mvc");
            }
            catch (FileLoadException) { }
            catch (FileNotFoundException) { }

            _initialized = true;
        }

        static dynamic GetController(HttpContextBase context, RouteData routeData)
        {
            var name = routeData.Values["controller"] as string;
            if (string.IsNullOrEmpty(name))
                return null;

            dynamic controllerBuilder = GetStaticProperty(_mvc.GetType("System.Web.Mvc.ControllerBuilder"), "Current");
            var controllerFactory = controllerBuilder.GetControllerFactory();

            var requestContext = new RequestContext(context, routeData);

            return controllerFactory.CreateController(requestContext, name);
        }

        static MethodInfo GetActionMethod(HttpContextBase context, RouteData routeData, dynamic controller)
        {
            var actionName = routeData.Values["action"] as string;
            if (actionName == null)
                return null;

            dynamic controllerContext = Activator.CreateInstance(_mvc.GetType("System.Web.Mvc.ControllerContext"),
                context, routeData, controller);

            dynamic controllerDescriptor = Activator.CreateInstance(_mvc.GetType("System.Web.Mvc.ReflectedControllerDescriptor"),
                controller.GetType());
            var actionDescriptor = controllerDescriptor.FindAction(controllerContext, actionName);

            return actionDescriptor?.MethodInfo;
        }

        static object GetStaticProperty(Type t, string propertyName)
        {
            return t.GetProperty(propertyName).GetValue(null);
        }
    }
}
