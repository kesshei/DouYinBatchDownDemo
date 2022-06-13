using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestCore
{
    /// <summary>
    /// 请求扩展
    /// </summary>
    public static class WebRequestExtension
    {
        /// <summary>
        /// 获取请求流，有超时时间
        /// </summary>
        /// <param name="request"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static Stream GetRequestStreamWithTimeout(WebRequest request, int? millisecondsTimeout = null)
        {
            return WebRequestExtension.AsyncToSyncWithTimeout<Stream>(new Func<AsyncCallback, object, IAsyncResult>(request.BeginGetRequestStream), new Func<IAsyncResult, Stream>(request.EndGetRequestStream), millisecondsTimeout ?? request.Timeout);
        }
        /// <summary>
        /// 获取响应流，有超时时间
        /// </summary>
        /// <param name="request"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static WebResponse GetResponseWithTimeout(HttpWebRequest request, int? millisecondsTimeout = null)
        {
            return WebRequestExtension.AsyncToSyncWithTimeout<WebResponse>(new Func<AsyncCallback, object, IAsyncResult>(request.BeginGetResponse), new Func<IAsyncResult, WebResponse>(request.EndGetResponse), millisecondsTimeout ?? request.Timeout);
        }
        /// <summary>
        /// 一个异步实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        private static T AsyncToSyncWithTimeout<T>(Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end, int millisecondsTimeout)
        {
            IAsyncResult asyncResult = begin(null, null);
            if (!asyncResult.AsyncWaitHandle.WaitOne(millisecondsTimeout))
            {
                TimeoutException ex = new TimeoutException();
                throw new WebException(ex.Message, ex, WebExceptionStatus.Timeout, null);
            }
            return end(asyncResult);
        }
    }
}
