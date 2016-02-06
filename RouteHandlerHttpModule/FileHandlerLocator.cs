using System.IO;
using System.Web;

namespace RouteHandlerHttpModule
{
    public static class FileHandlerLocator
    {
        public static string Locate(HttpContext context)
        {
            var requestPath = context.Request.Url.AbsolutePath;
            var filePath = context.Request.MapPath(requestPath);

            return File.Exists(filePath)
                ? filePath
                : null;
        }
    }
}
