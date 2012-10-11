using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WaMedia.Common.Models
{
    public class Asset
    {
        public IAsset MediaAsset { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
