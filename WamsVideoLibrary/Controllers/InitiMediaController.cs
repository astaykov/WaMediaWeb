using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WamsVideoLibrary.Controllers
{
    public class InitMediaController : Controller
    {
        //
        // GET: /InitiMedia/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Init(string account, string key)
        {
            Session["mctx"] = new CloudMediaContext(account, key);
            return RedirectToAction("Index", "Home");
        }
    }
}
