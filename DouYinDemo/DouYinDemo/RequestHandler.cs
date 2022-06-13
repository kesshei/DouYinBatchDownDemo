using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CefSharp;
using CefSharp.Handler;

namespace DouYinDemo
{
    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        //protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser,
        //    IFrame frame, IRequest request, IRequestCallback callback)
        //{
        //    var headers = request.Headers;
        //    headers["User-Agent"] = "My User Agent";
        //    request.Headers = headers;

        //    return CefReturnValue.Continue;
        //}
        private readonly Dictionary<ulong, MemoryStreamResponseFilter> responseDictionary =
            new Dictionary<ulong, MemoryStreamResponseFilter>();

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser,
            IFrame frame,
            IRequest request, IResponse response)
        {
            var dataFilter = new MemoryStreamResponseFilter();
            responseDictionary.Add(request.Identifier, dataFilter);
            return dataFilter;
        }

        protected override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            var url = new Uri(request.Url);
            Console.WriteLine(url.ToString());
            if (response.StatusCode == 200)
            {
                var result = responseDictionary[request.Identifier];
                var data = result.Data?.ToArray();
                if (data == null)
                {
                    Trace.WriteLine($"{url.Scheme}  data is null！");
                    return;
                }
                else
                {
                    var postData = Encoding.UTF8.GetString(data);
                }


                var elements = request.PostData.Elements;
                foreach (var item in elements)
                {
                    var postData = Encoding.UTF8.GetString(item.Bytes);
                    //postData为页面提交的post表单数据，可以做查询
                }
            }


            base.OnResourceLoadComplete(browserControl, browser, frame, request, response, status,
                receivedContentLength);
        }
    }

    public class CustomHandler : CefSharp.Handler.RequestHandler
    {
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator,
            ref bool disableDefaultHandling)
        {
            var url = new Uri(request.Url);
            var extension = url.ToString().ToLower();
            //if (request.ResourceType ==
            //    ResourceType.Image || extension.EndsWith(".jpg")
            //                       || extension.EndsWith(".png")
            //                       || extension.EndsWith(".gif")
            //                       || extension.EndsWith(".jpeg")
            //)
            //Trace.WriteLine($"type:{request.ResourceType} url:{request.Url}");
            //if (request.Method.ToUpper() == "POST" && request.PostData != null)
            //{
            //    if (request.PostData.Elements.Count > 0)
            //    {
            //        var PostData = new byte[request.PostData.Elements[0].Bytes.Length];
            //        request.PostData.Elements[0].Bytes.CopyTo(PostData, 0);
            //    }
            //}

            return new CustomResourceRequestHandler();
        }
    }

    public class MemoryStreamResponseFilter : IResponseFilter
    {
        private int contentLength;
        private List<byte> dataAll = new List<byte>();
        private MemoryStream memoryStream = new MemoryStream();

        public byte[] Data => memoryStream?.ToArray();

        bool IResponseFilter.InitFilter()
        {
            //NOTE: We could initialize this earlier, just one possible use of InitFilter
            memoryStream = new MemoryStream();

            return true;
        }

        FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            if (dataIn == null)
            {
                dataInRead = 0;
                dataOutWritten = 0;

                return FilterStatus.Done;
            }

            dataInRead = dataIn.Length;
            dataOutWritten = Math.Min(dataInRead, dataOut.Length);

            //Important we copy dataIn to dataOut
            dataIn.CopyTo(dataOut);

            //Copy data to stream
            dataIn.Position = 0;
            dataIn.CopyTo(memoryStream);


            return FilterStatus.Done;
        }

        void IDisposable.Dispose()
        {
            
        }

        public event Action<byte[]> NotifyData;

        public void SetContentLength(int contentLength)
        {
            this.contentLength = contentLength;
        }
    }
}