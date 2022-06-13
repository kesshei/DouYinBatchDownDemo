using CefSharp;
using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouYinDemo
{
    public delegate bool FilterRequest(IRequest request);
    public delegate void GetResponse(IRequest request, byte[] data = null);
    public class CustomHandler : RequestHandler
    {
        public event FilterRequest FilterRequest;
        public event GetResponse GetResponse;
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator,
            ref bool disableDefaultHandling)
        {
            if (FilterRequest?.Invoke(request) == true)
            {
                var handel = new CustomResourceRequestHandler();
                handel.FilterRequest += FilterRequest;
                handel.GetResponse += GetResponse;
                return handel;
            }
            return base.GetResourceRequestHandler(chromiumWebBrowser, browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
        }
    }

    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        public event FilterRequest FilterRequest;
        public event GetResponse GetResponse;
        private readonly Dictionary<ulong, StreamResponseFilter> responseDictionary =
            new Dictionary<ulong, StreamResponseFilter>();

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser,
            IFrame frame,
            IRequest request, IResponse response)
        {
            if (FilterRequest?.Invoke(request) == true)
            {
                var dataFilter = new StreamResponseFilter();
                responseDictionary.Add(request.Identifier, dataFilter);
                return dataFilter;
            }
            return null;
        }

        protected override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            var url = new Uri(request.Url);
            if (response.StatusCode == 200)
            {
                if (responseDictionary.ContainsKey(request.Identifier))
                {
                    var fliter = responseDictionary[request.Identifier];
                    GetResponse?.Invoke(request, fliter.GetResponse());
                    fliter.Dispose();
                    responseDictionary.Remove(request.Identifier);
                }
            }
        }
    }

    public class StreamResponseFilter : IResponseFilter
    {
        private MemoryStream responseStream;
        public StreamResponseFilter()
        {
            responseStream = new MemoryStream();
        }

        bool IResponseFilter.InitFilter()
        {
            return responseStream != null && responseStream.CanWrite;
        }
        public byte[] GetResponse()
        {
            return responseStream?.ToArray();
        }
        FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            if (dataIn == null)
            {
                dataInRead = 0;
                dataOutWritten = 0;

                return FilterStatus.Done;
            }

            dataInRead = Math.Min(dataIn.Length, dataOut.Length);
            dataOutWritten = dataInRead;

            var readBytes = new byte[dataInRead];
            dataIn.Read(readBytes, 0, readBytes.Length);
            dataOut.Write(readBytes, 0, readBytes.Length);

            responseStream.Write(readBytes, 0, readBytes.Length);

            if (dataInRead < dataIn.Length)
            {
                return FilterStatus.NeedMoreData;
            }

            return FilterStatus.Done;
        }

        public void Dispose()
        {
            responseStream = null;
        }

        void IDisposable.Dispose()
        {

        }
    }
}
