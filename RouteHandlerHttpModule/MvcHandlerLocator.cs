using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RouteHandlerHttpModule
{
    public static class MvcHandlerLocator
    {
        // Implementation based on a post by VitalyB:
        // http://stackoverflow.com/a/19382567/27581

        public static string Locate(HttpContext context)
        {
            try
            {
                return ActualLocate(context);
            }
            catch (FileLoadException) // Assembly load error
            {
                return null;
            }
            catch (FileNotFoundException) // Assembly load error
            {
                return null;
            }
        }

        static string ActualLocate(HttpContext context)
        {
            var httpContext = new HttpContextWrapper(context);

            var routeData = RouteTable.Routes.GetRouteData(httpContext);
            if (routeData == null)
                return null;

            var controller = GetController(httpContext, routeData);
            if (controller == null)
                return null;

            var controllerType = controller.GetType();
            var assembly = controllerType.Assembly;

            var actionMethod = GetActionMethod(httpContext, routeData, controller);

            var signature = actionMethod == null
                ? Convert.ToString(controllerType)
                : $"{controllerType}.{actionMethod.Name}";

            return $"{signature} ({assembly.FullName})";
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
