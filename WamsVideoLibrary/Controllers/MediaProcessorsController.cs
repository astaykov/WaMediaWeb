using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WaMediaWeb.Controllers
{
    public class MediaProcessorsController : BaseMediaController
    {
        //
        // GET: /MediaProcessors/

        public ActionResult Index()
        {
            return View(this.MediaService.MediaContext.MediaProcessors);
        }

    }
}
