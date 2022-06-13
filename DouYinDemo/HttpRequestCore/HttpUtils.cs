using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestCore
{
    // Token: 0x02000056 RID: 86
    public static class HttpUtils
    {
        // Token: 0x06000215 RID: 533 RVA: 0x00006EFC File Offset: 0x000050FC
        static HttpUtils()
        {
            for (char c = 'a'; c <= 'z'; c += '\u0001')
            {
                HttpUtils.PercentEncodedStrings[(int)c] = c.ToString();
            }
            for (char c2 = 'A'; c2 <= 'Z'; c2 += '\u0001')
            {
                HttpUtils.PercentEncodedStrings[(int)c2] = c2.ToString();
            }
            for (char c3 = '0'; c3 <= '9'; c3 += '\u0001')
            {
                HttpUtils.PercentEncodedStrings[(int)c3] = c3.ToString();
            }
            HttpUtils.PercentEncodedStrings[45] = "-";
            HttpUtils.PercentEncodedStrings[46] = ".";
            HttpUtils.PercentEncodedStrings[95] = "_";
            HttpUtils.PercentEncodedStrings[126] = "~";
        }

        // Token: 0x06000216 RID: 534 RVA: 0x00006FCB File Offset: 0x000051CB
        public static string NormalizePath(string path)
        {
            return HttpUtils.Normalize(path).Replace("%2F", "/");
        }

        // Token: 0x06000217 RID: 535 RVA: 0x00006FE4 File Offset: 0x000051E4
        public static string Normalize(string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in Encoding.UTF8.GetBytes(value))
            {
                stringBuilder.Append(HttpUtils.PercentEncodedStrings[(int)(b & byte.MaxValue)]);
            }
            return stringBuilder.ToString();
        }

        // Token: 0x06000218 RID: 536 RVA: 0x00007030 File Offset: 0x00005230
        public static string GenerateHostHeader(Uri uri)
        {
            string text = uri.Host;
            if (HttpUtils.IsUsingNonDefaultPort(uri))
            {
                text = text + ":" + uri.Port;
            }
            return text;
        }

        // Token: 0x06000219 RID: 537 RVA: 0x00007064 File Offset: 0x00005264
        public static bool IsUsingNonDefaultPort(Uri uri)
        {
            string a = uri.Scheme.ToLower();
            int port = uri.Port;
            if (port <= 0)
            {
                return false;
            }
            if (a == "http")
            {
                return port != 80;
            }
            return a == "https" && port != 443;
        }

        /// <summary>
        /// 字符串拼接成url地址
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="forSignature"></param>
        /// <returns></returns>
        public static string GetCanonicalQueryString(IDictionary<string, string> parameters, bool forSignature)
        {
            if (parameters.Count == 0)
            {
                return "";
            }
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> keyValuePair in parameters)
            {
                string key = keyValuePair.Key;
                if (!forSignature || !"Authorization".Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    if (key == null)
                    {
                        throw new ArgumentNullException("parameter key should NOT be null");
                    }
                    string value = keyValuePair.Value;
                    if (value == null)
                    {
                        if (forSignature)
                        {
                            list.Add(HttpUtils.Normalize(key) + '=');
                        }
                        else
                        {
                            list.Add(HttpUtils.Normalize(key));
                        }
                    }
                    else
                    {
                        list.Add(HttpUtils.Normalize(key) + '=' + HttpUtils.Normalize(value));
                    }
                }
            }
            list.Sort();
            return string.Join("&", list.ToArray());
        }
        /// <summary>
        /// 字符串拼接成url地址
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="forSignature"></param>
        /// <returns></returns>
        public static string GetPostParameters(IDictionary<string, string> parameters)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> keyValuePair in parameters)
            {
                string key = keyValuePair.Key;
                string value = keyValuePair.Value;
                if (value != null)
                {
                    list.Add(key + '=' + value);
                }
            }
            return string.Join("&", list.ToArray());
        }

        // Token: 0x0600021B RID: 539 RVA: 0x000071B0 File Offset: 0x000053B0
        public static string AppendUri(string baseUri, params string[] pathComponents)
        {
            if (pathComponents.Length == 0)
            {
                return baseUri;
            }
            StringBuilder stringBuilder = new StringBuilder(baseUri.ToString().TrimEnd(new char[]
            {
                '/'
            }));
            for (int i = 0; i < pathComponents.Length; i++)
            {
                string text = pathComponents[i];
                if (!string.IsNullOrEmpty(text))
                {
                    if (i < pathComponents.Length - 1)
                    {
                        text = text.TrimEnd(new char[]
                        {
                            '/'
                        });
                    }
                    if (!text.StartsWith("/"))
                    {
                        stringBuilder.Append('/');
                    }
                    stringBuilder.Append(HttpUtils.NormalizePath(text));
                }
            }
            return stringBuilder.ToString();
        }

        // Token: 0x0400013F RID: 319
        private static readonly string[] PercentEncodedStrings = (from v in Enumerable.Range(0, 256)
                                                                  select "%" + v.ToString("X2")).ToArray<string>();
    }
}
