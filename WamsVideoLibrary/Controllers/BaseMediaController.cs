using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WaMedia.Common.Implementations;
using WaMedia.Common.Contracts;

namespace WaMediaWeb.Controllers
{
    public class BaseMediaController : Controller
    {
        private IMediaService _mediaService;
        private IAssetService _assetSErvice;
        private IJobService _jobService;
        private ILocatorService _locatorService;

        public BaseMediaController()
        {
            this._mediaService = new MediaService();
            this._assetSErvice = new AssetService(this.MediaService);
            this._jobService = new JobService(this.MediaService);
            this._locatorService = new LocatorService(this.MediaService);
        }

        protected IMediaService MediaService
        {
            get
            {
                return this._mediaService;
            }
        }

        protected IAssetService AssetService
        {
            get { return this._assetSErvice; }
        }

        protected IJobService JobService
        {
            get
            {
                return this._jobService;
            }
        }

        protected ILocatorService LocatorService
        {
            get { return this._locatorService; }
        }

    }
}
