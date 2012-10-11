using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaMedia.Common.Contracts;
using WaMedia.Common.Models;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WaMedia.Common.Implementations
{
    public class AssetService : IAssetService
    {
        private IMediaService _mediaService;
        public AssetService(IMediaService mediaService)
        {
            this._mediaService = mediaService;
        }
        public IMediaService MediaService
        {
            get { return this._mediaService; }
        }

        public IQueryable<Models.Asset> Assets
        {
            get {
                var result = (
                    from a in this.MediaService.MediaContext.Assets 
                    select a).ToList();
                return result.Select( a => new Asset { MediaAsset = a, ThumbnailUrl = "" }).AsQueryable();
            }
        }

        public Models.Asset GetAssetById(string assetId)
        {
            var assetQuery = from a in this.MediaService.MediaContext.Assets
                             where a.Id.Equals(assetId)
                             select a;
            var iAsset = assetQuery.FirstOrDefault();
            if (iAsset != null)
            {
                return new Asset { MediaAsset = iAsset, ThumbnailUrl = "" };
            }
            return null;
        }

        [Obsolete("Don't use! There must be a smarter way to do that!")]
        public string ThumbnailUrl(IAsset asset)
        {
            if (asset == null)
            {
                return null;
            }
            var src = asset;
            if (asset.State == AssetState.Initialized)
            {
                asset.Publish();
            }
            var file = (from f in src.Files where f.Name.EndsWith(".jpg") && f.IsPrimary && !f.IsEncrypted select f).FirstOrDefault();
            if (file == null)
            {
                return null;
            }
            // Create an 10-day readonly access policy. 
            IAccessPolicy streamingPolicy = this.MediaService.MediaContext.AccessPolicies.Create("Thumbnail Policy",
                TimeSpan.FromDays(10),
                AccessPermissions.Read);

            // Create the origin locator. Set the start time as 5 minutes 
            // before the present so that the locator can be accessed immediately 
            // if there is clock skew between the client and server.
            ILocator originLocator =
                (from l in this.MediaService.MediaContext.Locators
                 where l.AssetId.Equals(file.Asset.Id)
                 select l).FirstOrDefault();

            if (originLocator == null)
            {
                originLocator = this.MediaService.MediaContext.Locators.CreateSasLocator(file.Asset,
                 streamingPolicy,
                 DateTime.UtcNow.AddMinutes(-5));
            }
            // Create a full URL to the manifest file. Use this for playback
            // in streaming media clients. 
            return originLocator.Path + file.Name;
        }

        public void CreateAsset(string pathToFile)
        {
            this.MediaService.MediaContext.Assets.Create(pathToFile, AssetCreationOptions.None);
        }


        public void Publish(string assetId)
        {
            var asset = this.GetAssetById(assetId);
            if (asset != null)
            {
                asset.MediaAsset.Publish();
            }
        }


        public void AssignThumbnail(string assetId)
        {
            var asset = this.GetAssetById(assetId);
            var files = (
                from a in this.MediaService.MediaContext.Assets
                    where a.ParentAssets.Contains(asset.MediaAsset)
                     && a.Files.Count > 0
                     && a.Files.Where( f => f.Name.EndsWith(".jpg") && f.IsPrimary).FirstOrDefault() != null
                select a.Files.Where(f => f.Name.EndsWith(".jpg") && f.IsPrimary).FirstOrDefault()
                    );
            var file = files.FirstOrDefault();
            if (file != null)
            {
                var tmpPath = System.IO.Path.GetTempPath();
                var tmpName = System.IO.Path.Combine(tmpPath, file.Name);
                file.DownloadToFile(tmpName);
            }
        }
    }
}
