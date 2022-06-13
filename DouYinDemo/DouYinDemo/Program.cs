using CefSharp;
using CefSharp.OffScreen;
using DouYinDemo.Model;
using HttpRequestCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DouYinDemo
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var urlPattern = @"[a-zA-z]+://[^\s]*";
            CefSharpHelper.InitializeCefSharp();
            var url = "7- 长按复制此条消息，打开抖音搜索，查看TA的更多作品。 https://v.douyin.com/Y6BuYKo/";
            var reg = Regex.Match(url, urlPattern);
            if (reg.Success)
            {
                var result = VideoFactory.GetVideo(reg.Value).CrawlerUrl().Result;
                VideoFactory.DownVideo(result);
                Console.WriteLine("获取结束!");
            }
            else
            {
                Console.WriteLine("链接有误!");
            }
            Console.ReadLine();
        }
    }
}
