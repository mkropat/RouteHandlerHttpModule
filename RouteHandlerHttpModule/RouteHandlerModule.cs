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
            try
            {
                var headers = _context.Response.Headers;
                headers.Add("X-Route-Handler", GetHandler(_context.Context));
            }
            catch (HandlerNotFound)
            {
                // oh well
            }
        }

        static string GetHandler(HttpContext context)
        {
            try
            {
                return TryLocateMvcHandler(context);
            }
            catch (HandlerNotFound)
            {
                return FileHandlerLocator.Locate(context);
            }
        }

        static string TryLocateMvcHandler(HttpContext context)
        {
            try
            {
                return MvcHandlerLocator.Locate(context);
            }
            catch (FileNotFoundException ex) // Assembly load error
            {
                throw new HandlerNotFound(ex);
            }
        }

        public void Dispose()
        {
        }
    }
}
