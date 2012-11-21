using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WaMediaWeb.Models
{
    public class StreamingUrlViewModel
    {
        public string Url { get; set; }
        public bool IsMp4Progressive { get; set; }
    }
}