using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WaMedia.Common
{
    public static class Extensions
    {
        [Obsolete("It's useless unless we find a way to add the generated thumbnail to the main asset")]
        public static bool HasThumbnail(this IAsset src)
        {
            
            var file = (from f in src.Files where f.Name.EndsWith(".jpg") && !f.IsEncrypted select f).FirstOrDefault();
            return (file != null);
        }
    }
}
