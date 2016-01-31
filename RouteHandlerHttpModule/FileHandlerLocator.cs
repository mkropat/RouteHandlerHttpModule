using System.IO;
using System.Web;

namespace RouteHandlerHttpModule
{
    internal static class FileHandlerLocator
    {
        public static string Locate(HttpContext context)
        {
            var requestPath = context.Request.Url.AbsolutePath;
            var filePath = context.Request.MapPath(requestPath);

            if (!File.Exists(filePath))
                throw new HandlerNotFound("Not a file");

            return filePath;
        }
    }
}
