using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HttpRequestCore
{
    /// <summary>
    /// 前置请求
    /// </summary>
    public class RequestInfo
    {
        #region 地址信息
        /// <summary>
        /// 操作的URL 地址
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// 具体操作的方法
        /// </summary>
        public HttpMethod HttpMethod { get; set; }
        /// <summary>
        /// 参数信息
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }
        /// <summary>
        /// 代理地址
        /// </summary>
        public string ProxyUrl { get; set; }
        /// <summary>
        /// 开启代理
        /// </summary>
        public void StartProxy()
        {
            var temp = HttpCore.Request(ProxyUrl);
            if (!string.IsNullOrEmpty(temp) && temp.IndexOf(':') > -1)
            {
                int port = 0;
                var ips = temp.Split(':');
                Config.ProxyHost = ips[0];
                int.TryParse(ips[1], out port);
                Config.ProxyPort = port;
            }
        }
        #endregion
        #region 头部信息
        private IDictionary<string, string> _Headers;
        /// <summary>
        /// 头部信息
        /// </summary>
        public IDictionary<string, string> Headers { get { return _Headers; } }
        #endregion
        #region  Body信息
        /// <summary>
        /// 具体操作的主体内容
        /// </summary>
        public Stream Content { get; set; }
        /// <summary>
        /// 配置信息
        /// </summary>
        public HttpDefaultConfig Config { get; set; }
        /// <summary>
        /// 大数据分块上传能用到
        /// </summary>
        public long[] Range { get; set; }
        /// <summary>
        /// MIME类型
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// cookie容器
        /// </summary>
        public CookieContainer cookieSession { get; set; }
        #endregion
        /// <summary>
        /// 构造函数
        /// </summary>
        public RequestInfo(string Uri, HttpMethod HttpMethod, HttpDefaultConfig Config)
        {
            this.Parameters = new Dictionary<string, string>();
            this._Headers = new Dictionary<string, string>();
            this.Uri = Uri;
            this.HttpMethod = HttpMethod;
            this.Config = Config;
            this.ContentType = this.Config.ContentType;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public RequestInfo(string Uri, string data = "", HttpMethod HttpMethod = HttpMethod.GET, HttpDefaultConfig Config = null)
        {
            this.Parameters = new Dictionary<string, string>();
            this._Headers = new Dictionary<string, string>();
            this.Uri = Uri;
            this.HttpMethod = HttpMethod;
            if (Config == null)
            {
                this.Config = new HttpDefaultConfig();
            }
            else
            {
                this.Config = Config;
            }
            this.ContentType = this.Config.ContentType;
            Content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public RequestInfo(string Uri, Dictionary<string, string> data, HttpMethod HttpMethod = HttpMethod.GET, HttpDefaultConfig Config = null)
        {
            this.Parameters = new Dictionary<string, string>();
            this._Headers = new Dictionary<string, string>();
            this.Uri = Uri;
            this.HttpMethod = HttpMethod;
            if (Config == null)
            {
                this.Config = new HttpDefaultConfig();
            }
            else
            {
                this.Config = Config;
            }
            this.ContentType = this.Config.ContentType;
        }
        /// <summary>
        /// 添加复制过来的头部信息
        /// </summary>
        /// <param name="data"></param>
        public void AddCopyHeaders(string data)
        {
            //复制过来的参数处理
            if (!string.IsNullOrEmpty(data))
            {
                //获取每一行信息
                var lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines != null && lines.Length > 0)
                {
                    foreach (var item in lines)
                    {
                        if (item.IndexOf(": ") > -1)
                        {
                            var values = item.Split(new string[] { ": " }, StringSplitOptions.None);
                            if (_Headers.ContainsKey(values[0]))
                            {
                                _Headers[values[0]] = values[1];
                            }
                            else
                            {
                                _Headers.Add(values[0], values[1]);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddHeader(string key, string value)
        {
            if (_Headers.ContainsKey(key))
            {
                _Headers[key] = value;
            }
            else
            {
                _Headers.Add(key, value);
            }
        }
        /// <summary>
        /// 生成一个新的对象
        /// //但是会用之前的信息
        /// </summary>
        /// <param name="Uri"></param>
        /// <param name="data"></param>
        /// <param name="HttpMethod"></param>
        /// <param name="Config"></param>
        /// <returns></returns>
        public RequestInfo NewCreate(string Uri, string data = "", HttpMethod HttpMethod = HttpMethod.GET, HttpDefaultConfig Config = null)
        {
            RequestInfo requestInfo = new RequestInfo(Uri, data, HttpMethod, Config);
            requestInfo._Headers = this.Headers;
            requestInfo.cookieSession = this.cookieSession;
            return requestInfo;
        }
        /// <summary>
        /// 获取cookie对象
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static CookieContainer GetCookieContainer(string data, string url, DateTime dt)
        {
            var cc = new CookieContainer();
            var domain = new Uri(url).Host;
            domain = domain.Substring(domain.IndexOf('.') + 1);
            if (!string.IsNullOrEmpty(data))
            {
                if (data.IndexOf("Cookie:") > -1)
                {
                    data = data.Replace("Cookie:", "");
                }
                var keys = data.Split(';');
                foreach (var item in keys)
                {
                    var index = -99;
                    if ((index = item.IndexOf('=')) > -1)
                    {
                        var key = item.Substring(0, index).Trim();
                        var value = item.Substring(index + 1).Trim();
                        var cookie = new Cookie(key, HttpUtility.UrlEncode(value), "/", domain); //初使化并设置Cookie的名称
                        cookie.Expires = dt; //设置过期时间  
                        cc.Add(cookie);
                    }
                }
            }
            return cc;
        }
    }
}
