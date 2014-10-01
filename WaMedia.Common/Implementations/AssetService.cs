using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Linq;
using WaMedia.Common.Contracts;
using WaMedia.Common.Models;

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
            get
            {
                return this.MediaService.MediaContext.Assets.ToList().Select(a => new Asset { MediaAsset = a, ThumbnailUrl = "" }).AsQueryable();
            }
        }

        public Models.Asset GetAssetById(string assetId)
        {
            var asset = this.MediaService.MediaContext.Assets.Where(x => x.Id.Equals(assetId)).FirstOrDefault();

            if (null == asset)
            {
                return null;
            }
            return new Asset { MediaAsset = asset, ThumbnailUrl = "" };
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
                // NO PUBLISH anymore!?
            }
            var file = (from f in src.AssetFiles where f.Name.EndsWith(".jpg") && f.IsPrimary && !f.IsEncrypted select f).FirstOrDefault();
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

        /// <summary>
        /// Creates the asset. Note, there is a breaking change in the new SDK!
        /// You have to first create the Asset, which is always empty.
        /// Then create a file within that asset, then upload a content the newly created file
        /// </summary>
        /// <param name="pathToFile">The path to file.</param>
        public void CreateAsset(string pathToFile)
        {
            IAsset newAsset = this.MediaService.MediaContext.Assets.Create(System.IO.Path.GetFileName(pathToFile), AssetCreationOptions.None);
            // note, the file to be uploaded must match (case insensitive)
            // to the Name property of the IAssetFile instance we are uploading to
            IAssetFile file = newAsset.AssetFiles.Create(System.IO.Path.GetFileName(pathToFile));
            file.Upload(pathToFile);
        }

        public void Publish(string assetId)
        {
            var asset = this.GetAssetById(assetId);
            if (asset != null)
            {
               // asset.MediaAsset.Publish();
                // no Publish in the new SDK, what to do now?
            }
        }


        public void AssignThumbnail(string assetId)
        {
            var asset = this.GetAssetById(assetId);
            var files = (
                from a in this.MediaService.MediaContext.Assets
                where a.ParentAssets.Contains(asset.MediaAsset)
                 && a.AssetFiles.Count() > 0
                 && a.AssetFiles.Where(f => f.Name.EndsWith(".jpg") && f.IsPrimary).FirstOrDefault() != null
                select a.AssetFiles.Where(f => f.Name.EndsWith(".jpg") && f.IsPrimary).FirstOrDefault()
                    );
            var file = files.FirstOrDefault();
            if (file != null)
            {
                var tmpPath = System.IO.Path.GetTempPath();
                var tmpName = System.IO.Path.Combine(tmpPath, file.Name);
                file.Download(tmpName);
            }
        }


        public void DeleteAsset(string assetId)
        {
            var asset = this.GetAssetById(assetId);

            foreach (var locator in this.MediaService.MediaContext.Locators.Where(l => l.AssetId.Equals(assetId)))
            {
                locator.Delete();
            }
            for (int i = 0; i < asset.MediaAsset.ContentKeys.Count; i++)
            {
                asset.MediaAsset.ContentKeys.RemoveAt(0);
            }
            asset.MediaAsset.Update();
            asset.MediaAsset.Delete();
        }

        public void Rename(string assetId, string newName)
        {
            var asset = this.GetAssetById(assetId);
            if (asset != null)
            {
                asset.MediaAsset.Name = newName;
                asset.MediaAsset.Update();
            }
        }

        public void CreateEmptyAsset(string name)
        {
            this.MediaService.MediaContext.Assets.Create(name, AssetCreationOptions.None);
        }


        public void CopyFromBlob(string destinationSasUri, string srcBlobSasUri)
        {
            CloudBlockBlob blob = new CloudBlockBlob(srcBlobSasUri);
            string fileName = (blob.Name.Contains("/")) 
                ? blob.Name.Substring(blob.Name.LastIndexOf("/"))
                : blob.Name;
            CloudBlobContainer cbc = new CloudBlobContainer(destinationSasUri);

            //UriBuilder ub = new UriBuilder(destUri);
            //ub.Path += "/" + fileName;
            //CloudBlockBlob destBlob = new CloudBlockBlob(ub.Uri);
            CloudBlockBlob destBlob = cbc.GetBlockBlobReference(fileName);
            BlobRequestOptions bro = new BlobRequestOptions();
            bro.RetryPolicy =  RetryPolicies.RetryExponential(5, TimeSpan.FromMilliseconds(150));
            destBlob.BeginCopyFromBlob(blob, bro, (result) => {  }, null);


           // destBlob.UploadFromStream(System.IO.File.OpenRead(@"D:\Install.txt"));

            System.Diagnostics.Debug.WriteLine(destBlob.Name);
        }
    }
}
