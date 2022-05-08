
using CefSharp;
using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace AdapterRequestHandler
{
    public class RequestResourceEventArgs : EventArgs
    {

        public string RequestUrl { get; private set; }

        public RequestResourceEventArgs(string requestUrl)
        {
            RequestUrl = requestUrl;
        }
    }

    public delegate void RequestResourceHandler( RequestResourceEventArgs e);

    public class RequestResource
    {
        public static event RequestResourceHandler GetUrl;

        public static void OnRequestEvent(string url)
        {
            if (RequestResource.GetUrl != null)
            {
                RequestResource.GetUrl?.Invoke( new RequestResourceEventArgs(url));
            }
        }

    }

    public class RequestEventHandler : RequestHandler
    {



        //public static event EventHandler<RequestResourceEventArgs> RequestResource;



        public static List<string> _BlockList = new List<string>();

        public static void SetBlocklist(List<string> BlockList)
        {
            _BlockList = BlockList;

        }



        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            //HelperClasses.Logger.Log("In GetResourceRequestHandler : " + request.Url);
        

            RequestResource.OnRequestEvent(request.Url);



            //Only intercept specific Url

            // Uri url = new Uri(request.Url);

            //if (_BlockList.Contains(url.Host))//Regex.Match(request.Url, pattern).Success
            //{
            //    return new ResourceEventHandler();

            //}
            ///Default behaviour, url will be loaded normally.
            return null;
        }

    }
}

class ResourceEventHandler : ResourceRequestHandler
{
    /*protected override bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
    {
        HelperClasses.Logger.Log(request.Url);
        return false;
    }*/

    private static HttpClient httpClient = new HttpClient();

    protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
    {

        // Download the JS
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri(request.Url),
            Method = HttpMethod.Get,
        };
        var res = httpClient.SendAsync(req).Result.Content.ReadAsStringAsync().Result;
        var modRes = Regex.Replace(res, @"(t.isDlcTitleInfoSupported=function\(e\)\{)", "$1return false;");

        // since we cant mod the response...
        frame.ExecuteJavaScriptAsync(modRes, request.Url, 0);
        // this intentionally errors. (CORS issue)
        return ResourceHandler.FromString("/*edited*/", mimeType: Cef.GetMimeType("js"));
    }
}
