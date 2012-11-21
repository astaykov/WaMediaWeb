using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaMedia.Common.Models;

namespace WaMedia.Common.Contracts
{
    public interface ILocatorService
    {
        IMediaService MediaService { get; }
        string GetSmoothStreamingOriginLocator(Asset assetToStream);
        string GetHLSOriginLocator(Asset assetToStream);
        string GetMp4StreamingOriginLocator(Asset assetToStream);
        string GetSmoothStreamingAzureCDNLocator(Models.Asset assetToStream);
        string GetSasLocator(Asset asset);
    }
}
