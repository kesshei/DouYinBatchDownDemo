using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChromiumWebBrowser = CefSharp.OffScreen.ChromiumWebBrowser;
using CefSharp;
using CefSharp.OffScreen;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using DouYinDemo.Model;

namespace DouYinDemo
{
    public static class CefSharpHelper
    {
        public static void InitializeCefSharp()
        {
            var settings = new CefSettings();
            settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CefSharp.BrowserSubprocess.exe");
            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            settings.UserAgent = VideoFactory.ua;
            settings.RemoteDebuggingPort = 8088;
            settings.RootCachePath = Path.GetFullPath("cache");
            settings.CachePath = Path.GetFullPath("cache\\global");
            settings.UncaughtExceptionStackSize = 10;
            settings.WindowlessRenderingEnabled = false;

            Cef.Initialize(settings, false, browserProcessHandler: null);
        }

        public static Task LoadPageAsync(IWebBrowser browser, string address = null)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;

            if (!string.IsNullOrEmpty(address))
            {
                browser.Load(address);
            }
            return tcs.Task;
        }
        private static void DisplayBitmap(Task<Bitmap> task)
        {
            var screenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CefSharp screenshot" + DateTime.Now.Ticks + ".png");
            Console.WriteLine("图片已经保存到 {0}", screenshotPath);

            var bitmap = task.Result;
            bitmap.Save(screenshotPath);
            bitmap.Dispose();

            Console.WriteLine("屏幕截图已经完毕，等待打开...");

            Process.Start(new ProcessStartInfo(screenshotPath)
            {
                UseShellExecute = true
            });

            Console.WriteLine("屏幕截图已经打开!");
        }
        public static string ChangeUrlParameters(string url, Dictionary<string, string> parameters)
        {
            var urlSplit = url.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            var RootUrl = urlSplit[0];
            var RootParameter = new Dictionary<string, string>();
            if (urlSplit.Count() > 0)
            {
                var parameter = urlSplit[1].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var keyValue in parameter)
                {
                    var kv = keyValue.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (kv.Length == 2)
                    {
                        RootParameter[kv[0]] = kv[1];
                    }
                    else
                    {
                        RootParameter[kv[0]] = "";
                    }
                }
            }

            foreach (var item in parameters)
            {
                RootParameter[item.Key] = item.Value;
            }

            List<string> list = new List<string>();
            foreach (var keyValuePair in RootParameter)
            {
                string key = keyValuePair.Key;
                string value = keyValuePair.Value;
                if (value == null)
                {
                    value = string.Empty;
                }
                list.Add(key + '=' + value);
            }
            var urlParameter = string.Join("&", list.ToArray());
            if (!string.IsNullOrEmpty(urlParameter))
            {
                urlParameter = "?" + urlParameter;
            }
            return RootUrl + urlParameter;
        }
    }
}
