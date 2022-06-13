using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpRequestCore
{
    /// <summary>
    /// 请求的默认配置
    /// </summary>
    public class HttpDefaultConfig
    {
        /// <summary>
        /// Http头部类型信息
        /// </summary>
        public string ContentType { get; set; } = HTTPContentType.HTML;
        /// <summary>
        /// 超时时间 毫秒
        /// </summary>
        public int TimeoutInMillis { get; set; } = 50 * 1000;
        /// <summary>
        /// HttpRequest 读写 超时时间
        /// </summary>
        public int ReadWriteTimeoutInMillis { get; set; } = 50 * 1000;
        /// <summary>
        /// 是否采用 NagleAlgorithm算法， 如果立即发送则为flase，否则为True
        /// </summary>
        public bool UseNagleAlgorithm { get; set; } = false;
        /// <summary>
        /// 获取或设置与 System.Net.ServicePoint 对象关联的连接在被关闭前可以持续空闲的时间
        /// </summary>
        public int MaxIdleTimeInMillis { get; set; } = DefaultMaxIdleTimeInMillis;
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int ConnectionLimit { get; set; } = DefaultConnectionLimit;
        /// <summary>
        /// 默认缓存长度
        /// </summary>
        public int BufferSize { get; set; } = 5 * 1024 * 1024;
        /// <summary>
        /// 当POST的时候会先让服务器应答一下，兼容不好，所以建议为flase,不进行应答
        /// </summary>
        public bool Expect100Continue { get; set; } = false;
        /// <summary>
        /// 返回默认的用户代理头信息
        /// </summary>
        public string UserAgent { get; set; } = GenerateDefaultUserAgent();
        /// <summary>
        /// 是否启动cookie
        /// </summary>
        public bool IsStartCookie { get; set; } = true;
        /// <summary>
        /// 是否启动代理
        /// </summary>
        public bool IsStartProxy { get; set; } = false;
        /// <summary>
        /// 代理IP地址
        /// </summary>
        public string ProxyHost { get; set; }
        /// <summary>
        /// 代理端口
        /// </summary>
        public int ProxyPort { get; set; }
        /// <summary>
        /// 自动跳转为否
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = false;

        public HttpDefaultConfig()
        {
        }
        /// <summary>
        /// 获取默认用户头信息
        /// </summary>
        /// <returns></returns>
        private static string GenerateDefaultUserAgent()
        {
            return "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
        }

        /// <summary>
        /// 获取当前系统默认.net 框架的版本
        /// </summary>
        /// <returns></returns>
        private static string GetFrameworkVersion()
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\"))
            {
                if (registryKey == null)
                {
                    return "Unknown(NDP key not found)";
                }
                using (RegistryKey registryKey2 = registryKey.OpenSubKey("v4"))
                {
                    if (Environment.Version.Major >= 4 && registryKey2 != null)
                    {
                        return HttpDefaultConfig.GetFrameworkVersionAfter4(registryKey2);
                    }
                }
                using (RegistryKey registryKey3 = registryKey.OpenSubKey("v3.5"))
                {
                    if (registryKey3 != null)
                    {
                        return "3.5";
                    }
                }
                using (RegistryKey registryKey4 = registryKey.OpenSubKey("v3.0"))
                {
                    if (registryKey4 != null)
                    {
                        return "3.0";
                    }
                }
                using (RegistryKey registryKey5 = registryKey.OpenSubKey("v2.0.50727"))
                {
                    if (registryKey5 != null)
                    {
                        return "2.0";
                    }
                }
            }
            return "Unknown";
        }
        /// <summary>
        /// 获取当前.net版本
        /// </summary>
        /// <param name="v4Key"></param>
        /// <returns></returns>
        private static string GetFrameworkVersionAfter4(RegistryKey v4Key)
        {
            using (RegistryKey registryKey = v4Key.OpenSubKey("Full"))
            {
                if (registryKey != null)
                {
                    object value = registryKey.GetValue("Release");
                    if (value != null)
                    {
                        int num = Convert.ToInt32(value);
                        if (num > 393273)
                        {
                            return ">4.6RC";
                        }
                        if (num == 393273)
                        {
                            return "4.6RC";
                        }
                        if (num >= 379893)
                        {
                            return "4.5.2";
                        }
                        if (num >= 378675)
                        {
                            return "4.5.1";
                        }
                        if (num >= 378389)
                        {
                            return "4.5";
                        }
                    }
                }
            }
            return "4.0";
        }
        /// <summary>
        /// 获取或设置与 System.Net.ServicePoint 对象关联的连接在被关闭前可以持续空闲的时间
        /// </summary>
        private static int DefaultMaxIdleTimeInMillis
        {
            get
            {
                if (ServicePointManager.MaxServicePointIdleTime == 100000)
                {
                    return 50000;
                }
                return ServicePointManager.MaxServicePointIdleTime;
            }
        }
        /// <summary>
        /// 获取或设置此 System.Net.ServicePoint 对象上允许的最大连接数。
        /// </summary>
        private static int DefaultConnectionLimit
        {
            get
            {
                if (ServicePointManager.DefaultConnectionLimit == 2)
                {
                    return 50;
                }
                return ServicePointManager.DefaultConnectionLimit;
            }
        }
    }
}
