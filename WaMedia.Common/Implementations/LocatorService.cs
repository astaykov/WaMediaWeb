using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaMedia.Common.Contracts;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WaMedia.Common.Implementations
{
    public class LocatorService : ILocatorService
    {
        private IMediaService _mediaService;
        public LocatorService(IMediaService mediaService)
        {
            this._mediaService = mediaService;
        }

        public IMediaService MediaService
        {
            get { return this._mediaService; }
        }

        public string GetSmoothStreamingOriginLocator(Models.Asset assetToStream)
        {
            // Get a reference to the manifest file from the collection 
            // of streaming files in the asset. 
            var theManifest =
                                from f in assetToStream.MediaAsset.Files
                                where f.Name.EndsWith(".ism")
                                select f;
            // Cast the reference to a true IFileInfo type. 
            IFileInfo manifestFile = theManifest.First();

            // Create an 1-day readonly access policy. 
            IAccessPolicy streamingPolicy = this.MediaService.MediaContext.AccessPolicies.Create("Streaming policy",
                TimeSpan.FromDays(1),
                AccessPermissions.Read);

            // Create the origin locator. Set the start time as 5 minutes 
            // before the present so that the locator can be accessed immediately 
            // if there is clock skew between the client and server.
            ILocator originLocator =
                (from l in this.MediaService.MediaContext.Locators
                 where l.AssetId.Equals(assetToStream.MediaAsset.Id)
                 select l).FirstOrDefault();

            if (originLocator == null)
            {
                originLocator = this.MediaService.MediaContext
                    .Locators.CreateOriginLocator(assetToStream.MediaAsset,
                 streamingPolicy,
                 DateTime.UtcNow.AddMinutes(-5));
            }
            // Create a full URL to the manifest file. Use this for playback
            // in streaming media clients. 
            string urlForClientStreaming = originLocator.Path + manifestFile.Name + "/manifest";

            // Display the full URL to the streaming manifest file.
            Console.WriteLine("URL to manifest for client streaming: ");
            Console.WriteLine(urlForClientStreaming);

            return urlForClientStreaming;
        }


        public string GetMp4StreamingOriginLocator(Models.Asset assetToStream)
        {
            // Get a reference to the manifest file from the collection 
            // of streaming files in the asset. 
            var theManifest =
                                from f in assetToStream.MediaAsset.Files
                                where f.Name.EndsWith(".mp4")
                                select f;
            // Cast the reference to a true IFileInfo type. 
            IFileInfo manifestFile = theManifest.First();

            // Create an 1-day readonly access policy. 
            IAccessPolicy streamingPolicy = this.MediaService.MediaContext.AccessPolicies.Create("Readonly 1 hour policy",
                TimeSpan.FromHours(1),
                AccessPermissions.Read);

            // Create the origin locator. Set the start time as 5 minutes 
            // before the present so that the locator can be accessed immediately 
            // if there is clock skew between the client and server.
            ILocator originLocator =
                (from l in this.MediaService.MediaContext.Locators
                 where l.AssetId.Equals(assetToStream.MediaAsset.Id)
                 select l).FirstOrDefault();

            if (originLocator == null)
            {
                originLocator = this.MediaService.MediaContext
                    .Locators.CreateSasLocator(assetToStream.MediaAsset,
                 streamingPolicy,
                 DateTime.UtcNow.AddMinutes(-5));
            }
            // Create a full URL to the manifest file. Use this for playback
            // in streaming media clients. 
            string urlForClientStreaming = originLocator.Path + manifestFile.Name;

            // Display the full URL to the streaming manifest file.
            Console.WriteLine("URL to for progressive download: ");
            Console.WriteLine(urlForClientStreaming);

            return urlForClientStreaming;
        }
    }
}
