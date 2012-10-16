using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WaMediaWeb.Models;
using WaMedia.Common;
using System.Dynamic;

namespace WaMediaWeb.Controllers
{
    public class AssetsController : BaseMediaController
    {
        //
        // GET: /Assets/

        public ActionResult Index()
        {
            return View(this.AssetService.Assets.OrderByDescending(a => a.MediaAsset.LastModified).ToList());
        }

        public ActionResult CreateMediaAsset()
        {
            var tmpName = System.IO.Path.GetTempPath();
            string pathToTempFile = System.IO.Path.Combine(tmpName, "Unknown");
            if (Request.Files.Count > 0)
            {
                var file = Request.Files.Get(0);
                pathToTempFile = System.IO.Path.Combine(tmpName, file.FileName);
                file.SaveAs(pathToTempFile);
            }
            else
            {
                pathToTempFile = System.IO.Path.Combine(tmpName, Request.Params["qqfile"]);
                using (var fs = System.IO.File.Create(pathToTempFile))
                {
                    Request.InputStream.CopyTo(fs);
                    fs.Flush();
                }
            }
            this.AssetService.CreateAsset(pathToTempFile);
            System.IO.File.Delete(pathToTempFile);
            return Content(@"{""success"": true}");
        }

        public ActionResult EncodeAndConvert(string assetId)
        {
            var assetToEncode = this.AssetService.GetAssetById(assetId);
            this.JobService.CreateEncodeToSmoothStreamingJob(assetToEncode);
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult Details(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            if (asset == null)
            {
                return RedirectToAction("Index");
            }
            return View(asset);
        }

        public ActionResult GetStreamingUrl(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            string streamingUrl = this.LocatorService.GetSmoothStreamingOriginLocator(asset);
            StreamingUrlViewModel model = new StreamingUrlViewModel();
            model.Url = streamingUrl;
            return View(model);
        }

        public ActionResult GetMp4StreamingUrl(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            string streamingUrl = this.LocatorService.GetMp4StreamingOriginLocator(asset);
            StreamingUrlViewModel model = new StreamingUrlViewModel();
            model.Url = streamingUrl;
            return View("GetStreamingUrl", model);
        }



        public ActionResult DecryptAsset(string assetId)
        {
            this.JobService.DecryptAsset(this.AssetService.GetAssetById(assetId));
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult PublishAsset(string assetId)
        {
            this.AssetService.Publish(assetId);
            return RedirectToAction("Index");
        }

        public ActionResult ConvertToMp4(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            this.JobService.CreateNewJob(asset, MediaEncoders.WINDOWS_AZURE_MEDIA_ENCODER, Tasks.H264_HD_720P_VBR);
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult ConvertToPlayReady(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            this.JobService.CreateNewJob(asset, MediaEncoders.PLAY_READY_ENCODER, this.JobService.GetPlayReadyTask(keySeed: PlayReady.DEV_SERVER_KEY_SEED, playReadyServerUrl: PlayReady.DEV_SERVER_LICENSE_URL));
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult DeleteAsset(string assetId)
        {
            this.AssetService.DeleteAsset(assetId);
            return RedirectToAction("Index");
        }

        public ActionResult Rename(string assetId, string newName)
        {
            this.AssetService.Rename(assetId, newName);
            return RedirectToAction("Details", new { assetId = assetId });
        }
    }
}
