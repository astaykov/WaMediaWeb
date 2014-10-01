using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using WaMedia.Common.Contracts;

namespace WaMedia.Common.Implementations
{
    public class MediaService : IMediaService
    {
        private static readonly string _mediaAccount;
        private static readonly string _mediaKey;

        static MediaService()
        {
            _mediaAccount = CloudConfigurationManager.GetSetting("MediaAccount");
            _mediaKey = CloudConfigurationManager.GetSetting("MediaKey");
        }

        public MediaService()
        {
            this._mediaContext = new CloudMediaContext(_mediaAccount, _mediaKey);
           
        }

        public MediaService(CloudMediaContext ctx)
        {
            this._mediaContext = ctx;
        }

        private CloudMediaContext _mediaContext;
        public CloudMediaContext MediaContext
        {
            get { return this._mediaContext; }
        }

        public void Reset()
        {
            List<IJob> jobs = this.MediaContext.Jobs.ToList();
            List<IAsset> assets = this.MediaContext.Assets.ToList();
            List<ILocator> locators = this.MediaContext.Locators.ToList();
            List<IContentKey> keys = this.MediaContext.ContentKeys.ToList();
            foreach (var loc in locators)
            {
                loc.Delete();
            }
            foreach (var job in jobs)
            {
                job.Delete();
            }
            foreach (var asset in assets)
            {
                var assetKeys = asset.ContentKeys.ToList();
                foreach (var key in assetKeys)
                {
                    asset.ContentKeys.Remove(key);
                }
                asset.Update();
                asset.Delete();
            }
           
            //// just don't delete the keys! Your media account will not work anymore
            //foreach (var key in keys)
            //{
            //    this.MediaContext.ContentKeys.Delete(key);
            //}
            //this.MediaContext.ContentKeys.Create(Guid.NewGuid(),
            //    System.Text.Encoding.UTF8.GetBytes("0123456789123456"), "new content key");
            
        }

        public IMediaProcessor GetMediaProcessorByName(string name)
        {
            // Query for a media processor to get a reference.
            // Get the latest in version
            Microsoft.WindowsAzure.MediaServices.Client.IMediaProcessor processor =
                (from p in this.MediaContext.MediaProcessors where p.Name == name select p)
                .ToList()
                .OrderBy(wame => new Version(wame.Version))
                .LastOrDefault();
            if (processor == null)
            {
                throw new ArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    "Unknown processor",
                    name));
            }
            return processor;
        }
    }
}
