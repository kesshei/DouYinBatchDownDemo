using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouYinDemo.Model
{
    public class VideoInfo
    {
        public string Title { get; set; }
        public string Vid { get; set; }
        public List<string> Urls { get; set; } = new List<string>();
        public List<string> Videos { get; set; } = new List<string>();
        public List<string> tags { get; set; } = new List<string>();
    }
}
