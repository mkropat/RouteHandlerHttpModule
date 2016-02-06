using System.Web;
using System.Web.Routing;

namespace RouteHandlerHttpModule
{
    public class MvcHandlerLocator
    {
        readonly StaticControllerLocator _staticControllerLocator = new StaticControllerLocator();

        public string Locate(HttpContext context)
        {
            var httpContext = new HttpContextWrapper(context);

            var routeData = RouteTable.Routes.GetRouteData(httpContext);
            if (routeData == null)
                return null;

            var result = _staticControllerLocator.IsSupported
                ? _staticControllerLocator.Locate(httpContext, routeData)
                : DynamicControllerLocator.Locate(httpContext, routeData);

            if (result?.ControllerType == null)
                return null;

            var signature = result.ActionMethod == null
                ? $"{result.ControllerType}"
                : $"{result.ControllerType}.{result.ActionMethod.Name}";
            var assembly = result.ControllerType.Assembly;

            return $"{signature} ({assembly.FullName})";
        }
    }
}
