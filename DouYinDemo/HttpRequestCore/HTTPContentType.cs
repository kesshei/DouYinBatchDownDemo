using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestCore
{
    /// <summary>
    /// 互联网媒体类型
    /// </summary>
    public class HTTPContentType
    {
        /// <summary>
        /// HTML格式
        /// </summary>
        public const string HTML = "text/html";
        /// <summary>
        /// 纯文本格式     
        /// </summary>
        public const string TXT = "text/plain";
        /// <summary>
        /// Json
        /// </summary>
        public const string Json = "application/json";
        /// <summary>
        /// 二进制流数据（如常见的文件下载）
        /// </summary>
        public const string Byte = "application/octet-stream";
        /// <summary>
        /// form表单数据被编码为key/value格式发送到服务器
        /// </summary>
        public const string Form = "application/x-www-form-urlencoded";

    }
}
