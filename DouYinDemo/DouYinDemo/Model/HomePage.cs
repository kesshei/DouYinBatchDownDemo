using CefSharp;
using CefSharp.OffScreen;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DouYinDemo.Model
{
    public class HomePage : Video
    {
        public int CrawlerTimeOut = 3;
        public int RefreshInterval = 1;
        public AutoResetEvent autoResetEvent;
        public HomePage(string url) : base(VideoType.Single, url)
        {
            autoResetEvent = new AutoResetEvent(false);
        }

        public override async Task<List<VideoInfo>> CrawlerUrl()
        {
            var List = new List<VideoInfo>();
            var requestContextSettings = new RequestContextSettings
            {
                CachePath = Path.GetFullPath($@"cache\{Guid.NewGuid().ToString("N")}")
            };
            using var requestContext = new RequestContext(requestContextSettings);
            using var browser = new ChromiumWebBrowser(Url, new BrowserSettings() { WindowlessFrameRate = 1 }, requestContext);
            var handler = new CustomHandler();
            handler.FilterRequest += (request) =>
            {
                if (request.Method.StartsWith("GET", StringComparison.InvariantCultureIgnoreCase) && request.Url.Contains("aweme/post"))
                {
                    return true;
                }
                return false;
            };
            handler.GetResponse += (request, data) =>
            {
                var url = request.Url;
                var json = Encoding.UTF8.GetString(data);
                Task.Run(async () =>
                {

                    if (!string.IsNullOrEmpty(json) && json.Contains("has_more"))
                    {
                        var jObject = JObject.Parse(json);
                        if (jObject != null && jObject["status_code"].ToString() == "0")
                        {
                            bool has_more = (bool)jObject["has_more"];
                            var max_cursor = jObject["max_cursor"].ToString();
                            var aweme_list = (JArray)jObject["aweme_list"];
                            foreach (var item in aweme_list)
                            {
                                VideoInfo videoInfo = new VideoInfo();
                                videoInfo.Title = item["desc"].ToString();
                                videoInfo.Vid = item["video"]["vid"].ToString();
                                foreach (var tag in (JArray)(item["text_extra"]))
                                {
                                    videoInfo.tags.Add(tag["hashtag_name"].ToString());
                                }

                                var urlList = (JArray)item["video"]["play_addr"]["url_list"];
                                foreach (var video in urlList)
                                {
                                    videoInfo.Videos.Add(video.ToString());
                                }
                                List.Add(videoInfo);
                            }

                            if (has_more)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(RefreshInterval));
                                var newUrl = CefSharpHelper.ChangeUrlParameters(url, new Dictionary<string, string>() { { "max_cursor", max_cursor } });
                                await CefSharpHelper.LoadPageAsync(browser, newUrl);
                            }
                            else
                            {
                                autoResetEvent.Set();
                            }
                        }
                    }
                });
            };
            browser.RequestHandler = handler;
            browser.Size = new System.Drawing.Size(375, 812);

            await CefSharpHelper.LoadPageAsync(browser);
            autoResetEvent.WaitOne(TimeSpan.FromMinutes(CrawlerTimeOut));
            //SpinWait.SpinUntil(() => false, TimeSpan.FromMinutes(CrawlerTimeOut));
            return List;
        }
    }
}
