using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HttpRequestCore
{
    /// <summary>
    /// http 请求类
    /// </summary>
    public static class HttpCore
    {
        static HttpCore()
        {
            //设置最大连接数
            ServicePointManager.DefaultConnectionLimit = 1024;
        }
        /// <summary>
        /// 一个请求执行类
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string Execute(RequestInfo request, Action<HttpWebResponse> action = null)
        {
            var temp = string.Empty;
            HttpWebResponse response = null;
            //request 是封装了 一些相关文件的请求
            HttpDefaultConfig config = request.Config;
            //然后这个方法创建了 httpWebRequest这个请求
            HttpWebRequest httpWebRequest = CreateHttpWebRequest(request);
            PopulateRequestHeaders(request, httpWebRequest);
            try
            {
                if (request.Content != null && request.Content.Length > 0)
                {
                    httpWebRequest.AllowWriteStreamBuffering = false;
                    using (Stream requestStreamWithTimeout = WebRequestExtension.GetRequestStreamWithTimeout(httpWebRequest, config.TimeoutInMillis))
                    {
                        //缓存字节的长度
                        byte[] array = new byte[config.BufferSize];
                        int num = 0;
                        long contentLengthFromInternalRequest = request.Content.Length;
                        int num2;
                        while ((num2 = request.Content.Read(array, 0, array.Length)) > 0)
                        {
                            if (contentLengthFromInternalRequest > 0L && (long)(num2 + num) >= contentLengthFromInternalRequest)
                            {
                                requestStreamWithTimeout.Write(array, 0, (int)(contentLengthFromInternalRequest - (long)num));
                                break;
                            }
                            requestStreamWithTimeout.Write(array, 0, num2);
                            num += num2;
                        }
                    }
                }
                response = (WebRequestExtension.GetResponseWithTimeout(httpWebRequest, config.TimeoutInMillis) as HttpWebResponse);
                action?.Invoke(response);
                if (request.cookieSession != null) { request.cookieSession.Add(response.Cookies); }
                StreamReader sr = new StreamReader(response.GetResponseStream());
                temp = sr.ReadToEnd().Trim();
                sr.Close();
                return temp;
            }
            catch (Exception e)
            {
                //  throw e;
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                }
            }
            return temp;
        }
        /// <summary>
        /// 获取页面的HTML
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string IWebClient(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Host = "www.66ip.cn";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.75 Safari/537.36";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader sr = new StreamReader(response.GetResponseStream());
                return sr.ReadToEnd();
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 获取重定向后的地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetRedirectLocation(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.75 Safari/537.36";
                request.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.Found)
                {
                    return response.Headers["Location"];
                }
            }
            catch (Exception e)
            {
            }
            return null;
        }
        /// <summary>
        /// 创建HhttpWebRequest请求 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static HttpWebRequest CreateHttpWebRequest(RequestInfo request)
        {
            HttpDefaultConfig config = request.Config;
            string Url = request.Uri;
            if ((request.HttpMethod == HttpMethod.POST || request.HttpMethod == HttpMethod.PUT || request.HttpMethod == HttpMethod.DELETE))
            {
                string postdata = HttpUtils.GetPostParameters(request.Parameters);
                byte[] Bytedata = System.Text.Encoding.UTF8.GetBytes(postdata);
                request.Content = new MemoryStream(Bytedata);
            }
            else
            {
                string QueryString = HttpUtils.GetCanonicalQueryString(request.Parameters, false);
                if (QueryString.Length > 0)
                {
                    Url = Url + "?" + QueryString;
                }
            }
            if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            HttpWebRequest httpWebRequest = WebRequest.Create(Url) as HttpWebRequest;
            httpWebRequest.Timeout = (config.TimeoutInMillis);
            httpWebRequest.ReadWriteTimeout = (config.ReadWriteTimeoutInMillis);
            //设置body长度信息
            if (!request.Headers.ContainsKey("Content-Length") && request.Content != null)
            {
                request.Headers["Content-Length"] = request.Content.Length.ToString();
            }
            httpWebRequest.ServicePoint.UseNagleAlgorithm = config.UseNagleAlgorithm;
            httpWebRequest.ServicePoint.MaxIdleTime = config.MaxIdleTimeInMillis;
            httpWebRequest.ServicePoint.ConnectionLimit = config.ConnectionLimit;
            httpWebRequest.ServicePoint.Expect100Continue = request.Config.Expect100Continue;
            httpWebRequest.Method = request.HttpMethod.ToString();
            httpWebRequest.ContentType = request.ContentType;
            return httpWebRequest;
        }
        /// <summary>
        /// 头部信息处理
        /// </summary>
        /// <param name="request"></param>
        /// <param name="httpWebRequest"></param>
        public static void PopulateRequestHeaders(RequestInfo request, HttpWebRequest httpWebRequest)
        {
            HttpDefaultConfig config = request.Config;
            httpWebRequest.UserAgent = request.Config.UserAgent;
            httpWebRequest.AllowAutoRedirect = config.AllowAutoRedirect;
            httpWebRequest.Method = request.HttpMethod.ToString();
            httpWebRequest.ContentType = request.ContentType;
            if (config.IsStartCookie)
            {
                if (request.cookieSession == null)
                {
                    request.cookieSession = new CookieContainer();
                }
                httpWebRequest.CookieContainer = request.cookieSession;
            }
            if (config.IsStartProxy)
            {
                if (!string.IsNullOrEmpty(config.ProxyHost) && config.ProxyPort > 0)
                {
                    httpWebRequest.Proxy = new WebProxy(config.ProxyHost, config.ProxyPort);
                }
            }
            foreach (KeyValuePair<string, string> keyValuePair in request.Headers)
            {
                string key = keyValuePair.Key;
                if (key.Equals("Content-Length", StringComparison.CurrentCultureIgnoreCase))
                {
                    httpWebRequest.ContentLength = Convert.ToInt64(keyValuePair.Value);
                }
                else if (key.Equals("Content-Type", StringComparison.CurrentCultureIgnoreCase))
                {
                    httpWebRequest.ContentType = keyValuePair.Value;
                }
                else if (key.Equals("Accept", StringComparison.CurrentCultureIgnoreCase))
                {
                    httpWebRequest.Accept = keyValuePair.Value;
                }
                else if (key.Equals("Referer", StringComparison.CurrentCultureIgnoreCase))
                {
                    httpWebRequest.Referer = keyValuePair.Value;
                }
                else if (key.Equals("Cookie", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (httpWebRequest.CookieContainer != null && !string.IsNullOrEmpty(keyValuePair.Value))
                    {
                        httpWebRequest.CookieContainer = RequestInfo.GetCookieContainer(keyValuePair.Value, request.Uri, DateTime.Now.AddDays(1));
                    }
                }
                else if (!key.Equals("Host", StringComparison.CurrentCultureIgnoreCase))
                {
                    httpWebRequest.Headers[key] = keyValuePair.Value;
                }
            }
        }
        /// <summary>
        /// 实现get post方法
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="PostData">post的时候的数据</param>
        /// <returns></returns>
        public static string Request(string url, string PostData = "", CookieContainer cookieSession = null, string ContentType = "application/x-www-form-urlencoded")
        {
            //检测 是否为空
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接
            string temp = string.Empty;
            HttpWebRequest req = null;
            HttpWebResponse response = null;
            Stream reqStream = null;
            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 1024;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    req = (HttpWebRequest)WebRequest.Create(url);
                }
                else
                {
                    req = (HttpWebRequest)WebRequest.Create(url);
                }
                //请求的Session
                if (cookieSession != null)
                {
                    req.CookieContainer = cookieSession;
                }
                else
                {
                    req.CookieContainer = new CookieContainer();
                    cookieSession = req.CookieContainer;
                }
                req.AllowAutoRedirect = false;//自动跳转为否
                req.Credentials = CredentialCache.DefaultCredentials;
                req.KeepAlive = true;
                if (string.IsNullOrEmpty(PostData))//根据这个变量判断出来是按照哪种方式进行请求
                {
                    req.Method = "GET";
                }
                else
                {
                    //如果是POST ，就把长度设置
                    //传输的编码设置 
                    //然后把请求数据输出
                    req.Method = "POST";
                    req.Proxy = null;
                    req.Timeout = 10000;//设置超时时间
                    req.ServicePoint.Expect100Continue = false;
                    req.ContentType = ContentType;
                    req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(PostData);
                    req.ContentLength = data.Length;
                    //往服务器写入数据
                    reqStream = req.GetRequestStream();
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
                //获取服务端返回
                response = (HttpWebResponse)req.GetResponse();
                cookieSession.Add(response.Cookies);
                //获取服务端返回数据 Encoding.GetEncoding("gbk")
                StreamReader sr = new StreamReader(response.GetResponseStream(), getResponseEncoding(response));
                temp = sr.ReadToEnd().Trim();
                sr.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (req != null)
                {
                    req.Abort();
                }
            }
            return temp;
        }
        /// <summary>
        /// 返回编码
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Encoding getResponseEncoding(HttpWebResponse response)
        {
            if (response != null && !string.IsNullOrEmpty(response.ContentType) && response.ContentType.IndexOf("charset", StringComparison.OrdinalIgnoreCase) > -1)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string[] data = response.ContentType.Split(';');
                foreach (var item in data)
                {
                    string[] temp = item.Split('=');
                    if (temp.Length > 1)
                    {
                        dic.Add(temp[0].ToLower().Trim(), temp[1].ToLower().Trim());
                    }
                }
                if (dic.ContainsKey("charset"))
                {
                    return Encoding.GetEncoding(dic["charset"]);
                }
            }
            return Encoding.UTF8;
        }
        /// <summary>
        /// https请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
    }
}
