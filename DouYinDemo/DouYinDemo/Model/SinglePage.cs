using HttpRequestCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouYinDemo.Model
{
    public class SinglePage : Video
    {
        private string ItemId;
        public SinglePage(string itemId) : base(VideoType.Single, string.Empty)
        {
            this.ItemId = itemId;
        }
        public override Task<List<VideoInfo>> CrawlerUrl()
        {
            var list = new List<VideoInfo>();

            var jsonUrl = $"https://www.iesdouyin.com/web/api/v2/aweme/iteminfo/?item_ids={ItemId}";
            RequestInfo requestInfo = new RequestInfo(jsonUrl);
            requestInfo.Config.AllowAutoRedirect = true;
            requestInfo.Config.UserAgent = VideoFactory.ua;
            var result = HttpCore.Execute(requestInfo);
            JObject jObject = JObject.Parse(result);
            if (jObject != null && jObject["status_code"].ToString() == "0")
            {
                try
                {
                    var urlList = (JArray)jObject["item_list"][0]["video"]["play_addr"]["url_list"];
                    if (urlList != null && urlList.Count > 0)
                    {
                        var videoInfo = new VideoInfo();
                        videoInfo.Title = jObject["item_list"][0]["desc"].ToString();
                        videoInfo.Vid = jObject["item_list"][0]["video"]["vid"].ToString();
                        foreach (var tag in (JArray)(jObject["item_list"][0]["text_extra"]))
                        {
                            videoInfo.tags.Add(tag["hashtag_name"].ToString());
                        }

                        foreach (var item in urlList)
                        {
                            videoInfo.Urls.Add(item.ToString().Trim().Replace("playwm", "play"));
                        }
                        list.Add(videoInfo);
                    }
                }
                catch
                { }
            }

            foreach (var item in list)
            {
                foreach (var urlItem in item.Urls)
                {
                    requestInfo = requestInfo.NewCreate(urlItem);
                    HttpCore.Execute(requestInfo, (r) =>
                    {
                        item.Videos.Add(r.Headers["location"]);
                    });
                }
            }

            return Task.FromResult(list);
        }
    }
}
