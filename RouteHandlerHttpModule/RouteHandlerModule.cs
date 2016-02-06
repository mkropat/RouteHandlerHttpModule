using System;
using System.Web;

namespace RouteHandlerHttpModule
{
    public class RouteHandlerModule : IHttpModule
    {
        readonly MvcHandlerLocator _mvcHandlerLocator = new MvcHandlerLocator();
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
                headers.Add("X-Route-Handler", handler);
            }
        }

        string GetHandler(HttpContext context)
        {
            return _mvcHandlerLocator.Locate(context) ??
                FileHandlerLocator.Locate(context);
        }

        public void Dispose()
        {
        }
    }
}
