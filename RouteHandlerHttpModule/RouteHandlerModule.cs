using System;
using System.IO;
using System.Web;

namespace RouteHandlerHttpModule
{
    public class RouteHandlerModule : IHttpModule
    {
        HttpApplication _context;

        public void Init(HttpApplication context)
        {
            _context = context;

            context.BeginRequest += OnRequest;
        }

        void OnRequest(object sender, EventArgs e)
        {
            var handler = GetHandler(_context.Context);
            if (handler != null)
            {
                var headers = _context.Response.Headers;
                headers.Add("X-Route-Handler", GetHandler(_context.Context));
            }
        }

        static string GetHandler(HttpContext context)
        {
            return TryLocateMvcHandler(context) ??
                FileHandlerLocator.Locate(context);
        }

        static string TryLocateMvcHandler(HttpContext context)
        {
            try
            {
                return MvcHandlerLocator.Locate(context);
            }
            catch (FileNotFoundException) // Assembly load error
            {
                return null;
            }
        }

        public void Dispose()
        {
        }
    }
}
