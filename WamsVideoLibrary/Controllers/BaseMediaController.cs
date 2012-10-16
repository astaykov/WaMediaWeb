using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WaMedia.Common.Implementations;
using WaMedia.Common.Contracts;
using WamsVideoLibrary;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WaMediaWeb.Controllers
{
    public class BaseMediaController : Controller
    {
        private IMediaService _mediaService;
        private IAssetService _assetSErvice;
        private IJobService _jobService;
        private ILocatorService _locatorService;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            if (this._mediaService == null)
            {
                this._mediaService = new MediaService((CloudMediaContext)requestContext.HttpContext.Application["mctx"]);
                this._assetSErvice = new AssetService(this.MediaService);
                this._jobService = new JobService(this.MediaService);
                this._locatorService = new LocatorService(this.MediaService);
            }
            base.Initialize(requestContext);
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
