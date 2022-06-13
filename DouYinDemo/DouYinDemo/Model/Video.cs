using HttpRequestCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouYinDemo.Model
{
    public abstract class Video
    {
        public Video(VideoType videoType, string url)
        {
            this.VideoType = videoType;
            this.Url = url;
        }
        public VideoType VideoType { get; private set; }
        public string Url { get; private set; }
        public abstract Task<List<VideoInfo>> CrawlerUrl();
    }
    public static class VideoFactory
    {
        public static string ua = @"Mozilla/5.0 (Linux; U; Android 8.1.0; en-US; Nexus 6P Build/OPM7.181205.001) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/57.0.2987.108 UCBrowser/12.11.1.1197 Mobile Safari/537.36";
        public static Video GetVideo(string url)
        {
            Video video = null;
            VideoType videoType = VideoType.Single;
            var itemID = string.Empty;
            if (SwitchVideo(url, ref videoType, ref itemID))
            {
                switch (videoType)
                {
                    case VideoType.Single:
                        {
                            video = new SinglePage(itemID);
                        }
                        break;
                    case VideoType.Home:
                        {
                            video = new HomePage(url);
                        }
                        break;
                }
            }
            return video;
        }
        public static bool SwitchVideo(string url, ref VideoType videoType, ref string itemID)
        {
            var videoKind = VideoType.Single;
            var id = string.Empty;
            try
            {
                RequestInfo requestInfo = new RequestInfo(url);
                requestInfo.Config.AllowAutoRedirect = true;
                requestInfo.Config.UserAgent = ua;
                HttpCore.Execute(requestInfo, (httpResponse) =>
                {
                    var url = httpResponse?.ResponseUri.ToString();
                    if (url.StartsWith("https://www.iesdouyin.com/share/user/"))
                    {
                        videoKind = VideoType.Home;
                    }
                    else
                    {
                        var urlSplit = httpResponse?.ResponseUri.AbsolutePath.ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        id = urlSplit[urlSplit.Length - 1];
                    }
                });
                return true;
            }
            catch (Exception)
            {

            }
            finally
            {
                videoType = videoKind;
                itemID = id;
            }
            return false;
        }
        public static void DownVideo(List<VideoInfo> result, string rootFloder = "")
        {
            if (string.IsNullOrEmpty(rootFloder))
            {
                rootFloder = Path.Combine(AppContext.BaseDirectory, "Video");
            }
            if (!Directory.Exists(rootFloder))
            {
                Directory.CreateDirectory(rootFloder);
            }
            foreach (var item in result)
            {
                if (string.IsNullOrEmpty(item.Vid))
                {
                    //不是视频，直接下一个
                    continue;
                }
                var path = Path.Combine(rootFloder, item.Vid);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                item.Videos.Reverse();
                foreach (var video in item.Videos)
                {
                    //获取真实的下载地址
                    var url = HttpCore.GetRedirectLocation(video);
                    var realPath = Path.Combine(path, item.Vid + ".mp4");
                    if (!File.Exists(realPath))
                    {
                        Console.WriteLine($"正在下载:{item.Title}");
                        if (WebHelper.DownloadFile(url, Path.Combine(path, item.Vid + ".mp4")))
                        {
                            File.WriteAllText(Path.Combine(path, item.Vid + ".json"), JsonConvert.SerializeObject(item));
                            break;
                        }
                        Console.WriteLine($"{item.Title} 处理完毕!");
                    }
                }
            }
        }
    }
    public enum VideoType
    {
        Single,
        Home
    }
}
