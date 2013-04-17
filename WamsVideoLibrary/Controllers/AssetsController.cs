using System.Linq;
using System.Web.Mvc;
using WaMedia.Common;
using WaMediaWeb.Models;

namespace WaMediaWeb.Controllers
{
    public class AssetsController : BaseMediaController
    {
        //
        // GET: /Assets/

        public ActionResult Index()
        {
            var model = this.AssetService.Assets.OrderByDescending(a => a.MediaAsset.LastModified).ToList();
            return View(model);
        }

        public ActionResult CreateMediaAsset()
        {
            var tmpName = Server.MapPath("~/tmpuploads");
            if (!System.IO.Directory.Exists(tmpName))
            {
                System.IO.Directory.CreateDirectory(tmpName);
            }

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
            return Json(new { success = true });
        }

        public ActionResult CreateEmptyAsset(string name)
        {
            this.AssetService.CreateEmptyAsset(name);
            return RedirectToAction("index");
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
                return HttpNotFound();
            }
            return View(asset);
        }

        public ActionResult GetStreamingUrl(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            string streamingUrl = this.LocatorService.GetSmoothStreamingOriginLocator(asset);
            if (string.IsNullOrWhiteSpace(streamingUrl))
            {
                return HttpNotFound();
            }
            return View(new StreamingUrlViewModel() { Url = streamingUrl, IsMp4Progressive = false });

        }

        public ActionResult GetHlsStreamingUrl(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            string streamingUrl = this.LocatorService.GetHLSOriginLocator(asset);
            if (string.IsNullOrWhiteSpace(streamingUrl))
            {
                return HttpNotFound();
            }
            return View("GetStreamingUrl", new StreamingUrlViewModel() { Url = streamingUrl, IsMp4Progressive = false });
        }

        public ActionResult GetCDNStreamingUrl(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            string streamingUrl = this.LocatorService.GetSmoothStreamingAzureCDNLocator(asset);
            if (string.IsNullOrWhiteSpace(streamingUrl))
            {
                return HttpNotFound();
            }
            return View("GetStreamingUrl", new StreamingUrlViewModel() { Url = streamingUrl });

        }

        public ActionResult GetMp4StreamingUrl(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            string streamingUrl = this.LocatorService.GetMp4StreamingOriginLocator(asset);
            StreamingUrlViewModel model = new StreamingUrlViewModel();
            model.Url = streamingUrl;
            model.IsMp4Progressive = true;
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
            this.JobService.CreateNewJob(asset, MediaEncoders.WINDOWS_AZURE_MEDIA_ENCODER, Tasks.H264_HD_720P_CBR);
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult CreateThumbnails(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            this.JobService.CreateThumbnails(asset);
            //this.JobService.CreateNewJob(asset, MediaEncoders.WINDOWS_AZURE_MEDIA_ENCODER, Tasks.H264_512k_DSL_CBR);
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult ConvertToPlayReady(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            this.JobService.CreateNewJob(asset, MediaEncoders.WINDOWS_AZURE_MEDIA_ENCRYPTOR, this.JobService.GetPlayReadyTask(keySeed: PlayReady.DEV_SERVER_KEY_SEED, playReadyServerUrl: PlayReady.DEV_SERVER_LICENSE_URL));
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult ConvertToHls(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            this.JobService.CreateNewJob(asset, MediaEncoders.WINDOWS_AZURE_MEDIA_PACKAGER, this.JobService.GetSmoothToHlsTask());
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult ConvertToMultiBitrateMp4(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            this.JobService.CreateNewJob(asset, MediaEncoders.WINDOWS_AZURE_MEDIA_ENCODER, Tasks.H264_ADAPTIVE_BITRATE_SD_16x9);
            return RedirectToAction("Index", "Jobs");
        }

        public ActionResult Mp4ToSmooth(string assetId)
        {
            var asset = this.AssetService.GetAssetById(assetId);
            this.JobService.CreateNewJob(asset, MediaEncoders.WINDOWS_AZURE_MEDIA_ENCODER, Tasks.H264_Smooth_720p_3G_4G);
            //this.JobService.CreateNewJob(asset, MediaEncoders.WINDOWS_AZURE_MEDIA_PACKAGER, this.JobService.GetMp4ToSmoothTask());
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

        public ActionResult GetSasUrl(string assetId)
        {
            var sasLoc = this.LocatorService.GetSasLocator(this.AssetService.GetAssetById(assetId));
            return View((object)sasLoc);
        }

        public ActionResult CopyFromBlob(string assetId, string srcBlob)
        {
            var src = this.LocatorService.GetSasLocator(this.AssetService.GetAssetById(assetId));
            this.AssetService.CopyFromBlob(src, srcBlob);
            return RedirectToAction("Details", new { assetId = assetId });
        }
    }
}
