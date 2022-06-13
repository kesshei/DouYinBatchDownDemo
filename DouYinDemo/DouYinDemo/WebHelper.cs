using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace DouYinDemo
{
    /// <summary>
    /// 网络辅助功能帮助类
    /// </summary>
    public class WebHelper
    {
        /// <summary>
        /// 下载网络文件到本地
        /// </summary>
        /// <param name="Url">网络文件URL</param>
        /// <param name="thisAddress">本地文件 加上文件名和后缀</param>
        /// <returns>返回真假</returns>
        public static Boolean DownloadFile(string Url, string thisAddress)
        {
            WebClient webclient = new WebClient();
            try
            {
                webclient.DownloadFile(Url, thisAddress);
            }
            catch (Exception ex) { return false; }
            return true;
        }
    }
}
