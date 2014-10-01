using Microsoft.WindowsAzure.MediaServices.Client;

namespace WaMedia.Common.Models
{
    public class Asset
    {
        public IAsset MediaAsset { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
